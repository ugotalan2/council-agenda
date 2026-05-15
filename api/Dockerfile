FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY ./publish .
ENTRYPOINT ["dotnet", "CouncilAgendaApi.dll"]