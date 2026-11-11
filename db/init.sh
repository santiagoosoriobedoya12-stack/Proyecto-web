#!/bin/bash
# Habilita la detención inmediata del script si cualquier comando devuelve un código de error distinto de 0
set -e

# Imprime mensaje de estado mientras espera a que el motor SQL Server acepte conexiones
echo "==> Esperando a que SQL Server esté listo..."
# Bucle hasta que 'sqlcmd' ejecute con éxito la consulta trivial 'SELECT 1' (-C confía en certificado TLS local, -b falla si hay error SQL)
until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" -b > /dev/null 2>&1; do
    # Informa que el motor sigue iniciándose
    echo "SQL Server aún no está listo. Esperando 2 segundos..."
    # Pausa 2 segundos antes del siguiente reintento
    sleep 2
done

# Mensaje informativo antes de comprobar la existencia de la base de datos
echo "==> Verificando si la base de datos bdfacturas_sqlserver_local ya existe..."
# Consulta sys.databases (-h -1 quita cabeceras, -W quita espacios) para contar si ya existe la BD y limpia espacios con 'tr'
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'bdfacturas_sqlserver_local'" 2>/dev/null | tr -d '[:space:]')

# Si DB_EXISTS vale 1, la base ya fue inicializada en una ejecución previa (el volumen mssqldata persiste los datos)
if [ "$DB_EXISTS" = "1" ]; then
    # Omite recrear tablas y datos para no borrar información existente
    echo "==> La base de datos bdfacturas_sqlserver_local ya existe. Omitiendo inicialización."
else
    # Si no existe (primer arranque o tras docker compose down -v), ejecuta el script maestro DDL + DML + SPs + Triggers
    echo "==> Creando base de datos y ejecutando /scripts/bdfacturas.sql..."
    # Ejecuta el archivo completo montado en /scripts/bdfacturas.sql dentro del contenedor
    /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/bdfacturas.sql
    # Confirma que las 12 tablas, procedimientos, disparadores y semilla quedaron cargados
    echo "==> Inicialización completada con éxito."
fi
