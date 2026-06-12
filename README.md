# AI Project API

Flask API for job recommendation using the trained random forest model.

## Endpoints

- `GET /` basic status response.
- `GET /health` hosting health check.
- `POST /recommend` returns the top matching jobs.

Example request:

```json
{
  "worker_job_type": 1,
  "worker_location": 0,
  "worker_experience": 3
}
```

## Local Run

```bash
pip install -r requirements.txt
gunicorn ai_api:app --bind 0.0.0.0:5000
```

On Windows without gunicorn support:

```bash
python ai_api.py
```

## Recommended Free Hosting

Use Koyeb or Render for this Flask API. Both provide HTTPS URLs automatically.

### Koyeb

1. Push this folder to GitHub.
2. Create a Koyeb Web Service from the GitHub repository.
3. Use the Dockerfile deployment path.
4. Set the service port to `8000` if the platform asks for it.
5. After deployment, test:

```bash
curl https://YOUR-SERVICE.koyeb.app/health
```

### Render

1. Push this folder to GitHub.
2. Create a new Render Blueprint or Web Service.
3. Render can use `render.yaml` automatically.
4. After deployment, test:

```bash
curl https://YOUR-SERVICE.onrender.com/health
```

## Vercel

The Vercel configuration belongs in `vercel.json`, not `Procfile`.

`Procfile` is kept for hosts that run a normal Python web process:

```Procfile
web: gunicorn ai_api:app
```

## ASP.NET Configuration

Set `PythonAI:BaseUrl` to the deployed API root URL without `/recommend`.

Example:

```json
{
  "PythonAI": {
    "BaseUrl": "https://YOUR-SERVICE.koyeb.app"
  }
}
```
