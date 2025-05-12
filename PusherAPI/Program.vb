Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection

Public Module Program
    Public Sub Main(args As String())
        Dim builder As WebApplicationBuilder = WebApplication.CreateBuilder(args)
        NotificationService.Key = builder.Configuration.Item("Key")
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()
        builder.Services.AddSingleton(Of NotificationService)
        Dim app As WebApplication = builder.Build()
        app.UseSwagger()
        app.UseSwaggerUI()
        app.UseAuthorization()
        app.MapControllers()
        app.UseWebSockets()
        app.MapGet("/", Function() "yo were in")
        app.Map("/ws", AddressOf NotificationService.SocketHandler)
#If DEBUG Then
        app.Run("http://localhost:7166")
#Else
        app.Run()
#End If
    End Sub
End Module