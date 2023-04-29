Imports System.Net
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Server.Kestrel
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting

Public Module Program
    Public Sub Main(args As String())
        Dim builder As WebApplicationBuilder = WebApplication.CreateBuilder(args)
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()
        builder.Services.AddSingleton(Of NotificationService)
        Dim app As WebApplication = builder.Build()
        app.UseSwagger()
        app.UseSwaggerUI()
        app.UseHttpsRedirection()
        app.UseAuthorization()
        app.MapControllers()
        app.UseWebSockets()
        app.Map("/ws", Async Function(context As HttpContext)
                           If context.WebSockets.IsWebSocketRequest Then
                               Dim webSocket = Await context.WebSockets.AcceptWebSocketAsync()
                               Dim last As Notification
                               While True
                                   If NotificationService.Clients.Any AndAlso last IsNot NotificationService.Clients.LastOrDefault Then
                                       last = NotificationService.Clients.Last
                                       Dim data As Byte() = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(last))
                                       Await webSocket.SendAsync(data, WebSocketMessageType.Text, True, CancellationToken.None)
                                   End If
                               End While
                           Else
                               context.Response.StatusCode = HttpStatusCode.BadRequest
                           End If
                       End Function)
        app.Run("https://localhost:7166")
    End Sub
End Module