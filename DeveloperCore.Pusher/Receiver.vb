Imports System.Threading

''' <summary>
''' Receives messages on multiple channels.
''' </summary>
Public Class Receiver
    Private ReadOnly _listener As Listener
    Dim _channels As New List(Of Channel)

    Public Sub New(scheme As String, host As String, port As Integer, key As String)
        _listener = New Listener(scheme, host, port, key, AddressOf Trigger)
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
    Public ReadOnly Property Url As String
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
        Await ConnectAsync(CancellationToken.None)
    End Function

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <param name="token">The cancellation token.</param>
    Public Async Function ConnectAsync(token As CancellationToken) As Task
        Await _listener.ConnectAsync(token)
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
        Await _listener.DisconnectAsync(token)
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
End Class