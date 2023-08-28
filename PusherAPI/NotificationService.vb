Public Class NotificationService

    Public Shared Event NotificationReceived(notification As Notification)
    
    Public Sub Send(channel As String, [event] As String, data As Object)
        Dim notification As New Notification(channel, [event], data)
        RaiseEvent NotificationReceived(notification)
    End Sub
End Class