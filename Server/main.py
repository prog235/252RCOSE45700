# main.py
"""
1. 사용자 입력을 받아 무엇에 대한 질문인지 판단함
2. 질문의 종류에 따라 DB에서 프롬프트 input으로 넘길 정보를 붙임
3. general rule과 필요한 데이터를 프롬프트로 만들어 LLM API request
4. 받아온 응답을 다시 유니티로 넘김
"""

import llm
from session_store import SessionStore, SessionError
from starlette.concurrency import iterate_in_threadpool
from starlette.background import BackgroundTask
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field
import uvicorn
import logging
import os

app = FastAPI(title="Chat Backend", version="1.0.0")
sessions = SessionStore()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class SessionIn(BaseModel):
    sessionId: str = Field(..., min_length=1)


class ChatIn(SessionIn):
    message: str = Field(..., min_length=1, description="사용자 입력 메시지")
    curRoom: int = Field(...)
    truthLevel: int = Field(...)
    gifted: bool = Field(...)

logger = logging.getLogger("chat")
logging.basicConfig(level=logging.INFO)

@app.get("/", tags=["health"])
def health():
    return {"status": "ok"}


# -----------------------------
# Simulation lifecycle
# -----------------------------
@app.post("/simulation/start")
async def simulation_start():
    """
    Unity에서 '새 시뮬레이션 시작' 시 1회 호출.
    새 UUID와 별도의 로그를 생성하며 기존 로그는 보존한다.
    """
    try:
        return {"ok": True, "sessionId": sessions.create()}
    except Exception as e:
        logger.exception("simulation_start failed")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/simulation/end")
async def simulation_end(payload: SessionIn):
    """
    Unity에서 '시뮬레이션 종료' 시 1회 호출.
    해당 세션만 닫고 로그를 그대로 보존한다. 진행 중인 턴은 마무리한다.
    """
    try:
        sessions.end(payload.sessionId)
        return {"ok": True}
    except SessionError as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))
    except Exception as e:
        logger.exception("simulation_end failed")
        raise HTTPException(status_code=500, detail=str(e))


# -----------------------------
# Chat
# -----------------------------
@app.post("/chat/send")
async def chat_send(payload: ChatIn):

    msg = payload.message.strip()
    curRoom = payload.curRoom
    truthLevel = payload.truthLevel
    gifted = payload.gifted

    if not msg:
        raise HTTPException(status_code=400, detail="Empty message")

    # Reserve before sending SSE headers so invalid/busy sessions get HTTP errors.
    try:
        turn = sessions.begin_turn(payload.sessionId)
    except SessionError as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))

    async def event_generator():
        reply = llm.generate_reply(msg, curRoom, truthLevel, gifted, str(turn.log_path))
        try:
            async for token in iterate_in_threadpool(reply):
                yield f"data: {token}\n\n"
            yield "event: end\ndata: [DONE]\n\n"
        finally:
            try:
                reply.close()
            finally:
                turn.release()

    return StreamingResponse(
        event_generator(), media_type="text/event-stream",
        background=BackgroundTask(turn.release),
    )


if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=int(os.getenv("PORT", "8000")),
        reload=True
    )
