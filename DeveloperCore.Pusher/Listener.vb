Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading

Public Class Listener
    Private ReadOnly _trigger As Action(Of Notification)
    Dim _connected As Boolean = False
    Dim _watchToken As CancellationTokenSource
    Dim _wsClient As ClientWebSocket

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification))
        _trigger = trigger
        Me.Key = key
        Url = uri
    End Sub

    Public ReadOnly Property Url As Uri
    Public ReadOnly Property Key As String

    Public ReadOnly Property Connected As Boolean
        Get
            Return _connected
        End Get
    End Property
    
    Public Async Function ConnectAsync(token As CancellationToken) As Task
        If Connected Then Return
        _connected = True
        _wsClient = New ClientWebSocket
        Await _wsClient.ConnectAsync(Url, token)
        _watchToken = New CancellationTokenSource
        Watch(_watchToken.Token)
    End Function
    
    Public Async Function DisconnectAsync(token As CancellationToken) As Task
        If Connected Then
            _watchToken.Cancel()
            If _wsClient.State = WebSocketState.Aborted Then
                _wsClient.Abort()
            Else
                Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token)
            End If
            _connected = False
        End If
    End Function
    
    Private Async Sub Watch(token As CancellationToken)
        Try
            While _wsClient.State = WebSocketState.Open
                If _watchToken.IsCancellationRequested Then Return
                Dim buffer As New ArraySegment(Of Byte)(New Byte(1024) {})
                Dim result As WebSocketReceiveResult = Await _wsClient.ReceiveAsync(buffer, token)
                If result.MessageType = WebSocketMessageType.Close Then
                    Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token)
                Else
                    Dim data As String = Encoding.UTF8.GetString(buffer.Array).Replace(vbNullChar, "")
                    Dim obj As JsonObject = JsonNode.Parse(data)
                    Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString, Date.Now)
                    _trigger(n)
                End If
            End While
        Catch ex As OperationCanceledException

        End Try
    End Sub
End Class