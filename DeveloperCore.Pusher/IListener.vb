Imports System.Threading

Public Interface IListener
    ReadOnly Property Connected As Boolean
    ReadOnly Property Key As String
    ReadOnly Property Url As Uri
    Function ConnectAsync(token As CancellationToken) As Task
    Function DisconnectAsync(token As CancellationToken) As Task
End Interface
