Imports System.IO
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading

''' <summary>
''' Receives messages on multiple channels.
''' </summary>
Public Class Receiver
    Dim _wsClient As ClientWebSocket
    Dim _connected As Boolean = False
    Dim _watchToken As CancellationTokenSource
    Dim _channels As New List(Of Channel)

    ''' <summary>
    ''' Determines whether the client is connected to the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Connected As Boolean
        Get
            Return _connected
        End Get
    End Property

    Public Sub New(url As String)
        Me.Url = url
    End Sub

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <returns></returns>
    Public Async Function ConnectAsync() As Task
        Await ConnectAsync(CancellationToken.None)
    End Function

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <param name="token">The cancellation token.</param>
    Public Async Function ConnectAsync(token As CancellationToken) As Task
        If Connected Then Return
        _connected = True
        _wsClient = New ClientWebSocket
        Await _wsClient.ConnectAsync(New Uri(Path.Combine(Url, "ws")), token)
        _watchToken = New CancellationTokenSource
        Watch(_watchToken.Token)
    End Function

    ''' <summary>
    ''' Disconnects from the server.
    ''' </summary>
    Public Async Function DisconnectAsync() As Task
        Await DisconnectAsync(CancellationToken.None)
    End Function

    ''' <summary>
    ''' Disconnects from the server.
    ''' </summary>
    ''' <param name="token">The cancellation token.</param>
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

    ''' <summary>
    ''' Subscribes to a channel.
    ''' </summary>
    ''' <param name="channel">The channel name.</param>
    ''' <returns></returns>
    Public Function Subscribe(channel As String) As Channel
        Dim ch As New Channel(channel)
        _channels.Add(ch)
        Return ch
    End Function
    
    ''' <summary>
    ''' Unsubscribes from a channel.
    ''' </summary>
    ''' <param name="channel">The channel.</param>
    Public Sub Unsubscribe(channel As Channel)
        _channels.Remove(channel)
    End Sub
    
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
                    For Each ch As Channel In _channels.Where(Function(x) x.Name = n.Channel)
                        ch.MessageReceived(n)
                    Next
                End If
            End While
        Catch ex As OperationCanceledException

        End Try
    End Sub
    
    ''' <summary>
    ''' The base URL of the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Url As String
End Class