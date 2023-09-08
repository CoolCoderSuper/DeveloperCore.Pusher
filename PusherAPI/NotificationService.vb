Imports Microsoft.Extensions.Configuration

Public Class NotificationService
    Private ReadOnly _config As IConfiguration

    Public Sub New(config As IConfiguration)
        _config = config
    End Sub
    
    Public Function Send(channel As String, [event] As String, key As String, data As Object) As Boolean
        If key <> _config.Item("Key") Then Return False
        Dim notification As New Notification(channel, [event], data)
        RaiseEvent NotificationReceived(notification)
        Return True
    End Function
    
    Public Shared Event NotificationReceived(notification As Notification)
End Class