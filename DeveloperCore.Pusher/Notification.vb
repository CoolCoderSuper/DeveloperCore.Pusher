''' <summary>
''' A notification received from the server.
''' </summary>
Public Class Notification
    Public Sub New(channel As String, [event] As String, data As String)
        Me.Channel = channel
        Me.Event = [event]
        Me.Data = data
    End Sub

    ''' <summary>
    ''' The channel the notification was received on.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Channel As String

    ''' <summary>
    ''' The event that was triggered.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property [Event] As String

    ''' <summary>
    ''' The data received, a JSON string.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Data As String
End Class