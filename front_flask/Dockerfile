# ==============================================================
# Dockerfile para el Frontend Web (Flask + Jinja2 + Bootstrap Local)
# Expone el puerto 8116; no incluye driver ni acceso directo a MariaDB.
# ==============================================================

FROM python:3.12-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8116

CMD ["python", "app.py"]
