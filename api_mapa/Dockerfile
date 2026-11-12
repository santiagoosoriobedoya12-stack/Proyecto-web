# ==============================================================
# Dockerfile para la API REST Mapa de Conocimiento (FastAPI + PyMySQL)
# Expone el puerto 8117 con recarga automática en desarrollo.
# ==============================================================

FROM python:3.12-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8117

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8117", "--reload"]
