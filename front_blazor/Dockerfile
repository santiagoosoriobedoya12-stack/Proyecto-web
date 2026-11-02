# ==============================================================
# Dockerfile para el Frontend en Blazor (front_blazor)
# Corre en el puerto 8096 con soporte de recarga en vivo (dotnet watch)
# ==============================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Puerto oficial asignado al frontend en la especificación
ENV ASPNETCORE_URLS=http://0.0.0.0:8096

# Detección de cambios de archivos para recompilación automática
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

EXPOSE 8096

# Recompila y reinicia la aplicación Blazor al modificar archivos .razor o .cs
CMD ["dotnet", "watch", "run", "--no-launch-profile"]

