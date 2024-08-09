Friend NotInheritable Class [Event]
    Public Sub New([event] As String, e As Action(Of Notification))
        Me.Event = [event]
        Action = e
    End Sub

    Public ReadOnly Property [Event] As String
    Public ReadOnly Property Action As Action(Of Notification)
End Class
