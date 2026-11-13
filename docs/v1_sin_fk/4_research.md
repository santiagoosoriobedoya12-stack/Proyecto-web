# Investigación Técnica — Versión 1

Investigación previa para la implementación del corte vertical en Python:

1. **PyMySQL vs SQLAlchemy / ORMs:**
   - La metodología exige SQL a la vista y control explícito de la persistencia.
   - Se seleccionó `pymysql` puro con `DictCursor`, permitiendo escribir sentencias SQL parametrizadas directas sin la sobrecarga de un ORM.
2. **PyMySQL y el comportamiento de `cursor.rowcount` en MariaDB:**
   - Por defecto, MySQL/MariaDB retorna las filas *cambiadas* físicamente en disco.
   - En un `PUT` idempotente que repite los mismos valores, `rowcount` daría `0`.
   - Solución: Pasar `client_flag=pymysql.constants.CLIENT.FOUND_ROWS` en `pymysql.connect()`.
3. **Pydantic v2 para Segregación de DTOs:**
   - `BaseModel` valida tipos y restricciones (`min_length`, `max_length`, `ge=0`, `EmailStr`).
   - El uso de esquemas separados por verbo garantiza que un `POST` exija todos los campos obligatorios y un `PATCH` permita actualizaciones parciales sin romper integridad.
