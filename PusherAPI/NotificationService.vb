Imports System.Net
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Configuration

Public Class NotificationService
    Private ReadOnly _config As IConfiguration

    Public Sub New(config As IConfiguration)
        _config = config
    End Sub
    
    Public Function Send(channel As String, [event] As String, key As String, data As Object) As Boolean
        If key <> _config.Item("Key") Then Return False
        Dim notification As New Notification(channel, [event], data)
        RaiseEvent NotificationReceived(notification)
        Return True
    End Function
    
    Public Shared Event NotificationReceived(notification As Notification)
    
    Public Shared Property Key As String
    
    Public Shared Async Function SocketHandler(context As HttpContext) As Task
        If context.WebSockets.IsWebSocketRequest Then
            If context.Request.Query.ContainsKey("key") AndAlso context.Request.Query.Item("key") = Key Then
                Dim webSocket = Await context.WebSockets.AcceptWebSocketAsync()
                Dim notificationHandler = Async Sub(notification)
                    Dim data As Byte() = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Of Notification)(notification))
                    Await webSocket.SendAsync(data, WebSocketMessageType.Text, True, CancellationToken.None)
                End Sub
                AddHandler NotificationReceived, notificationHandler
                While webSocket.State = WebSocketState.Open
                    Await Task.Delay(1000)
                End While
                RemoveHandler NotificationReceived, notificationHandler
            Else 
                context.Response.StatusCode = HttpStatusCode.Unauthorized
            End If
        Else
            context.Response.StatusCode = HttpStatusCode.BadRequest
        End If
    End Function
End Class