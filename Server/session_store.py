"""Single-process demo sessions. Existing logs are never reused or truncated."""
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
import threading
from uuid import UUID, uuid4


class SessionError(Exception):
    def __init__(self, status_code: int, message: str):
        super().__init__(message)
        self.status_code = status_code


@dataclass
class _Session:
    log_path: Path
    closed: bool = False
    busy: bool = False
    end_recorded: bool = False


class Turn:
    def __init__(self, store, session):
        self._store = store
        self._session = session
        self._released = False
        self.log_path = session.log_path

    def release(self):
        # Both stream-finally and response cleanup may call this.
        with self._store._lock:
            if self._released:
                return
            self._released = True
            self._session.busy = False
            if self._session.closed:
                self._store._record_end(self._session)


class SessionStore:
    def __init__(self, log_dir=None):
        self.log_dir = Path(log_dir) if log_dir is not None else (
            Path(__file__).resolve().parent / "logs" / "sessions"
        )
        self._sessions = {}
        self._lock = threading.Lock()

    def create(self) -> str:
        with self._lock:
            self.log_dir.mkdir(parents=True, exist_ok=True)
            while True:
                session_id = str(uuid4())
                path = self.log_dir / (session_id + ".txt")
                try:
                    with path.open("x", encoding="utf-8") as f:
                        f.write("# simulation log\n")
                        f.write(f"# sessionId: {session_id}\n")
                        f.write(f"# started_at: {datetime.now().isoformat(timespec='seconds')}\n\n")
                except FileExistsError:
                    continue
                self._sessions[session_id] = _Session(path)
                return session_id

    def _get(self, session_id: str) -> _Session:
        try:
            valid = str(UUID(session_id)) == session_id
        except (ValueError, TypeError, AttributeError):
            valid = False
        if not valid:
            raise SessionError(400, "Invalid sessionId")
        session = self._sessions.get(session_id)
        if session is None:
            raise SessionError(404, "Unknown sessionId; start a new simulation")
        return session

    def begin_turn(self, session_id: str) -> Turn:
        with self._lock:
            session = self._get(session_id)
            if session.closed:
                raise SessionError(410, "Session has ended")
            if session.busy:
                raise SessionError(409, "A chat request is already in progress for this session")
            session.busy = True
            return Turn(self, session)

    def end(self, session_id: str):
        with self._lock:
            session = self._get(session_id)
            session.closed = True
            # Prevent new requests immediately; let the reserved turn finish.
            if not session.busy:
                self._record_end(session)

    def _record_end(self, session):
        if not session.end_recorded:
            with session.log_path.open("a", encoding="utf-8") as f:
                f.write(f"\n# ended_at: {datetime.now().isoformat(timespec='seconds')}\n")
            session.end_recorded = True
