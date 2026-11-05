# llm.py
from openai import OpenAI
from dotenv import load_dotenv
import os
from datetime import datetime
import threading
import truth_selection as ts

load_dotenv()
client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

SYSTEM_PROMPT_PATH = "general_rule.txt"
DEVELOPER_PROMPT_PATH = "developer_rule.txt"
TRUTH_PATH = "truth.txt"

ROOM_FILE_MAP = {
    0: "basement.txt",
    1: "living_room.txt",
    2: "tom_room.txt",
    3: "james_room.txt",
}

# 간단한 동시성 보호(여러 요청이 동시에 append할 수 있어서)
_log_lock = threading.Lock()



def load_prompt(path: str) -> str:
    with open(path, "r", encoding="utf-8") as f:
        return f.read().strip()


def append_log(role: str, text: str, log_path: str) -> None:
    """검증된 세션의 로그에만 append. 경로는 session_store가 제공한다."""

    stamp = datetime.now().strftime("%H:%M:%S")
    line = f"[{stamp}] {role}: {text}\n"

    with _log_lock:
        with open(log_path, "a", encoding="utf-8") as f:
            f.write(line)


def generate_reply_stream(msg: str, curRoom: int, truthLevel: int, gifted: bool, log_path: str):
    general_rule = load_prompt(SYSTEM_PROMPT_PATH)
    developer = load_prompt(DEVELOPER_PROMPT_PATH)
    hint = load_prompt(ROOM_FILE_MAP[curRoom])
    location = ROOM_FILE_MAP[curRoom].split(".")[0]
    truth_text = load_prompt(TRUTH_PATH)
    truth = ts.load_truth_for_level(truth_text, truthLevel)
    prompt = f"""
    {developer}
    
    현재 생일 선물을 받았는가? {gifted}
    현재 위치는 {location}이다.

    {hint}

    [STORY]
    {truth}
    """
    history = format_pairs_for_prompt(
        load_recent_dialog_pairs(log_path, 5)
    )

    prompt_messages = [
        {"role": "system", "content": general_rule},
        {"role": "developer", "content": prompt},
        *history,
        {"role": "user", "content": msg},
    ]

    print("\n===== PROMPT DEBUG START =====")
    for m in prompt_messages:
        print(f"[{m['role'].upper()}]")
        print(m["content"])
        print()
    print("===== PROMPT DEBUG END =====\n")

    with client.responses.stream(
        model="gpt-5-mini",
        input=prompt_messages,
        reasoning={"effort": "low"},
        text={"verbosity": "low"},
    ) as stream:
        for event in stream:
            if event.type == "response.output_text.delta":
                yield event.delta


def generate_reply(msg: str, curRoom: int, truthLevel: int, gifted: bool, log_path: str):
    # user msg log
    append_log("USER", msg, log_path)

    # stream tokens while collecting full response
    chunks = []
    for delta in generate_reply_stream(msg, curRoom, truthLevel, gifted, log_path):
        chunks.append(delta)
        yield delta

    full_text = "".join(chunks).strip()
    append_log("ASSISTANT", full_text, log_path)


def format_pairs_for_prompt(pairs):
    """
    LLM input용 role 기반 메시지 배열로 변환
    """
    messages = []
    for user, assistant in pairs:
        messages.append({"role": "user", "content": user})
        messages.append({"role": "assistant", "content": assistant})
    return messages


def load_recent_dialog_pairs(path: str, pair_count: int = 5):
    """
    지정된 세션 로그에서 최근 user-assistant 대화 pair_count개를 가져온다.
    return: List[Tuple[str, str]]  # [(user, assistant), ...]
    """
    if not os.path.exists(path):
        return []

    with open(path, "r", encoding="utf-8") as f:
        lines = [line.strip() for line in f if line.strip()]

    pairs = []
    i = 0

    while i < len(lines) - 1:
        if "USER:" in lines[i] and "ASSISTANT:" in lines[i + 1]:
            user_msg = lines[i].split("USER:", 1)[1].strip()
            assistant_msg = lines[i + 1].split("ASSISTANT:", 1)[1].strip()
            pairs.append((user_msg, assistant_msg))
            i += 2
        else:
            i += 1  # 포맷이 어긋난 줄 스킵

    # 최근 pair_count개만 반환
    return pairs[-pair_count:]


