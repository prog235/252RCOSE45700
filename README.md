# Haegin — Escape Game

Unity 기반 방탈출 게임입니다. 오브젝트 탐색과 아이템·퍼즐 상호작용에 더해, FastAPI 서버와 OpenAI API를 연결한 스트리밍 대화 기능을 제공합니다.

## 프로젝트 구성

- `Assets/`: 게임 로직, 장면, UI 및 에셋
- `Packages/`, `ProjectSettings/`: Unity 패키지 및 프로젝트 설정
- `Server/`: AI 대화 API, 방별 힌트·스토리 프롬프트, 세션 관리 및 테스트

서버는 현재 방, 스토리 진행도, 선물 수령 여부와 최근 대화를 바탕으로 응답을 구성합니다. 시뮬레이션마다 별도 세션과 대화 로그를 관리합니다.

## 실행 방법

1. Unity Hub에서 이 저장소를 열어 Unity **6000.2.9f1**로 실행합니다.
2. `Server/`에서 Python **3.10 이상**의 가상환경을 만들고 `pip install -r requirements.txt`로 의존성을 설치합니다.
3. `Server/.env.example`을 `Server/.env`로 복사하고 `OPENAI_API_KEY`를 설정합니다.
4. `Server/`에서 `python main.py`로 서버를 실행합니다. 기본 포트는 **8000**이며, Unity 클라이언트의 서버 주소를 실행 환경에 맞게 설정합니다.

자세한 서버 실행 및 오프라인 테스트 방법은 [Server/README.md](Server/README.md)를 참고하세요. API 키와 실제 대화 로그는 저장소에 포함하지 않습니다.

## 개발 기록

- ~10/15 : 카메라 회전 및 오브젝트 페이드 인/아웃 구현
- 10/19 ~ 10/20 : LLM API 연결 및 채팅 인터페이스 구현(백엔드 로직, FastAPI)
- 10/21 ~ 10/23 : 아이템 획득 및 인벤토리 UI 구현(Camera Raycast, Hotspot)
- 10/24 ~ 10/27 : 자연스러운 그림자 및 렌더링 오류 수정을 위한 회전 로직 수정, RenderQueue 조정
- 10/27 ~ : 지하실(튜토리얼 룸) 퍼즐 설계 (UI, 로직, PuzzleManager)
- ~ 11/6 : 지하실 핫스팟 로직(조명 on), 카메라 줌인/줌아웃 로직, toolbox 퍼즐 설계, 단서 구현
- ~ 11/17 : 로봇 응답(stream, UI) 기능 구현 / 게임 내 주요 기능 모두 구현
- ~ 11/20 : 거실 모델링, group fader 보완
