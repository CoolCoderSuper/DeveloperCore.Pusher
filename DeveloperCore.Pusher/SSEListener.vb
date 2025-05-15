Imports System.IO
Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports System.Threading

Public Class SSEListener
    Implements IListener

    Private ReadOnly _trigger As Action(Of Notification)
    Private ReadOnly _onStateChanged As Action(Of Boolean)
    Private _watchToken As CancellationTokenSource
    Private _client As HttpClient

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification), onStateChanged As Action(Of Boolean))
        _trigger = trigger
        _onStateChanged = onStateChanged
        Me.Key = key
        Url = uri
    End Sub

    Public ReadOnly Property Url As Uri Implements IListener.Url
    Public ReadOnly Property Key As String Implements IListener.Key

    Public ReadOnly Property Connected As Boolean Implements IListener.Connected
        Get
            Return _client IsNot Nothing
        End Get
    End Property

    Public Function ConnectAsync(token As CancellationToken) As Task Implements IListener.ConnectAsync
        If Connected Then Return Task.CompletedTask
        _client = New HttpClient() With {.Timeout = Timeout.InfiniteTimeSpan}
        _watchToken = New CancellationTokenSource
        Watch(_watchToken.Token)
        _onStateChanged(Connected)
        Return Task.CompletedTask
    End Function

    Public Function DisconnectAsync(token As CancellationToken) As Task Implements IListener.DisconnectAsync
        If Connected Then
            _watchToken.Cancel()
            _client.Dispose()
            _client = Nothing
            _onStateChanged(Connected)
        End If
        Return Task.CompletedTask
    End Function

    Private Async Sub Watch(token As CancellationToken)
        Try
            Dim uri As New UriBuilder(Url)
            uri.Path = $"sse/{Key}"
            Using request As New HttpRequestMessage(HttpMethod.Get, uri.Uri)
                request.Headers.Add("Accept", "text/event-stream")
                Using response = Await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(False)
                    response.EnsureSuccessStatusCode()
                    Using stream = Await response.Content.ReadAsStreamAsync().ConfigureAwait(False)
                        Using reader = New StreamReader(stream)
                            While Not _watchToken.IsCancellationRequested
                                Dim line As String = Await reader.ReadLineAsync().ConfigureAwait(False)
                                If String.IsNullOrEmpty(line) Then Continue While
                                line = line.Substring(6)
                                Console.WriteLine(line)
                                Dim obj As JsonObject = JsonNode.Parse(line)
                                Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString, Date.Now)
                                _trigger(n)
                            End While
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As OperationCanceledException
        End Try
    End Sub
End Class
