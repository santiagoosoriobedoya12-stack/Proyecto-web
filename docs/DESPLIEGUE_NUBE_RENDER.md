# Guía de Despliegue en la Nube (PostgreSQL + FastAPI + Flask en Render.com)

Esta guía explica paso a paso cómo publicar el proyecto **Mapa de Conocimiento** en la nube de forma **100% gratuita** utilizando [Render.com](https://render.com), cumpliendo con los requisitos de entrega del profesor:
- **Base de datos:** PostgreSQL en la nube.
- **Backend:** API REST en FastAPI.
- **Frontend:** Aplicación web en Flask.
- **Credenciales del docente:** Usuario `admin` / Contraseña `admin123`.

---

## Opción Rápida: Despliegue Automático con `render.yaml` (Blueprints)

El repositorio incluye el archivo [`render.yaml`](../render.yaml) configurado para desplegar los 3 servicios juntos:

1. Ingresa a [https://render.com](https://render.com) e inicia sesión con tu cuenta de **GitHub**.
2. En el panel principal (Dashboard), haz clic en **New +** y selecciona **Blueprint**.
3. Selecciona tu repositorio: `santiagoosoriobedoya12-stack/Proyecto-web`.
4. Render detectará automáticamente el archivo `render.yaml` y te mostrará:
   - Base de datos: `mapa-db-postgres` (PostgreSQL gratuito).
   - API: `mapa-conocimiento-api` (FastAPI).
   - Front: `mapa-conocimiento-front` (Flask).
5. Haz clic en **Apply**. Render creará los 3 servicios y los conectará automáticamente.

---

## Opción Manual (Paso a Paso en 15 Minutos)

Si prefieres crear cada servicio manualmente en Render:

### Paso 1: Crear la Base de Datos PostgreSQL
1. En Render, clic en **New +** → **PostgreSQL**.
2. Llena los campos:
   - **Name:** `mapa-db-postgres`
   - **Database:** `mapa_conocimiento`
   - **User:** `mapa_user`
   - **Region:** Selecciona la más cercana (ej. `Oregon (US West)` u `Ohio`).
   - **Plan:** `Free`.
3. Haz clic en **Create Database**.
4. Una vez creada, busca la sección **Connections** y copia la **Internal Database URL** (para conectar dentro de Render) y la **External Database URL**.

#### Cargar las 21 Tablas en PostgreSQL
1. En la página de tu base de datos en Render, haz clic en la pestaña **Connect** → abre la consola web (**PSQL**) o usa DBeaver / pgAdmin con la URL Externa.
2. Abre el archivo [`db/init_postgres.sql`](../db/init_postgres.sql), copia todo su contenido y pégalo en la consola de PostgreSQL para crear las 21 tablas, los 17 ODS y los usuarios semilla.

---

### Paso 2: Crear el Servicio de la API (FastAPI)
1. En Render, clic en **New +** → **Web Service**.
2. Conecta tu repositorio de GitHub `santiagoosoriobedoya12-stack/Proyecto-web`.
3. Configura:
   - **Name:** `mapa-conocimiento-api`
   - **Region:** La misma de la base de datos (ej. `Oregon`).
   - **Branch:** `main`
   - **Root Directory:** `api_mapa`
   - **Runtime:** `Python 3`
   - **Build Command:** `pip install -r requirements.txt`
   - **Start Command:** `uvicorn main:app --host 0.0.0.0 --port $PORT`
   - **Plan:** `Free`
4. En la sección **Environment Variables**, agrega:
   - `DATABASE_URL`: Pega la URL de tu base de datos PostgreSQL de Render.
   - `DB_ENGINE`: `postgres`
   - `JWT_SECRET`: `SuperClaveSecretaMapaConocimientoParaJwt2026*`
5. Haz clic en **Create Web Service**.
6. Cuando termine el despliegue (diga *Live*), copia la URL generada (ej. `https://mapa-conocimiento-api.onrender.com`).
   - Pruébala en el navegador: `https://mapa-conocimiento-api.onrender.com/swagger`.

---

### Paso 3: Crear el Servicio del Frontend (Flask)
1. En Render, clic en **New +** → **Web Service**.
2. Conecta nuevamente tu repositorio `santiagoosoriobedoya12-stack/Proyecto-web`.
3. Configura:
   - **Name:** `mapa-conocimiento-front`
   - **Region:** La misma de la API.
   - **Branch:** `main`
   - **Root Directory:** `front_flask`
   - **Runtime:** `Python 3`
   - **Build Command:** `pip install -r requirements.txt`
   - **Start Command:** `gunicorn -b 0.0.0.0:$PORT app:app`
   - **Plan:** `Free`
4. En **Environment Variables**, agrega:
   - `API_BASE_URL`: Pega la URL completa de tu API creada en el Paso 2 (ej. `https://mapa-conocimiento-api.onrender.com`).
5. Haz clic en **Create Web Service**.
6. Cuando termine, tendrás la URL pública de tu Frontend (ej. `https://mapa-conocimiento-front.onrender.com`).

---

## Paso 4: Probar la Navegación y Obtener los Datos de Entrega

1. Abre la URL del Frontend en tu navegador.
2. Haz clic en **Iniciar sesión** e ingresa con:
   - **Usuario:** `admin`
   - **Contraseña:** `admin123`
3. Verifica que la barra de navegación muestre:
   - **v1 Catálogos y Proyectos**
   - **v2 Investigación y Vínculos**
   - **v3 Administración**
4. Copia las URLs y pégalas en el correo para el profesor.
