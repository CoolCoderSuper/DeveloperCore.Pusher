Imports System.IO
Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading

''' <summary>
''' Represents a receiver for a channel.
''' </summary>
Public Class Receiver
    Dim _wsClient As ClientWebSocket
    Dim _connected As Boolean = False
    Dim _watchToken As CancellationTokenSource
    Dim _events As New List(Of [Event])

    ''' <summary>
    ''' Determines whether the client is connected to the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Connected As Boolean
        Get
            Return _connected
        End Get
    End Property

    Public Sub New(url As String, channel As String)
        Me.Url = url
        Me.Channel = channel
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
                    Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString)
                    If n.Channel = Channel Then
                        _events.ForEach(Sub(x) If x.Event = n.Event Then x.Action.Invoke(n))
                    End If
                End If
            End While
        Catch ex As OperationCanceledException

        End Try
    End Sub

    ''' <summary>
    ''' Binds an event to an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Bind([event] As String, e As Action(Of Notification))
        _events.Add(New [Event]([event], e))
    End Sub

    ''' <summary>
    ''' Unbinds an event from an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Unbind([event] As String, e As Action(Of Notification))
        _events.RemoveAll(Function(x) x.Event = [event] AndAlso x.Action = e)
    End Sub

    ''' <summary>
    ''' The base URL of the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Url As String

    ''' <summary>
    ''' The channel to listen to.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Channel As String
End Class