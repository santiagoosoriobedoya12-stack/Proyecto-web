# ==============================================================
# Dockerfile para el Frontend Web en Python (front_python)
# Utiliza FastAPI + Jinja2 + Bootstrap Local en el puerto 8096
# ==============================================================

FROM python:3.12-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8096

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8096", "--reload"]
