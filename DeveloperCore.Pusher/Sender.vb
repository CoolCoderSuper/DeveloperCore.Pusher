Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json

''' <summary>
''' Sends a notification to the server.
''' </summary>
Public Class Sender
    Public Sub New(url As String, key As String)
        Me.Url = url
        Me.Key = key
    End Sub

    ''' <summary>
    ''' The base url of the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Url As String

    ''' <summary>
    ''' The key to use for authentication.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Key As String

    ''' <summary>
    ''' Sends a notification to the server.
    ''' </summary>
    ''' <param name="channel">The channel to send on.</param>
    ''' <param name="event">The event to trigger.</param>
    ''' <param name="data">Any additional data to send.</param>
    Public Async Function SendAsync(channel As String, [event] As String, data As Object) As Task
        Using client As New HttpClient
            Dim content As New StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
            Dim res = Await client.PostAsync(Path.Combine(Url, $"Notification?channel={channel}&event={[event]}&key={Key}"), content)
            If res.StatusCode <> Net.HttpStatusCode.OK Then
                Throw New Exception($"Failed to send notification: {res.StatusCode}")
            End If
        End Using
    End Function
End Class