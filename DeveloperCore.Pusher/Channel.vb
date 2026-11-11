''' <summary>
''' Represents a channel.
''' </summary>
Public NotInheritable Class Channel
    Private ReadOnly _events As New List(Of [Event])
    Private ReadOnly _eventLock As New Object()

    Public Sub New(name As String)
        Me.Name = name
    End Sub
    
    ''' <summary>
    ''' Binds an event to an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Bind([event] As String, e As Action(Of Notification))
        SyncLock _eventLock
            _events.Add(New [Event]([event], e))
        End SyncLock
    End Sub

    ''' <summary>
    ''' Unbinds an event from an action.
    ''' </summary>
    ''' <param name="event">The event name.</param>
    ''' <param name="e">The action to perform.</param>
    Public Sub Unbind([event] As String, e As Action(Of Notification))
        SyncLock _eventLock
            _events.RemoveAll(Function(x) x.Event = [event] AndAlso x.Action = e)
        End SyncLock
    End Sub
    
    ''' <summary>
    ''' Unbinds all events.
    ''' </summary>
    Public Sub UnbindAll()
        SyncLock _eventLock
            _events.Clear()
        End SyncLock
    End Sub
    
    Friend Sub MessageReceived(n As Notification)
        Dim handlers As [Event]()
        SyncLock _eventLock
            handlers = _events.Where(Function(x) x.Event = n.Event).ToArray()
        End SyncLock
        'Invoke outside the lock so callbacks can change subscriptions without blocking dispatch.
        For Each handler In handlers
            handler.Action.Invoke(n)
        Next
    End Sub

    ''' <summary>
    ''' The channel name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
End Class