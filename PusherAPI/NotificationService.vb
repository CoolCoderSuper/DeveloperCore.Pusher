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
        Dim str As String = JsonSerializer.Serialize(notification)
        Dim dataBytes As Byte() = Encoding.UTF8.GetBytes(str)
        For Each client In _clients.ToArray()
            Await client.SendAsync(dataBytes, WebSocketMessageType.Text, True, CancellationToken.None)
        Next
        For Each callback In _sseCallbacks.ToArray()
            callback(str)
        Next
        Return True
    End Function

    Private Shared ReadOnly _sseCallbacks As New List(Of Action(Of String))()
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

    Public Shared Function SseHandler(key As String, token As CancellationToken) As IResult
        If key = NotificationService.Key Then
            Return TypedResults.ServerSentEvents(New EventStreamEnumerable(token))
        Else
            Return TypedResults.Unauthorized()
        End If
    End Function

    Public Class EventStreamEnumerable
        Implements IAsyncEnumerable(Of String)

        Private ReadOnly _token As CancellationToken

        Public Sub New(token As CancellationToken)
            _token = token
        End Sub

        Public Function GetAsyncEnumerator(Optional cancellationToken As CancellationToken = Nothing) As IAsyncEnumerator(Of String) Implements IAsyncEnumerable(Of String).GetAsyncEnumerator
            Return New EventStreamEnumerator(_token)
        End Function
    End Class

    Public Class EventStreamEnumerator
        Implements IAsyncEnumerator(Of String)

        Private ReadOnly _token As CancellationToken
        Private _value As String
        Private _triggered As Boolean

        Public Sub New(token As CancellationToken)
            _token = token
            _sseCallbacks.Add(Sub(value)
                _value = value
                _triggered = True
            End Sub)
        End Sub

        Public ReadOnly Property Current As String Implements IAsyncEnumerator(Of String).Current
            Get
                Return _value
            End Get
        End Property

        Public Function MoveNextAsync() As ValueTask(Of Boolean) Implements IAsyncEnumerator(Of String).MoveNextAsync
            If _token.IsCancellationRequested Then
                Return ValueTask.FromResult(False)
            End If
            Return New ValueTask(Of Boolean)(Task.Run(Function()
                While Not _triggered AndAlso Not _token.IsCancellationRequested
                End While
                _triggered = False
                Return True
            End Function))
        End Function

        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
        End Function
    End Class
End Class
