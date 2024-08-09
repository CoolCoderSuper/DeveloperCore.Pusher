''' <summary>
''' A notification received from the server.
''' </summary>
Public NotInheritable Class Notification
    Public Sub New(channel As String, [event] As String, data As String, [date] As Date)
        Me.Channel = channel
        Me.Event = [event]
        Me.Data = data
        Me.Date = [date]
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
    
    ''' <summary>
    ''' The date the notification was received.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property [Date] As Date
End Class