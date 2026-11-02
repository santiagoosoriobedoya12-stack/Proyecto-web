# ==============================================================
# Dockerfile para la API de Facturación (api_facturas)
# Utiliza la imagen del SDK completo de .NET 10 para soportar 'dotnet watch'
# ==============================================================

# Usamos la imagen oficial del SDK de .NET 10 (no solo el runtime)
# porque el curso requiere recarga en vivo (hot reload) mediante 'dotnet watch'
FROM mcr.microsoft.com/dotnet/sdk:10.0

# Establecemos la carpeta de trabajo dentro del contenedor
WORKDIR /app

# Configuramos la URL en la que escuchará ASP.NET Core (puerto 8032)
ENV ASPNETCORE_URLS=http://0.0.0.0:8032

# Activamos el observador de archivos por sondeo para que detecte cambios
# realizados desde Windows a través del volumen montado
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Exponemos el puerto 8032 para la comunicación de red
EXPOSE 8032

# El comando por defecto es 'dotnet watch run':
# Cada vez que guardes un archivo .cs en tu editor, el proyecto se recompilará solo
CMD ["dotnet", "watch", "run", "--no-launch-profile"]
