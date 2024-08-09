Imports System.Runtime.CompilerServices

Friend Module AsyncExtensions
    <Extension>
    Public Function KeepContext(Of T)(a As Task(Of T)) As ConfiguredTaskAwaitable(Of T)
        Return a.ConfigureAwait(True)
    End Function

    <Extension>
    Public Function FreeContext(Of T)(a As Task(Of T)) As ConfiguredTaskAwaitable(Of T)
        Return a.ConfigureAwait(False)
    End Function
    
    <Extension>
    Public Function KeepContext(a As Task) As ConfiguredTaskAwaitable
        Return a.ConfigureAwait(True)
    End Function

    <Extension>
    Public Function FreeContext(a As Task) As ConfiguredTaskAwaitable
        Return a.ConfigureAwait(False)
    End Function
End Module