#!/bin/bash
set -e

echo "==> Esperando a que SQL Server esté listo..."
until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" -b > /dev/null 2>&1; do
    echo "SQL Server aún no está listo. Esperando 2 segundos..."
    sleep 2
done

echo "==> Verificando si la base de datos bdfacturas_sqlserver_local ya existe..."
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'bdfacturas_sqlserver_local'" 2>/dev/null | tr -d '[:space:]')

if [ "$DB_EXISTS" = "1" ]; then
    echo "==> La base de datos bdfacturas_sqlserver_local ya existe. Omitiendo inicialización."
else
    echo "==> Creando base de datos y ejecutando /scripts/bdfacturas.sql..."
    /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/bdfacturas.sql
    echo "==> Inicialización completada con éxito."
fi

