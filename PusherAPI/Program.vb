Imports System.Net
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.DependencyInjection

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
        app.UseAuthorization()
        app.MapControllers()
        app.UseWebSockets()
        app.Map("/ws", 
                Async Function(context As HttpContext)
                    If context.WebSockets.IsWebSocketRequest Then
                        If context.Request.Query.ContainsKey("key") AndAlso context.Request.Query.Item("key") = builder.Configuration.Item("key") Then
                            Dim webSocket = Await context.WebSockets.AcceptWebSocketAsync()
                            Dim notificationHandler = Async Sub(notification)
                                Dim data As Byte() = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Of Notification)(notification))
                                Await webSocket.SendAsync(data, WebSocketMessageType.Text, True, CancellationToken.None)
                            End Sub
                            AddHandler NotificationService.NotificationReceived, notificationHandler
                            While webSocket.State = WebSocketState.Open
                                Await Task.Delay(1000)
                            End While
                            RemoveHandler NotificationService.NotificationReceived, notificationHandler
                        Else 
                            context.Response.StatusCode = HttpStatusCode.Unauthorized
                        End If
                    Else
                        context.Response.StatusCode = HttpStatusCode.BadRequest
                    End If
                End Function)
#If DEBUG Then
        app.Run("http://localhost:7166")
#Else
        app.Run()
#End If
    End Sub
End Module