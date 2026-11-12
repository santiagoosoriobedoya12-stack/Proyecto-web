# ==============================================================
# Script auxiliar opcional para importar los catálogos desde Excel (.xlsx)
# cuando se coloque el archivo oficial del curso en la carpeta Mapa_conocimiento/.
# Requisitos: pip install openpyxl pymysql
# ==============================================================

import os
import sys


def main():
    if len(sys.argv) < 2:
        print("Uso: python db/cargar_excel.py <ruta_al_archivo.xlsx>")
        sys.exit(1)

    ruta_xlsx = sys.argv[1]
    if not os.path.isfile(ruta_xlsx):
        print(f"No se encontró el archivo: {ruta_xlsx}")
        sys.exit(1)

    try:
        import openpyxl
        import pymysql
    except ImportError:
        print("Instala dependencias primero: pip install openpyxl pymysql")
        sys.exit(1)

    conn = pymysql.connect(
        host=os.getenv("DB_HOST", "localhost"),
        port=int(os.getenv("DB_PORT", "13333")),
        user=os.getenv("DB_USER", "mapa_user"),
        password=os.getenv("DB_PASSWORD", "MapaConocimiento2026!"),
        database=os.getenv("DB_NAME", "mapa_conocimiento"),
        charset="utf8mb4",
    )
    wb = openpyxl.load_workbook(ruta_xlsx, data_only=True)
    with conn.cursor() as cur:
        for hoja in wb.sheetnames:
            nombre = hoja.strip().lower()
            ws = wb[hoja]
            filas = list(ws.iter_rows(min_row=2, values_only=True))
            if "area_conocimiento" in nombre:
                for f in filas:
                    if f[0] is not None:
                        cur.execute(
                            "INSERT IGNORE INTO area_conocimiento (id, gran_area, area, disciplina) VALUES (%s, %s, %s, %s)",
                            (int(f[0]), str(f[1])[:60], str(f[2])[:60], str(f[3])[:60]),
                        )
            elif "ods" in nombre or "sostenible" in nombre:
                for f in filas:
                    if f[0] is not None:
                        cur.execute(
                            "INSERT IGNORE INTO objetivo_desarrollo_sostenible (id, nombre, categoria) VALUES (%s, %s, %s)",
                            (int(f[0]), str(f[1])[:60], str(f[2])[:45]),
                        )
            elif "aplicacion" in nombre:
                for f in filas:
                    if f[0] is not None:
                        cur.execute(
                            "INSERT IGNORE INTO area_aplicacion (id, nombre) VALUES (%s, %s)",
                            (int(f[0]), str(f[1])[:60]),
                        )
    conn.commit()
    conn.close()
    print("Importación de catálogos desde Excel finalizada correctamente.")


if __name__ == "__main__":
    main()
