Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json

Public Class Sender
    Public Sub New(url As String, channel As String)
        Me.Url = url
        Me.Channel = channel
    End Sub

    Public ReadOnly Property Url As String
    Public ReadOnly Property Channel As String

    Public Async Function Send([event] As String, data As Object) As Task
        Dim client As New HttpClient
        Dim content As New StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
        Dim res = Await client.PostAsync(Path.Combine(Url, $"Notification?channel={Channel}&event={[event]}"), content)
        client.Dispose()
    End Function
End Class