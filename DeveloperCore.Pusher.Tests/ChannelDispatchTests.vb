Imports System.Reflection
Imports System.Threading
Imports DeveloperCore.Pusher
Imports NUnit.Framework

Public Class ChannelDispatchTests
    <Test>
    Public Sub UnbindDuringDispatch_PreservesCapturedCallbacks()
        Dim channel As New Channel("test")
        Using entered As New ManualResetEventSlim(), release As New ManualResetEventSlim()
            Dim calls As Integer
            Dim callback As Action(Of Notification) = Sub(notification)
                                                         entered.Set()
                                                         If Not release.Wait(TimeSpan.FromSeconds(5)) Then Throw New TimeoutException()
                                                         Interlocked.Increment(calls)
                                                     End Sub
            channel.Bind("Item", callback)
            channel.Bind("Item", Sub(notification) Interlocked.Increment(calls))
            Dim dispatch = Task.Run(Sub() Deliver(channel, "Item"))
            Try
                Assert.That(entered.Wait(TimeSpan.FromSeconds(5)), [Is].True)
                channel.Unbind("Item", callback)
                channel.UnbindAll()
                channel.Bind("Item", Sub(notification) Interlocked.Add(calls, 10))
            Finally
                release.Set()
            End Try
            dispatch.GetAwaiter().GetResult()
            Assert.That(calls, [Is].EqualTo(2))
            Deliver(channel, "Item")
            Assert.That(calls, [Is].EqualTo(12))
        End Using
    End Sub

    <Test>
    Public Sub CallbackCanChangeSubscriptions_ForTheNextNotification()
        Dim channel As New Channel("test")
        Dim calls As New List(Of String)()
        channel.Bind("Item", Sub(notification)
                                 calls.Add("original")
                                 channel.UnbindAll()
                                 channel.Bind("Item", Sub(nextNotification) calls.Add("new"))
                             End Sub)
        channel.Bind("Item", Sub(notification) calls.Add("second"))
        Deliver(channel, "Item")
        Assert.That(calls, [Is].EqualTo({"original", "second"}))
        Deliver(channel, "Item")
        Assert.That(calls, [Is].EqualTo({"original", "second", "new"}))
    End Sub

    <Test>
    Public Sub Dispatch_PreservesEventFilteringOrderAndDuplicateBindings()
        Dim channel As New Channel("test")
        Dim calls As New List(Of Integer)()
        Dim duplicate As Action(Of Notification) = Sub(notification) calls.Add(2)
        channel.Bind("Item", Sub(notification) calls.Add(1))
        channel.Bind("SalesOrder", Sub(notification) calls.Add(99))
        channel.Bind("Item", duplicate)
        channel.Bind("Item", duplicate)
        Deliver(channel, "Item")
        Assert.That(calls, [Is].EqualTo({1, 2, 2}))
        channel.Unbind("Item", duplicate)
        calls.Clear()
        Deliver(channel, "Item")
        Assert.That(calls, [Is].EqualTo({1}))
    End Sub

    <Test>
    Public Sub CallbackCanWaitForAnotherThreadToBind()
        Dim channel As New Channel("test")
        channel.Bind("Item", Sub(notification)
                                 Dim bind = Task.Run(Sub() channel.Bind("SalesOrder", Sub(other) Return))
                                 Assert.That(bind.Wait(TimeSpan.FromSeconds(5)), [Is].True, "Callbacks must execute outside the subscription lock.")
                             End Sub)
        Deliver(channel, "Item")
    End Sub

    Private Shared Sub Deliver(channel As Channel, eventName As String)
        Dim notification As New Notification("test", eventName, "{}", Date.Now)
        GetType(Channel).GetMethod("MessageReceived", BindingFlags.Instance Or BindingFlags.NonPublic).Invoke(channel, {notification})
    End Sub
End Class
