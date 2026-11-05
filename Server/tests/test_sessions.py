"""Offline regression tests: fake OpenAI + fake dotenv, temporary session files only.
Run: PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover -s tests -v
"""
import asyncio
from contextlib import contextmanager
import hashlib
import os
from pathlib import Path
import sys
import tempfile
import threading
import types
import unittest
from unittest.mock import patch
from uuid import uuid4

SERVER = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SERVER))


class FakeResponses:
    def __init__(self):
        self.calls = []
        self.started = threading.Event()
        self.resume = threading.Event()
        self.closed = 0

    @contextmanager
    def stream(self, **kwargs):
        self.calls.append(kwargs)
        msg = kwargs['input'][-1]['content']
        if msg == 'slow':
            self.started.set()
            if not self.resume.wait(5):
                raise RuntimeError('test timeout')
        if msg == 'fail':
            raise RuntimeError('simulated SDK failure')
        try:
            yield iter([
                types.SimpleNamespace(type='response.created'),
                types.SimpleNamespace(type='response.output_text.delta', delta='reply:'),
                types.SimpleNamespace(type='response.output_text.delta', delta=msg),
            ])
        finally:
            self.closed += 1


fake_openai = types.ModuleType('openai')
fake_openai.OpenAI = lambda **kwargs: types.SimpleNamespace(responses=FakeResponses())
fake_dotenv = types.ModuleType('dotenv')
fake_dotenv.load_dotenv = lambda: None
# Never load the real dotenv or construct a real network client.
with patch.dict(sys.modules, {'openai': fake_openai, 'dotenv': fake_dotenv}), patch.dict(
        os.environ, {'OPENAI_API_KEY': 'offline-test-placeholder'}):
    import main
    import llm
    import session_store

from fastapi.testclient import TestClient


class SessionTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.store = session_store.SessionStore(Path(self.temp.name) / 'sessions')
        self.store_patch = patch.object(main, 'sessions', self.store)
        self.store_patch.start()
        self.fake = FakeResponses()
        self.client_patch = patch.object(llm, 'client', types.SimpleNamespace(responses=self.fake))
        self.client_patch.start()
        self.cwd = os.getcwd()
        os.chdir(SERVER)
        self.http = TestClient(main.app)
        self.legacy = {p: hashlib.sha256(p.read_bytes()).hexdigest()
                       for p in (SERVER / 'logs').glob('*.txt')}

    def tearDown(self):
        self.fake.resume.set()
        self.http.close()
        os.chdir(self.cwd)
        self.client_patch.stop()
        self.store_patch.stop()
        self.temp.cleanup()
        for p, digest in self.legacy.items():
            self.assertEqual(hashlib.sha256(p.read_bytes()).hexdigest(), digest)

    def start(self):
        response = self.http.post('/simulation/start')
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.json()['ok'])
        return response.json()['sessionId']

    def chat(self, sid, msg, **extra):
        payload = dict(sessionId=sid, message=msg, curRoom=0, truthLevel=15, gifted=False)
        payload.update(extra)
        return self.http.post('/chat/send', json=payload)

    def path(self, sid):
        return self.store.log_dir / (sid + '.txt')

    def test_a_sessions_isolate_files_and_actual_prompts(self):
        a, b = self.start(), self.start()
        self.chat(a, 'A-only')
        self.chat(b, 'B-only')
        self.assertEqual(llm.load_recent_dialog_pairs(self.path(a)), [('A-only', 'reply:A-only')])
        self.assertEqual(llm.load_recent_dialog_pairs(self.path(b)), [('B-only', 'reply:B-only')])
        self.chat(a, 'A-next')
        prompt = self.fake.calls[-1]['input']
        self.assertIn({'role': 'user', 'content': 'A-only'}, prompt)
        self.assertNotIn('B-only', str(prompt))
        self.chat(b, 'B-next')
        self.assertNotIn('A-only', str(self.fake.calls[-1]['input']))

    def test_b_restart_creates_empty_context_and_preserves_previous(self):
        a = self.start()
        self.chat(a, 'previous')
        before = self.path(a).read_bytes()
        b = self.start()
        self.assertNotEqual(a, b)
        self.assertEqual(llm.load_recent_dialog_pairs(self.path(b)), [])
        self.assertEqual(self.path(a).read_bytes(), before)

    def test_c_invalid_unknown_missing_and_closed_sessions(self):
        for sid, code in [('../current', 400), ('not-a-uuid', 400), (str(uuid4()), 404)]:
            self.assertEqual(self.chat(sid, 'blocked').status_code, code)
            self.assertEqual(self.http.post('/simulation/end', json={'sessionId': sid}).status_code, code)
        self.assertEqual(self.http.post('/chat/send', json={
            'message': 'blocked', 'curRoom': 0, 'truthLevel': 0, 'gifted': False
        }).status_code, 422)
        a = self.start()
        self.http.post('/simulation/end', json={'sessionId': a})
        self.assertEqual(self.chat(a, 'blocked').status_code, 410)
        self.assertEqual(self.fake.calls, [])

    def test_d_end_preserves_logs_and_does_not_touch_other_session(self):
        a, b = self.start(), self.start()
        self.chat(a, 'A')
        self.chat(b, 'B')
        before = self.path(b).read_bytes()
        self.assertEqual(self.http.post('/simulation/end', json={'sessionId': a}).status_code, 200)
        ended = self.path(a).read_bytes()
        self.assertIn(b'ASSISTANT: reply:A', ended)
        self.assertIn(b'# ended_at:', ended)
        self.http.post('/simulation/end', json={'sessionId': a})
        self.assertEqual(self.path(a).read_bytes(), ended)  # idempotent end
        self.assertEqual(self.path(b).read_bytes(), before)
        self.assertEqual(self.chat(b, 'still-open').status_code, 200)

    def test_e_five_pairs_prompt_roles_truth_and_stream_contract(self):
        a = self.start()
        for i in range(7):
            self.chat(a, f'question-{i}')
        self.assertEqual(len(llm.load_recent_dialog_pairs(self.path(a))), 5)
        response = self.chat(a, 'current', curRoom=2, truthLevel=50, gifted=True)
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.headers['content-type'].startswith('text/event-stream'))
        self.assertEqual(response.text, 'data: reply:\n\ndata: current\n\nevent: end\ndata: [DONE]\n\n')
        call = self.fake.calls[-1]
        self.assertEqual(call['model'], 'gpt-5-mini')
        self.assertEqual(call['reasoning'], {'effort': 'low'})
        self.assertEqual(call['text'], {'verbosity': 'low'})
        messages = call['input']
        self.assertEqual(messages[0], {'role': 'system', 'content': llm.load_prompt('general_rule.txt')})
        self.assertEqual(messages[1]['role'], 'developer')
        developer = messages[1]['content']
        self.assertIn(llm.load_prompt('developer_rule.txt'), developer)
        self.assertIn('True', developer)
        self.assertIn('tom_room', developer)
        self.assertIn(llm.load_prompt('tom_room.txt'), developer)
        truth = llm.ts.load_truth_for_level(llm.load_prompt('truth.txt'), 50)
        self.assertIn(truth, developer)
        self.assertEqual(messages[2:-1], [m for i in range(2, 7) for m in (
            {'role': 'user', 'content': f'question-{i}'},
            {'role': 'assistant', 'content': f'reply:question-{i}'})])
        self.assertEqual(messages[-1], {'role': 'user', 'content': 'current'})

    def test_overlapping_turn_rejected_and_end_waits_for_reserved_turn(self):
        a, b = self.start(), self.start()
        responses = []
        worker = threading.Thread(target=lambda: responses.append(self.chat(a, 'slow')))
        worker.start()
        try:
            self.assertTrue(self.fake.started.wait(3))
            self.assertEqual(self.chat(a, 'overlap').status_code, 409)
            self.assertEqual(self.chat(b, 'parallel-session').status_code, 200)
            self.assertEqual(self.http.post('/simulation/end', json={'sessionId': a}).status_code, 200)
            self.assertEqual(self.chat(a, 'after-end').status_code, 410)
            self.assertNotIn('# ended_at:', self.path(a).read_text())
        finally:
            self.fake.resume.set()
            worker.join(5)
        self.assertFalse(worker.is_alive())
        self.assertEqual(responses[0].status_code, 200)
        text = self.path(a).read_text()
        self.assertLess(text.index('ASSISTANT:'), text.index('# ended_at:'))
        self.assertNotIn('overlap', text)

    def test_sdk_failure_releases_reservation(self):
        a = self.start()
        with self.assertRaises(Exception):
            self.chat(a, 'fail')
        self.assertEqual(self.chat(a, 'recovered').status_code, 200)

    def test_stream_close_releases_reservation_and_closes_sdk(self):
        a = self.start()
        async def exercise():
            response = await main.chat_send(main.ChatIn(
                sessionId=a, message='partial', curRoom=0, truthLevel=0, gifted=False))
            await response.body_iterator.__anext__()
            await response.body_iterator.aclose()
            await response.background()  # second release is safe
        asyncio.run(exercise())
        self.assertEqual(self.fake.closed, 1)
        self.assertEqual(self.chat(a, 'next').status_code, 200)

    def test_old_cleanup_cannot_release_a_new_turn(self):
        a = self.start()
        first = self.store.begin_turn(a)
        first.release()
        second = self.store.begin_turn(a)
        first.release()
        with self.assertRaises(session_store.SessionError) as e:
            self.store.begin_turn(a)
        self.assertEqual(e.exception.status_code, 409)
        second.release()

    def test_restart_of_server_does_not_reuse_unregistered_files(self):
        a = self.start()
        self.chat(a, 'kept')
        before = self.path(a).read_bytes()
        restarted = session_store.SessionStore(self.store.log_dir)
        with self.assertRaises(session_store.SessionError) as e:
            restarted.begin_turn(a)
        self.assertEqual(e.exception.status_code, 404)
        b = restarted.create()
        self.assertNotEqual(a, b)
        self.assertEqual(self.path(a).read_bytes(), before)

    def test_uuid_collision_never_overwrites_an_existing_file(self):
        a = self.start()
        before = self.path(a).read_bytes()
        fresh = uuid4()
        with patch.object(session_store, 'uuid4', side_effect=[a, fresh]):
            b = self.store.create()
        self.assertEqual(b, str(fresh))
        self.assertEqual(self.path(a).read_bytes(), before)


if __name__ == '__main__':
    unittest.main()
