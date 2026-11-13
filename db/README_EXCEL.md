# Datos de Referencia y Catálogos en Excel

Este documento explica cómo se gestionan los catálogos del módulo **Mapa de Conocimiento** y cómo utilizar el archivo Excel oficial suministrado por el docente.

---

## 1. Estado Actual de la Base de Datos (Independencia del Excel)

Para que el proyecto funcione de inmediato al clonar y ejecutar `docker compose up -d`, **[`db/init.sql`](init.sql)** ya incluye:
- Los **17 Objetivos de Desarrollo Sostenible (ODS)** oficiales completos.
- Datos semilla de referencia para `area_conocimiento`, `area_aplicacion`, `linea_investigacion`, `aliado`, `tipo_producto`, `proyecto`, `docente`, `producto`, etc.
- Los **3 roles** (`Administrador`, `Investigador`, `Consulta`) y el usuario `admin` con hash BCrypt.

Por lo tanto, **el sistema no requiere obligatoriamente del archivo Excel para funcionar, probarse o sustentarse**.

---

## 2. Cómo Importar el Excel Oficial del Curso

El enunciado del curso menciona:
> *"los datos de referencia en el Excel de Mapa_conocimiento/"* (218 registros de `area_conocimiento`, 21 de `area_aplicacion`, etc.).

Si el docente entrega dicho archivo (por ejemplo `catalogos.xlsx` o `mapa_conocimiento.xlsx`), puedes colocarlo en esta carpeta (`db/`) o en la raíz del proyecto e importarlo con el script automatizado:

```powershell
# 1. Instalar dependencias del script (si se corre localmente fuera de Docker)
pip install openpyxl pymysql

# 2. Ejecutar la importación hacia MariaDB (puerto 13333 por defecto)
python db/cargar_excel.py db/catalogos.xlsx
```

El script [`db/cargar_excel.py`](cargar_excel.py):
1. Lee las hojas correspondientes (`area_conocimiento`, `ods`, `area_aplicacion`).
2. Inserta los registros respetando las columnas oficiales y omitiendo duplicados (`INSERT IGNORE`).
3. Mantiene la integridad referencial y las columnas de borrado lógico (`activo = 1`).
