Imports System.Collections.Concurrent

Public Class NotificationService

    Public Shared ReadOnly Property Clients As New ConcurrentQueue(Of Notification)

    Public Sub Send(channel As String, [event] As String, data As Object)
        If Clients.Count > 1000 Then
            Clients.Clear()
        End If
        Clients.Enqueue(New Notification(channel, [event], data))
    End Sub

End Class

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