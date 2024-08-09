Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
'TODO: Add auto reconnector
Public Class Listener
    Private ReadOnly _trigger As Action(Of Notification)
    Private ReadOnly _onStateChanged As Action(Of Boolean)
    Private ReadOnly _onError As Action(Of ReceiverError)
    Dim _watchToken As CancellationTokenSource
    Dim _wsClient As ClientWebSocket

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification), onStateChanged As Action(Of Boolean), onError As Action(Of ReceiverError))
        _trigger = trigger
        _onStateChanged = onStateChanged
        _onError = onError
        Me.Key = key
        Url = uri
    End Sub

    Public ReadOnly Property Url As Uri
    Public ReadOnly Property Key As String

    Public ReadOnly Property Connected As Boolean
        Get
            Return _wsClient IsNot Nothing AndAlso _wsClient.State = WebSocketState.Open
        End Get
    End Property

    Public Async Function ConnectAsync(token As CancellationToken) As Task
        If Connected Then Return
        _wsClient = New ClientWebSocket
        Dim uri As New UriBuilder(Url)
        uri.Path = "ws"
        uri.Query = $"key={Key}"
        Await _wsClient.ConnectAsync(uri.Uri, token)
        _watchToken = New CancellationTokenSource
        Watch(_watchToken.Token)
        _onStateChanged(Connected)
    End Function

    Public Async Function DisconnectAsync(token As CancellationToken) As Task
        If Connected Then
            _watchToken.Cancel()
            If _wsClient.State = WebSocketState.Aborted Then
                _wsClient.Abort()
            Else
                Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token)
            End If
            _onStateChanged(Connected)
        End If
    End Function

    Private Async Sub Watch(token As CancellationToken)
        Try
            While _wsClient.State = WebSocketState.Open
                If _watchToken.IsCancellationRequested Then Return
                Dim buffer As New ArraySegment(Of Byte)(New Byte(1024) {}) 'TODO: Read longer messages
                Dim result As WebSocketReceiveResult = Await _wsClient.ReceiveAsync(buffer, token)
                If result.MessageType = WebSocketMessageType.Close Then
                    Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token)
                Else
                    Dim data As String = Encoding.UTF8.GetString(buffer.Array).Replace(vbNullChar, "")
                    If data = "yo" Then
                        Await _wsClient.SendAsync(New ArraySegment(Of Byte)(Encoding.UTF8.GetBytes("dawg")), WebSocketMessageType.Text, True, token)
                    Else
                        Dim obj As JsonObject = JsonNode.Parse(data)
                        Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString, Date.Now)
                        _trigger(n)
                    End If
                End If
            End While
        Catch ex As OperationCanceledException
        Catch ex As WebSocketException
            _onError(New ReceiverError(ex.Message, ex))
            _onStateChanged(Connected)
        End Try
    End Sub
End Class