Imports System.IO
Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports System.Threading

Public NotInheritable Class SSEListener
    Implements IListener

    Private ReadOnly _trigger As Action(Of Notification)
    Private ReadOnly _onStateChanged As Action(Of Boolean)
    Private ReadOnly _onError As Action(Of ReceiverError)
    Private _watchToken As CancellationTokenSource
    Private _client As HttpClient
    Private _request As HttpRequestMessage
    Private _response As HttpResponseMessage

    Public Sub New(uri As Uri, key As String, trigger As Action(Of Notification), onStateChanged As Action(Of Boolean), onError As Action(Of ReceiverError))
        _trigger = trigger
        _onStateChanged = onStateChanged
        _onError = onError
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

    Public Async Function ConnectAsync(token As CancellationToken) As Task Implements IListener.ConnectAsync
        If Connected Then Return
        _client = New HttpClient() With {.Timeout = Timeout.InfiniteTimeSpan}
        _watchToken = New CancellationTokenSource
        Dim uri As New UriBuilder(Url)
        uri.Path = $"sse/{Key}"
        _request = New HttpRequestMessage(HttpMethod.Get, uri.Uri)
        _request.Headers.Add("Accept", "text/event-stream")
        _response = Await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(False)
        _response.EnsureSuccessStatusCode()
        Watch()
        _onStateChanged(Connected)
        Return
    End Function

    Public Function DisconnectAsync(token As CancellationToken) As Task Implements IListener.DisconnectAsync
        If Connected Then
            _watchToken.Cancel()
            _request.Dispose()
            _request = Nothing
            _response.Dispose()
            _response = Nothing
            _client.Dispose()
            _client = Nothing
            _onStateChanged(Connected)
        End If
        Return Task.CompletedTask
    End Function

    Private Async Sub Watch()
        Try
            Using stream = Await _response.Content.ReadAsStreamAsync().ConfigureAwait(False)
                Using reader = New StreamReader(stream)
                    While Not _watchToken.IsCancellationRequested
                        Dim line As String = Await reader.ReadLineAsync().ConfigureAwait(False)
                        If String.IsNullOrEmpty(line) Then Continue While
                        line = line.Substring(6)
                        If line = "connected" Then
                            Continue While
                        End If
                        Dim obj As JsonObject = JsonNode.Parse(line)
                        Dim n As New Notification(obj("Channel"), obj("Event"), obj("Data").ToJsonString, Date.Now)
                        _trigger(n)
                    End While
                End Using
            End Using
        Catch ex As OperationCanceledException
#Disable Warning CA1031'we want to catch all exceptions
        Catch ex As Exception
#Enable Warning CA1031
            _onError(New ReceiverError(ex.Message, ex))
            _onStateChanged(Connected)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _watchToken?.Dispose()
        _request?.Dispose()
        _response?.Dispose()
        _client?.Dispose()
    End Sub
End Class
