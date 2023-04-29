Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Logging

Namespace Controllers

    <ApiController>
    <Route("[controller]")>
    Public Class NotificationController
        Inherits ControllerBase

        Private ReadOnly _logger As ILogger(Of NotificationController)
        Private ReadOnly _service As NotificationService

        Public Sub New(logger As ILogger(Of NotificationController), service As NotificationService)
            _logger = logger
            _service = service
        End Sub

        <HttpPost("")>
        Public Sub Post(channel As String, [event] As String, <FromBody> data As Object)
            _service.Send(channel, [event], data)
        End Sub

    End Class

End Namespace