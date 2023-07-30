Namespace NeuralNetworkFactory
    Public Class Tril
        Public Sub Main()
            Dim matrix(,) As Integer = {{1, 2, 3, 9}, {4, 5, 6, 8}, {7, 8, 9, 9}}

            Dim result(,) As Integer = Tril(matrix)

            Console.WriteLine("Matrix:")
            PrintMatrix(matrix)

            Console.WriteLine("Tril Result:")
            PrintMatrix(result)
            Console.ReadLine()
        End Sub


        Public Shared Function Tril(ByVal matrix(,) As Integer) As Integer(,)
            Dim rows As Integer = matrix.GetLength(0)
            Dim cols As Integer = matrix.GetLength(1)

            Dim result(rows - 1, cols - 1) As Integer

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    If j <= i Then
                        result(i, j) = matrix(i, j)
                    End If
                Next
            Next

            Return result
        End Function
        Public Shared Function Tril(ByVal matrix(,) As Double) As Double(,)
            Dim rows As Integer = matrix.GetLength(0)
            Dim cols As Integer = matrix.GetLength(1)

            Dim result(rows - 1, cols - 1) As Double

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    If j <= i Then
                        result(i, j) = matrix(i, j)
                    End If
                Next
            Next

            Return result
        End Function
        Public Shared Function Tril(ByVal matrix As List(Of List(Of Double))) As List(Of List(Of Double))
            Dim rows As Integer = matrix.Count
            Dim cols As Integer = matrix(0).Count

            Dim result As New List(Of List(Of Double))

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    If j <= i Then
                        result(i)(j) = matrix(i)(j)
                    End If
                Next
            Next

            Return result
        End Function
        Public Shared Sub PrintMatrix(ByVal matrix(,) As Double)
            Dim rows As Integer = matrix.GetLength(0)
            Dim cols As Integer = matrix.GetLength(1)

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    Console.Write(matrix(i, j) & " ")
                Next
                Console.WriteLine()
            Next
        End Sub
        Public Shared Sub PrintMatrix(ByVal matrix(,) As Integer)
            Dim rows As Integer = matrix.GetLength(0)
            Dim cols As Integer = matrix.GetLength(1)

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    Console.Write(matrix(i, j) & " ")
                Next
                Console.WriteLine()
            Next
        End Sub
    End Class
End Namespace




