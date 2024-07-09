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

    Public Async Function Send(channel As String, [event] As String, key As String, data As Object) As Task(Of Boolean)
        If key <> _config.Item("Key") Then Return False
        Dim notification As New Notification(channel, [event], data)
        For Each client In _clients.ToArray()
            Dim dataBytes As Byte() = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification))
            Await client.SendAsync(dataBytes, WebSocketMessageType.Text, True, CancellationToken.None)
        Next
        Return True
    End Function

    Private Shared ReadOnly _clients As New List(Of WebSocket)

    Public Shared Property Key As String

    Public Shared Async Function SocketHandler(context As HttpContext) As Task
        If context.WebSockets.IsWebSocketRequest Then
            If context.Request.Query.ContainsKey("key") AndAlso context.Request.Query.Item("key") = Key Then
                Dim webSocket = Await context.WebSockets.AcceptWebSocketAsync()
                SyncLock _clients
                    _clients.Add(webSocket)
                End SyncLock
                Try
                    Await Task.WhenAny(StatusCheck(webSocket), Task.Delay(Timeout.Infinite))
                Finally
                    SyncLock _clients
                        _clients.Remove(webSocket)
                    End SyncLock
                    webSocket.Dispose()
                End Try
            Else
                context.Response.StatusCode = HttpStatusCode.Unauthorized
            End If
        Else
            context.Response.StatusCode = HttpStatusCode.BadRequest
        End If
    End Function

    Private Shared Async Function StatusCheck(webSocket As WebSocket) As Task
        Dim buffer As Byte() = New Byte(1023) {}
        While webSocket.State = WebSocketState.Open
            Try
                Await webSocket.SendAsync(Encoding.UTF8.GetBytes("yo"), WebSocketMessageType.Text, True, CancellationToken.None)
                Dim result = Await webSocket.ReceiveAsync(buffer, CancellationToken.None)
                If result.MessageType = WebSocketMessageType.Close Then
                    Exit While
                End If
            Catch ex As WebSocketException
                Exit While
            End Try
            Await Task.Delay(5000)
        End While
    End Function
End Class