Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json

''' <summary>
''' Sends a notification to the server.
''' </summary>
Public Class Sender
    Public Sub New(url As String)
        Me.Url = url
    End Sub

    ''' <summary>
    ''' The base url of the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Url As String

    ''' <summary>
    ''' Sends a notification to the server.
    ''' </summary>
    ''' <param name="channel">The channel to send on.</param>
    ''' <param name="event">The event to trigger.</param>
    ''' <param name="data">Any additional data to send.</param>
    Public Async Function SendAsync(channel As String, [event] As String, data As Object) As Task
        Using client As New HttpClient
            Dim content As New StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
            Dim res = Await client.PostAsync(Path.Combine(Url, $"Notification?channel={channel}&event={[event]}"), content)
        End Using
    End Function
End Class