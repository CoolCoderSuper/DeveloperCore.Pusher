﻿Imports System.Threading

''' <summary>
''' Receives messages on multiple channels.
''' </summary>
Public NotInheritable Class Receiver
    Implements IDisposable
    Private ReadOnly _listener As IListener
    Private ReadOnly _channels As New List(Of Channel)

    Public Sub New(uri As Uri, key As String, options As ReceiverProtocolOptions)
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(uri))
        If options.Protocol = ReceiverProtocol.WebSocket Then
            _listener = New WebSocketListener(uri, key, AddressOf Trigger, Sub(b) RaiseEvent StateChanged(b), Sub(e) RaiseEvent [Error](e), options.BufferSize)
        ElseIf options.Protocol = ReceiverProtocol.SSE Then
            _listener = New SSEListener(uri, key, AddressOf Trigger, Sub(b) RaiseEvent StateChanged(b), Sub(e) RaiseEvent [Error](e))
        End If
    End Sub

    ''' <summary>
    ''' Determines whether the client is connected to the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Connected As Boolean
        Get
            Return _listener.Connected
        End Get
    End Property

    ''' <summary>
    ''' The base URL of the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Url As Uri
        Get
            Return _listener.Url
        End Get
    End Property

    ''' <summary>
    ''' The key to use for authentication.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Key As String
        Get
            Return _listener.Key
        End Get
    End Property

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <returns></returns>
    Public Async Function ConnectAsync() As Task
        Await ConnectAsync(CancellationToken.None).FreeContext()
    End Function

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <param name="token">The cancellation token.</param>
    Public Async Function ConnectAsync(token As CancellationToken) As Task
        Await _listener.ConnectAsync(token).FreeContext()
    End Function

    ''' <summary>
    ''' Disconnects from the server.
    ''' </summary>
    Public Async Function DisconnectAsync() As Task
        Await DisconnectAsync(CancellationToken.None).FreeContext()
    End Function

    ''' <summary>
    ''' Disconnects from the server.
    ''' </summary>
    ''' <param name="token">The cancellation token.</param>
    Public Async Function DisconnectAsync(token As CancellationToken) As Task
        Await _listener.DisconnectAsync(token).FreeContext()
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
    ''' <returns></returns>
    Public Sub Unsubscribe(channel As Channel)
        _channels.Remove(channel)
    End Sub

    Private Sub Trigger(n As Notification)
        For Each ch As Channel In _channels.Where(Function(x) x.Name = n.Channel)
            ch.MessageReceived(n)
        Next
    End Sub

    ''' <summary>
    ''' Occurs when the state of the connection changes.
    ''' </summary>
    Public Event StateChanged(state As Boolean)

    ''' <summary>
    ''' Occurs when an error occurs.
    ''' </summary>
    Public Event [Error]([error] As ReceiverError)

    Public Sub Dispose() Implements IDisposable.Dispose
        _listener.Dispose()
    End Sub
End Class

Public NotInheritable Class ReceiverProtocolOptions
    Public Property Protocol As ReceiverProtocol
    Public Property BufferSize As Integer = WebSocketListener.DefaultBufferSize
End Class

Public Enum ReceiverProtocol
    WebSocket
    SSE
End Enum