Public Class Notification
    Public Sub New(channel As String, [event] As String, data As String)
        Me.Channel = channel
        Me.Event = [event]
        Me.Data = data
    End Sub

    Public ReadOnly Property Channel As String
    Public ReadOnly Property [Event] As String
    Public ReadOnly Property Data As String
End Class