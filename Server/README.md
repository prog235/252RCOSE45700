# Chat Backend

Run commands from this Server directory (prompt paths are relative to it).
Python 3.10 or newer is required.

```sh
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
# Set OPENAI_API_KEY in .env before starting.
python main.py
```

The server listens on port 8000 by default. Set PORT in .env to change it.
Session logs are created automatically under logs/sessions/.

Run offline tests (no API requests):

```sh
PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover -s tests -v
```
