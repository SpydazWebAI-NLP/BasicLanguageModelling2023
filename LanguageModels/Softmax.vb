Namespace NeuralNetworkFactory
    Public Class Softmax
        Public Shared Function Softmax(matrix2 As Integer(,)) As Double(,)
            Dim numRows As Integer = matrix2.GetLength(0)
            Dim numColumns As Integer = matrix2.GetLength(1)

            Dim softmaxValues(numRows - 1, numColumns - 1) As Double

            ' Compute softmax values for each row
            For i As Integer = 0 To numRows - 1
                Dim rowSum As Double = 0

                ' Compute exponential values and sum of row elements
                For j As Integer = 0 To numColumns - 1
                    softmaxValues(i, j) = Math.Sqrt(Math.Exp(matrix2(i, j)))
                    rowSum += softmaxValues(i, j)
                Next

                ' Normalize softmax values for the row
                For j As Integer = 0 To numColumns - 1
                    softmaxValues(i, j) /= rowSum
                Next
            Next

            ' Display the softmax values
            Console.WriteLine("Calculated:" & vbNewLine)
            For i As Integer = 0 To numRows - 1
                For j As Integer = 0 To numColumns - 1

                    Console.Write(softmaxValues(i, j).ToString("0.0000") & " ")
                Next
                Console.WriteLine(vbNewLine & "---------------------")
            Next
            Return softmaxValues
        End Function
        Public Shared Sub Main()
            Dim input() As Double = {1.0, 2.0, 3.0}

            Dim output() As Double = Softmax(input)

            Console.WriteLine("Input: {0}", String.Join(", ", input))
            Console.WriteLine("Softmax Output: {0}", String.Join(", ", output))
            Console.ReadLine()
        End Sub

        Public Shared Function Softmax(ByVal input() As Double) As Double()
            Dim maxVal As Double = input.Max()

            Dim exponentiated() As Double = input.Select(Function(x) Math.Exp(x - maxVal)).ToArray()

            Dim sum As Double = exponentiated.Sum()

            Dim softmaxOutput() As Double = exponentiated.Select(Function(x) x / sum).ToArray()

            Return softmaxOutput
        End Function
    End Class
End Namespace




