using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;

namespace CouncilAgendaApi.Services;

public interface IGoogleDocService
{
    Task<string> CreateAgendaDocAsync(string title, IList<Request> requests, string folderId, string refreshToken);
}

public class GoogleDocService : IGoogleDocService
{
    private readonly IConfiguration _config;

    public GoogleDocService(IConfiguration config)
    {
        _config = config;
    }

    private (DocsService docs, DriveService drive) BuildServices(string refreshToken)
    {
        var clientId = _config["Google:OAuthClientId"]!;
        var clientSecret = _config["Google:OAuthClientSecret"]!;

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[]
            {
                DriveService.Scope.Drive,
                DocsService.Scope.Documents
            }
        });

        var token = new TokenResponse { RefreshToken = refreshToken };
        var credential = new UserCredential(flow, "user", token);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Council Agenda"
        };

        return (new DocsService(initializer), new DriveService(initializer));
    }

    public async Task<string> CreateAgendaDocAsync(string title, IList<Request> requests, string folderId, string refreshToken)
    {
        var (docsService, driveService) = BuildServices(refreshToken);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = title,
            MimeType = "application/vnd.google-apps.document",
            Parents = new List<string> { folderId }
        };

        var createRequest = driveService.Files.Create(fileMetadata);
        createRequest.Fields = "id";
        createRequest.SupportsAllDrives = true;
        var file = await createRequest.ExecuteAsync();
        var docId = file.Id;

        if (requests.Count > 0)
        {
            await docsService.Documents.BatchUpdate(
                new BatchUpdateDocumentRequest { Requests = requests },
                docId
            ).ExecuteAsync();
        }

        return $"https://docs.google.com/document/d/{docId}/edit";
    }
}