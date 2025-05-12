Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection
Imports Scalar.AspNetCore

Public Module Program
    Public Sub Main(args As String())
        Dim builder As WebApplicationBuilder = WebApplication.CreateBuilder(args)
        NotificationService.Key = builder.Configuration.Item("Key")
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddOpenApi()
        builder.Services.AddSingleton(Of NotificationService)
        Dim app As WebApplication = builder.Build()
        app.UseAuthorization()
        app.MapControllers()
        app.UseWebSockets()
        app.MapGet("/", Function() "yo were in")
        app.Map("/ws", AddressOf NotificationService.SocketHandler)
        app.MapOpenApi()
        app.MapScalarApiReference()
#If DEBUG Then
        app.Run("http://localhost:7166")
#Else
        app.Run()
#End If
    End Sub
End Module