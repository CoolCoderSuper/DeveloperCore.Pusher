''' <summary>
''' Represents a channel.
''' </summary>
Public NotInheritable Class Channel
    Dim _events As New List(Of [Event])

    Public Sub New(name As String)
        Me.Name = name
    End Sub
    
    ''' <summary>
    ''' Binds an event to an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Bind([event] As String, e As Action(Of Notification))
        _events.Add(New [Event]([event], e))
    End Sub

    ''' <summary>
    ''' Unbinds an event from an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Unbind([event] As String, e As Action(Of Notification))
        _events.RemoveAll(Function(x) x.Event = [event] AndAlso x.Action = e)
    End Sub
    
    Public Sub UnbindAll()
        _events.Clear()
    End Sub
    
    Friend Sub MessageReceived(n As Notification)
        _events.ForEach(Sub(x) If x.Event = n.Event Then x.Action.Invoke(n))
    End Sub

    ''' <summary>
    ''' The channel name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
End Class