Imports System.Net.WebSockets
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading

Public NotInheritable Class WebSocketListener
    Implements IListener

    Public Const DefaultBufferSize As Integer = 1024
    Private ReadOnly _trigger As Action(Of Notification)
    Private ReadOnly _onStateChanged As Action(Of Boolean)
    Private ReadOnly _onError As Action(Of ReceiverError)
    Private ReadOnly _bufferSize As Integer
    Private _watchToken As CancellationTokenSource
    Private _wsClient As ClientWebSocket

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification), onStateChanged As Action(Of Boolean), onError As Action(Of ReceiverError))
        Me.New(uri, key, trigger, onStateChanged, onError, DefaultBufferSize)
    End Sub

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification), onStateChanged As Action(Of Boolean), onError As Action(Of ReceiverError), bufferSize As Integer)
        _trigger = trigger
        _onStateChanged = onStateChanged
        _onError = onError
        _bufferSize = bufferSize
        Me.Key = key
        Url = uri
    End Sub

    Public ReadOnly Property Url As Uri Implements IListener.Url
    Public ReadOnly Property Key As String Implements IListener.Key

    Public ReadOnly Property Connected As Boolean Implements IListener.Connected
        Get
            Return _wsClient IsNot Nothing AndAlso _wsClient.State = WebSocketState.Open
        End Get
    End Property

    Public Async Function ConnectAsync(token As CancellationToken) As Task Implements IListener.ConnectAsync
        If Connected Then Return
        _wsClient = New ClientWebSocket
        Dim uri As New UriBuilder(Url)
        uri.Path = "ws"
        uri.Query = $"key={Key}"
        Await _wsClient.ConnectAsync(uri.Uri, token).FreeContext()
        _watchToken = New CancellationTokenSource
        Watch(_watchToken.Token)
        _onStateChanged(Connected)
    End Function

    Public Async Function DisconnectAsync(token As CancellationToken) As Task Implements IListener.DisconnectAsync
        If Connected Then
            _watchToken.Cancel()
            If _wsClient.State = WebSocketState.Aborted Then
                _wsClient.Abort()
            Else
                Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token).FreeContext()
            End If
            _onStateChanged(Connected)
        End If
    End Function

    Private Async Sub Watch(token As CancellationToken)
        Try
            While _wsClient.State = WebSocketState.Open
                If _watchToken.IsCancellationRequested Then Return
                Dim buffers As New List(Of Byte())
                While True
#Disable Warning CA1861'we need a fresh array so that old data from last time is not included
                    Dim buffer As New ArraySegment(Of Byte)(New Byte(_bufferSize) {})
#Enable Warning CA1861
                    Dim result As WebSocketReceiveResult = Await _wsClient.ReceiveAsync(buffer, token).FreeContext()
                    buffers.Add(buffer.Array.AsSpan().Slice(0, result.Count).ToArray())
                    If result.MessageType = WebSocketMessageType.Close Then
                        Await _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, Nothing, token).FreeContext()
                        Return
                    ElseIf result.EndOfMessage Then
                        Exit While
                    End If
                End While
                Dim data As String = Encoding.UTF8.GetString(buffers.SelectMany(Function(x) x).ToArray())
                If data = "yo" Then
                    Await _wsClient.SendAsync(New ArraySegment(Of Byte)(Encoding.UTF8.GetBytes("dawg")), WebSocketMessageType.Text, True, token).FreeContext()
                Else
                    Dim obj As JsonObject = JsonNode.Parse(data)
                    Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString, Date.Now)
                    _trigger(n)
                End If
            End While
        Catch ex As OperationCanceledException
#Disable Warning CA1031'we want to catch all exceptions
        Catch ex As Exception
#Enable Warning CA1031
            _onError(New ReceiverError(ex.Message, ex))
            _onStateChanged(Connected)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _watchToken?.Dispose()
        _wsClient?.Dispose()
    End Sub
End Class