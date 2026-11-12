# ==============================================================
# Dockerfile para la API REST de Facturación en Python (api_python)
# Utiliza Python 3.12 sobre FastAPI + Uvicorn con recarga automática
# ==============================================================

# Imagen oficial liviana de Python 3.12
FROM python:3.12-slim

# Carpeta de trabajo interna dentro del contenedor
WORKDIR /app

# Evita archivos .pyc en disco y fuerza salida de logs inmediata en Docker
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Copia primero requirements.txt para aprovechar la caché de capas de Docker
COPY requirements.txt .

# Instala dependencias de FastAPI, PyMSSQL (SQL Server), PyJWT y BCrypt
RUN pip install --no-cache-dir -r requirements.txt

# Copia el código fuente de la API dentro del contenedor
COPY . .

# Expone el puerto interno 8032 (mapeado hacia 8035 y 8032 en docker-compose.yml)
EXPOSE 8032

# Inicia el servidor ASGI Uvicorn escuchando en todas las interfaces con --reload en desarrollo
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8032", "--reload"]
