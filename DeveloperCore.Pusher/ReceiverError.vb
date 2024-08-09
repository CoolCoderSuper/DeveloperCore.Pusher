Public NotInheritable Class ReceiverError
    Public Sub New(message As String, exception As Exception)
        Me.Message = message
        Me.Exception = exception
    End Sub

    Public ReadOnly Property Message As String
    Public ReadOnly Property Exception As Exception
End Class