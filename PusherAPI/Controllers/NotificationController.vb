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
        Public Function Post(channel As String, [event] As String, key As String, <FromBody> data As Object) As ActionResult
            If _service.Send(channel, [event], key, data) Then
                Return Ok()
            Else 
                Return Unauthorized()
            End If
        End Function
    End Class
End Namespace