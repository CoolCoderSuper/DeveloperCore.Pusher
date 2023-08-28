Public Class Notification
    Public Sub New(channel As String, [event] As String, data As Object)
        Me.Channel = channel
        Me.Event = [event]
        Me.Data = data
    End Sub
    Public Property Channel As String
    Public Property [Event] As String
    Public Property Data As Object
End Class