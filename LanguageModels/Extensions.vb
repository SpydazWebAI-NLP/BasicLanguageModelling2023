Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
''' <summary>
''' This is a TFIDF Vectorizer For basic Embeddings
''' </summary>
Public Class SentenceVectorizer
    Private ReadOnly documents As List(Of List(Of String))
    Private ReadOnly idf As Dictionary(Of String, Double)

    Public Sub New(documents As List(Of List(Of String)))
        Me.documents = documents
        Me.idf = CalculateIDF(documents)
    End Sub

    Public Sub New()
        documents = New List(Of List(Of String))
        idf = New Dictionary(Of String, Double)
    End Sub

    Public Function Vectorize(sentence As List(Of String)) As List(Of Double)
        Dim termFrequency = CalculateTermFrequency(sentence)
        Dim vector As New List(Of Double)

        For Each term In idf.Keys
            Dim tfidf As Double = termFrequency(term) * idf(term)
            vector.Add(tfidf)
        Next

        Return vector
    End Function

    Public Function CalculateIDF(documents As List(Of List(Of String))) As Dictionary(Of String, Double)
        Dim idf As New Dictionary(Of String, Double)
        Dim totalDocuments As Integer = documents.Count

        For Each document In documents
            Dim uniqueTerms As List(Of String) = document.Distinct().ToList()

            For Each term In uniqueTerms
                If idf.ContainsKey(term) Then
                    idf(term) += 1
                Else
                    idf(term) = 1
                End If
            Next
        Next

        For Each term In idf.Keys
            idf(term) = Math.Log(totalDocuments / idf(term))
        Next

        Return idf
    End Function

    Public Function CalculateTermFrequency(sentence As List(Of String)) As Dictionary(Of String, Double)
        Dim termFrequency As New Dictionary(Of String, Double)

        For Each term In sentence
            If termFrequency.ContainsKey(term) Then
                termFrequency(term) += 1
            Else
                termFrequency(term) = 1
            End If
        Next

        Return termFrequency
    End Function

End Class
Public Class VocabularyBuilder
    Private embeddingMatrix As Double(,)
    Private embeddingSize As Integer
    Private iterations As Integer
    Private Function GetEmbedding(index As Integer) As Double()
        If indexToWord.ContainsKey(index) Then
            Dim vector(embeddingSize - 1) As Double
            For i As Integer = 0 To embeddingSize - 1
                vector(i) = embeddingMatrix(index, i)
            Next
            Return vector
        Else
            Return Nothing
        End If
    End Function
    Public Function GenerateCooccurrenceMatrix(corpus As String(), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim matrix As New Dictionary(Of String, Dictionary(Of String, Double))

        For Each sentence In corpus
            Dim words As String() = sentence.Split(" "c)
            Dim length As Integer = words.Length

            For i As Integer = 0 To length - 1
                Dim targetWord As String = words(i)

                If Not matrix.ContainsKey(targetWord) Then
                    matrix(targetWord) = New Dictionary(Of String, Double)
                End If

                For j As Integer = Math.Max(0, i - windowSize) To Math.Min(length - 1, i + windowSize)
                    If i = j Then
                        Continue For
                    End If

                    Dim contextWord As String = words(j)
                    Dim distance As Double = 1 / Math.Abs(i - j)

                    If matrix(targetWord).ContainsKey(contextWord) Then
                        matrix(targetWord)(contextWord) += distance
                    Else
                        matrix(targetWord)(contextWord) = distance
                    End If
                Next
            Next
        Next

        Return matrix
    End Function
    Public Model As New Dictionary(Of String, Dictionary(Of String, Double))
    Private windowSize As Integer
    Public Sub Train(corpus As String(), ByRef WindowSize As Integer, ByRef Iterations As Integer)
        BuildVocabulary(corpus.ToList)
        InitializeEmbeddings()
        Model = GenerateCooccurrenceMatrix(corpus, WindowSize)

        For iteration As Integer = 1 To Iterations
            For Each targetWord In Model.Keys
                Dim targetIndex As Integer = GetOrCreateWordIndex(targetWord)
                Dim targetEmbedding As Double() = GetEmbedding(targetIndex)

                For Each contextWord In Model(targetWord).Keys
                    Dim contextIndex As Integer = GetOrCreateWordIndex(contextWord)
                    Dim contextEmbedding As Double() = GetEmbedding(contextIndex)
                    Dim cooccurrenceValue As Double = Model(targetWord)(contextWord)
                    Dim weight As Double = Math.Log(cooccurrenceValue)

                    For i As Integer = 0 To embeddingSize - 1
                        targetEmbedding(i) += weight * contextEmbedding(i)
                    Next
                Next
            Next
        Next
        Model = PMI.CalculatePMI(Model)
    End Sub
    Public Sub Train(corpus As String(), iterations As Integer, ByRef LearningRate As Double)

        Me.iterations = iterations
        Model = GenerateCooccurrenceMatrix(corpus, windowSize)
        InitializeEmbeddings()

        For iteration As Integer = 1 To iterations
            For Each targetWord In Model.Keys
                If wordToIndex.ContainsKey(targetWord) Then
                    Dim targetIndex As Integer = wordToIndex(targetWord)
                    Dim targetEmbedding As Double() = GetEmbedding(targetIndex)

                    ' Initialize gradient accumulator for target embedding
                    Dim gradTarget As Double() = New Double(embeddingSize - 1) {}

                    For Each contextWord In Model(targetWord).Keys
                        If wordToIndex.ContainsKey(contextWord) Then
                            Dim contextIndex As Integer = wordToIndex(contextWord)
                            Dim contextEmbedding As Double() = GetEmbedding(contextIndex)
                            Dim cooccurrenceValue As Double = Model(targetWord)(contextWord)
                            Dim weight As Double = Math.Log(cooccurrenceValue)

                            ' Initialize gradient accumulator for context embedding
                            Dim gradContext As Double() = New Double(embeddingSize - 1) {}

                            ' Calculate the gradients
                            For i As Integer = 0 To embeddingSize - 1
                                Dim gradCoefficient As Double = weight * targetEmbedding(i)

                                gradTarget(i) = LearningRate * gradCoefficient
                                gradContext(i) = LearningRate * gradCoefficient
                            Next

                            ' Update the target and context embeddings
                            For i As Integer = 0 To embeddingSize - 1
                                targetEmbedding(i) += gradTarget(i)
                                contextEmbedding(i) += gradContext(i)
                            Next
                        End If
                    Next
                End If
            Next
        Next
    End Sub


    Private Sub InitializeEmbeddings()
        Dim vocabSize As Integer = vocabulary.Count
        embeddingMatrix = New Double(vocabSize - 1, embeddingSize - 1) {}

        Dim random As New Random()
        For i As Integer = 0 To vocabSize - 1
            For j As Integer = 0 To embeddingSize - 1
                embeddingMatrix(i, j) = random.NextDouble()
            Next
        Next
    End Sub
    Private Function CalculateSimilarity(vectorA As Double(), vectorB As Double()) As Double
        Dim dotProduct As Double = 0
        Dim magnitudeA As Double = 0
        Dim magnitudeB As Double = 0

        For i As Integer = 0 To vectorA.Length - 1
            dotProduct += vectorA(i) * vectorB(i)
            magnitudeA += vectorA(i) * vectorA(i)
            magnitudeB += vectorB(i) * vectorB(i)
        Next

        If magnitudeA <> 0 AndAlso magnitudeB <> 0 Then
            Return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB))
        Else
            Return 0
        End If
    End Function
    ''' <summary>
    ''' Discovers collocations among the specified words based on the trained model.
    ''' </summary>
    ''' <param name="words">The words to discover collocations for.</param>
    ''' <param name="threshold">The similarity threshold for collocation discovery.</param>
    ''' <returns>A list of collocations (word pairs) that meet the threshold.</returns>
    Public Function DiscoverCollocations(words As String(), threshold As Double) As List(Of Tuple(Of String, String))
        Dim collocations As New List(Of Tuple(Of String, String))

        For i As Integer = 0 To words.Length - 2
            For j As Integer = i + 1 To words.Length - 1
                Dim word1 As String = words(i)
                Dim word2 As String = words(j)

                If vocabulary.Contains(word1) AndAlso vocabulary.Contains(word2) Then
                    Dim vector1 As Double() = GetEmbedding(wordToIndex(word1))
                    Dim vector2 As Double() = GetEmbedding(wordToIndex(word2))
                    Dim similarity As Double = CalculateSimilarity(vector1, vector2)

                    If similarity >= threshold Then
                        collocations.Add(Tuple.Create(word1, word2))
                    End If
                End If
            Next
        Next

        Return collocations
    End Function
    ''' <summary>
    ''' Gets the most similar words to the specified word.
    ''' </summary>
    ''' <param name="word">The target word.</param>
    ''' <param name="topK">The number of similar words to retrieve.</param>
    ''' <returns>A list of the most similar words.</returns>
    Public Function GetMostSimilarWords(word As String, topK As Integer) As List(Of String)
        Dim wordIndex As Integer = wordToIndex(word)

        Dim similarities As New Dictionary(Of String, Double)()
        For Each otherWord As String In vocabulary
            If otherWord <> word Then
                Dim otherWordIndex As Integer = wordToIndex(otherWord)
                Dim similarity As Double = CalculateSimilarity(GetEmbedding(wordIndex), GetEmbedding(otherWordIndex))
                similarities.Add(otherWord, similarity)
            End If
        Next

        Dim orderedSimilarities = similarities.OrderByDescending(Function(x) x.Value)
        Dim mostSimilarWords As New List(Of String)()

        Dim count As Integer = 0
        For Each pair In orderedSimilarities
            mostSimilarWords.Add(pair.Key)
            count += 1
            If count >= topK Then
                Exit For
            End If
        Next

        Return mostSimilarWords
    End Function
    Private vocabulary As HashSet(Of String)
    Private wordToIndex As Dictionary(Of String, Integer)
    Private indexToWord As Dictionary(Of Integer, String)
    Public Function BuildVocabulary(corpus As List(Of String)) As HashSet(Of String)

        Dim index As Integer = 0
        For Each sentence As String In corpus
            Dim cleanedText As String = Regex.Replace(sentence, "[^\w\s]", "").ToLower()
            Dim tokens As String() = cleanedText.Split()
            For Each token As String In tokens
                If Not vocabulary.Contains(token) Then
                    vocabulary.Add(token)
                    wordToIndex.Add(token, index)
                    indexToWord.Add(index, token)
                    index += 1
                End If
            Next
        Next
        Return vocabulary
    End Function
    Public Function GetOrCreateWordIndex(word As String) As Integer
        If wordToIndex.ContainsKey(word) Then
            Return wordToIndex(word)
        Else
            Dim newIndex As Integer = vocabulary.Count
            vocabulary.Add(word)
            wordToIndex.Add(word, newIndex)
            indexToWord.Add(newIndex, word)
            Return newIndex
        End If
    End Function
    Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
        Dim dataGridView As New DataGridView()
        dataGridView.Dock = DockStyle.Fill
        dataGridView.AutoGenerateColumns = False
        dataGridView.AllowUserToAddRows = False

        ' Add columns to the DataGridView
        Dim wordColumn As New DataGridViewTextBoxColumn()
        wordColumn.HeaderText = "Word"
        wordColumn.DataPropertyName = "Word"
        dataGridView.Columns.Add(wordColumn)

        For Each contextWord As String In matrix.Keys
            Dim contextColumn As New DataGridViewTextBoxColumn()
            contextColumn.HeaderText = contextWord
            contextColumn.DataPropertyName = contextWord
            dataGridView.Columns.Add(contextColumn)
        Next

        ' Populate the DataGridView with the matrix data
        For Each word As String In matrix.Keys
            Dim rowValues As New List(Of Object)
            rowValues.Add(word)

            For Each contextWord As String In matrix.Keys
                Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                rowValues.Add(count)
            Next

            dataGridView.Rows.Add(rowValues.ToArray())
        Next

        Return dataGridView
    End Function
    Public Sub DisplayModel()
        DisplayMatrix(Model)
    End Sub
    Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
        Dim dataGridView As New DataGridView()
        dataGridView.Dock = DockStyle.Fill
        dataGridView.AutoGenerateColumns = False
        dataGridView.AllowUserToAddRows = False

        ' Add columns to the DataGridView
        Dim wordColumn As New DataGridViewTextBoxColumn()
        wordColumn.HeaderText = "Word"
        wordColumn.DataPropertyName = "Word"
        dataGridView.Columns.Add(wordColumn)

        For Each contextWord As String In matrix.Keys
            Dim contextColumn As New DataGridViewTextBoxColumn()
            contextColumn.HeaderText = contextWord
            contextColumn.DataPropertyName = contextWord
            dataGridView.Columns.Add(contextColumn)
        Next

        ' Populate the DataGridView with the matrix data
        For Each word As String In matrix.Keys
            Dim rowValues As New List(Of Object)()
            rowValues.Add(word)

            For Each contextWord As String In matrix.Keys
                Dim count As Integer = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                rowValues.Add(count)
            Next

            dataGridView.Rows.Add(rowValues.ToArray())
        Next

        Return dataGridView
    End Function

End Class
Public Class PMI
    ''' <summary>
    ''' Calculates the Pointwise Mutual Information (PMI) matrix for the trained model.
    ''' </summary>
    ''' <returns>A dictionary representing the PMI matrix.</returns>
    Public Shared Function CalculatePMI(ByRef model As Dictionary(Of String, Dictionary(Of String, Double))) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim pmiMatrix As New Dictionary(Of String, Dictionary(Of String, Double))

        Dim totalCooccurrences As Double = GetTotalCooccurrences(model)

        For Each targetWord In model.Keys
            Dim targetOccurrences As Double = GetTotalOccurrences(targetWord, model)

            If Not pmiMatrix.ContainsKey(targetWord) Then
                pmiMatrix(targetWord) = New Dictionary(Of String, Double)
            End If

            For Each contextWord In model(targetWord).Keys
                Dim contextOccurrences As Double = GetTotalOccurrences(contextWord, model)
                Dim cooccurrences As Double = model(targetWord)(contextWord)

                Dim pmiValue As Double = Math.Log((cooccurrences * totalCooccurrences) / (targetOccurrences * contextOccurrences))
                pmiMatrix(targetWord)(contextWord) = pmiValue
            Next
        Next

        Return pmiMatrix
    End Function
    Public Shared Function GetTotalCooccurrences(ByRef Model As Dictionary(Of String, Dictionary(Of String, Double))) As Double
        Dim total As Double = 0

        For Each targetWord In Model.Keys
            For Each cooccurrenceValue In Model(targetWord).Values
                total += cooccurrenceValue
            Next
        Next

        Return total
    End Function
    Public Shared Function GetTotalOccurrences(word As String, ByRef Model As Dictionary(Of String, Dictionary(Of String, Double))) As Double
        Dim total As Double = 0

        If Model.ContainsKey(word) Then
            total = Model(word).Values.Sum()
        End If

        Return total
    End Function
    Public Shared Function CalculateCosineSimilarity(vectorA As Double(), vectorB As Double()) As Double
        Dim dotProduct As Double = 0
        Dim magnitudeA As Double = 0
        Dim magnitudeB As Double = 0

        For i As Integer = 0 To vectorA.Length - 1
            dotProduct += vectorA(i) * vectorB(i)
            magnitudeA += vectorA(i) * vectorA(i)
            magnitudeB += vectorB(i) * vectorB(i)
        Next

        If magnitudeA <> 0 AndAlso magnitudeB <> 0 Then
            Return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB))
        Else
            Return 0
        End If
    End Function
    Public Shared Function GenerateCooccurrenceMatrix(corpus As String(), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim matrix As New Dictionary(Of String, Dictionary(Of String, Double))

        For Each sentence In corpus
            Dim words As String() = sentence.Split(" "c)
            Dim length As Integer = words.Length

            For i As Integer = 0 To length - 1
                Dim targetWord As String = words(i)

                If Not matrix.ContainsKey(targetWord) Then
                    matrix(targetWord) = New Dictionary(Of String, Double)
                End If

                For j As Integer = Math.Max(0, i - windowSize) To Math.Min(length - 1, i + windowSize)
                    If i = j Then
                        Continue For
                    End If

                    Dim contextWord As String = words(j)
                    Dim distance As Double = 1 / Math.Abs(i - j)

                    If matrix(targetWord).ContainsKey(contextWord) Then
                        matrix(targetWord)(contextWord) += distance
                    Else
                        matrix(targetWord)(contextWord) = distance
                    End If
                Next
            Next
        Next

        Return matrix
    End Function

End Class
Public Class WordListReader
    Private wordList As List(Of String)

    Public Sub New(filePath As String)
        wordList = New List(Of String)()
        ReadWordList(filePath)
    End Sub

    Private Sub ReadWordList(filePath As String)
        Using reader As New StreamReader(filePath)
            While Not reader.EndOfStream
                Dim line As String = reader.ReadLine()
                If Not String.IsNullOrEmpty(line) Then
                    wordList.Add(line.Trim.ToLower)
                End If
            End While
        End Using
    End Sub

    Public Function GetWords() As List(Of String)
        Return wordList
    End Function
    ' Usage Example:
    Public Shared Sub Main()
        ' Assuming you have a wordlist file named 'words.txt' in the same directory
        Dim corpusRoot As String = "."
        Dim wordlistPath As String = Path.Combine(corpusRoot, "wordlist.txt")

        Dim wordlistReader As New WordListReader(wordlistPath)
        Dim words As List(Of String) = wordlistReader.GetWords()

        For Each word As String In words
            Console.WriteLine(word)
        Next
        Console.ReadLine()
        ' Rest of your code...
    End Sub


End Class
''' <summary>
''' Returns a list WordGram Probability Given a Sequence of Tokens 
''' </summary>
Public Class Wordgram
    Private n As Integer
    Public Shared Sub Main()
        ' Train the wordgram model
        Dim trainingData As New List(Of String) From {"I love cats and dogs.", "Dogs are loyal companions."}
        Dim words As New List(Of String) From {
            "apple", "banana", "orange", "apple", "pear", "kiwi", "orange", "mango", "kiwi", "guava", "kiwi", "orange", "orange", "apple", "banana"
        }
        Dim sentences As New List(Of String) From {
            "I love apples.",
            "Bananas are tasty.",
            "I love apples.",
            "I enjoy eating bananas.",
            "mango is a delicious fruit.", "Bananas are tasty.",
            "I love apples.", "I enjoy eating bananas.",
            "Kiwi is a delicious fruit.", "I love apples.",
            "I enjoy eating bananas.",
            "orange is a delicious fruit.", "I love apples.",
            "I enjoy eating bananas.",
            "Kiwi is a delicious fruit.", "Bananas are tasty."
        }
        Dim Corpus As New List(Of String)
        Corpus.AddRange(sentences)
        Corpus.AddRange(words)


        ' Generate a sentence using the wordgram model
        For I = 1 To 5
            Dim wordgramModel As New Wordgram(Corpus, I)
            Dim generatedSentence As String = wordgramModel.GenerateSentence()
            Console.WriteLine(generatedSentence)
        Next I
        Console.ReadLine()
    End Sub

    Public wordgramCounts As New Dictionary(Of List(Of String), Integer)
    Public wordgramProbabilities As New Dictionary(Of List(Of String), Double)
    Public Sub New(trainingData As List(Of String), n As Integer)
        Me.n = n
        TrainModel(trainingData)
    End Sub
    Private Sub TrainModel(trainingData As List(Of String))
        ' Preprocess training data and tokenize into wordgrams
        Dim wordgrams As New List(Of List(Of String))
        For Each sentence As String In trainingData
            Dim tokens() As String = sentence.Split(" "c)
            For i As Integer = 0 To tokens.Length - n
                Dim wordgram As List(Of String) = tokens.Skip(i).Take(n).ToList()
                wordgrams.Add(wordgram)
            Next
        Next

        ' Count wordgrams
        For Each wordgram As List(Of String) In wordgrams
            If wordgramCounts.ContainsKey(wordgram) Then
                wordgramCounts(wordgram) += 1
            Else
                wordgramCounts.Add(wordgram, 1)
            End If
        Next

        ' Calculate wordgram probabilities
        Dim totalCount As Integer = wordgramCounts.Values.Sum()
        For Each wordgram As List(Of String) In wordgramCounts.Keys
            Dim count As Integer = wordgramCounts(wordgram)
            Dim probability As Double = count / totalCount
            wordgramProbabilities.Add(wordgram, probability)
        Next
    End Sub
    Private Function GenerateNextWord(wordgram As List(Of String)) As String
        Dim random As New Random()
        Dim candidates As New List(Of String)
        Dim probabilities As New List(Of Double)

        ' Collect candidate words and their probabilities
        For Each candidateWordgram As List(Of String) In wordgramCounts.Keys
            If candidateWordgram.GetRange(0, n - 1).SequenceEqual(wordgram) Then
                Dim candidateWord As String = candidateWordgram.Last()
                Dim probability As Double = wordgramProbabilities(candidateWordgram)
                candidates.Add(candidateWord)
                probabilities.Add(probability)
            End If
        Next

        ' Randomly select a candidate word based on probabilities
        Dim totalProbability As Double = probabilities.Sum()
        Dim randomValue As Double = random.NextDouble() * totalProbability
        Dim cumulativeProbability As Double = 0

        For i As Integer = 0 To candidates.Count - 1
            cumulativeProbability += probabilities(i)
            If randomValue <= cumulativeProbability Then
                Return candidates(i)
            End If
        Next

        Return ""
    End Function
    Public Function GenerateSentence() As String
        Dim sentence As New List(Of String)
        Dim random As New Random()

        ' Start the sentence with a random wordgram
        Dim randomIndex As Integer = random.Next(0, wordgramCounts.Count)
        Dim currentWordgram As List(Of String) = wordgramCounts.Keys(randomIndex)
        sentence.AddRange(currentWordgram)

        ' Generate subsequent words based on wordgram probabilities
        While wordgramCounts.ContainsKey(currentWordgram)
            Dim nextWord As String = GenerateNextWord(currentWordgram)
            If nextWord = "" Then
                Exit While
            End If
            sentence.Add(nextWord)

            ' Backoff to lower-order wordgrams if necessary
            If currentWordgram.Count > 1 Then
                currentWordgram.RemoveAt(0)
            Else
                Exit While
            End If
            currentWordgram.Add(nextWord)
        End While

        Return String.Join(" ", sentence)
    End Function
    Private Sub Train(trainingData As List(Of String))
        ' Preprocess training data and tokenize into wordgrams
        Dim wordgrams As New List(Of List(Of String))
        For Each sentence As String In trainingData
            Dim tokens() As String = sentence.Split(" "c)
            For i As Integer = 0 To tokens.Length - n
                Dim wordgram As List(Of String) = tokens.Skip(i).Take(n).ToList()
                wordgrams.Add(wordgram)
            Next
        Next

        ' Count wordgrams
        For Each wordgram As List(Of String) In wordgrams
            If wordgramCounts.ContainsKey(wordgram) Then
                wordgramCounts(wordgram) += 1
            Else
                wordgramCounts.Add(wordgram, 1)
            End If
        Next

        ' Calculate wordgram probabilities based on frequency-based distribution
        For Each wordgram As List(Of String) In wordgramCounts.Keys
            Dim count As Integer = wordgramCounts(wordgram)
            Dim order As Integer = wordgram.Count

            ' Calculate the frequency threshold for higher-order n-grams
            Dim frequencyThreshold As Integer = 5 ' Set your desired threshold
            If order = n AndAlso count >= frequencyThreshold Then
                wordgramProbabilities.Add(wordgram, count)
            ElseIf order < n AndAlso count >= frequencyThreshold Then
                ' Assign the frequency to lower-order n-grams
                Dim lowerOrderWordgram As List(Of String) = wordgram.Skip(1).ToList()
                If wordgramProbabilities.ContainsKey(lowerOrderWordgram) Then
                    wordgramProbabilities(lowerOrderWordgram) += count
                Else
                    wordgramProbabilities.Add(lowerOrderWordgram, count)
                End If
            End If
        Next

        ' Normalize probabilities within each order
        For order As Integer = 1 To n
            Dim totalProbability As Double = 0
            For Each wordgram As List(Of String) In wordgramProbabilities.Keys.ToList()
                If wordgram.Count = order Then
                    totalProbability += wordgramProbabilities(wordgram)
                End If
            Next
            For Each wordgram As List(Of String) In wordgramProbabilities.Keys.ToList()
                If wordgram.Count = order Then
                    wordgramProbabilities(wordgram) /= totalProbability
                End If
            Next
        Next
    End Sub


End Class
Public Class Co_Occurrence_Matrix
    Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer)), entityList As List(Of String)) As String
        ' Prepare the header row
        Dim headerRow As String = "|               |"

        For Each entity As String In entityList
            If coOccurrenceMatrix.ContainsKey(entity) Then
                headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
            End If
        Next

        Dim str As String = ""
        ' Print the header row
        Console.WriteLine(headerRow)

        str &= headerRow & vbNewLine
        ' Print the co-occurrence matrix
        For Each entity As String In coOccurrenceMatrix.Keys
            Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

            For Each coOccurringEntity As String In entityList
                Dim count As Integer = 0
                If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                    count = coOccurrenceMatrix(entity)(coOccurringEntity)
                End If
                rowString &= $"{count.ToString().PadLeft(7)} "
            Next

            Console.WriteLine(rowString)
            str &= rowString & vbNewLine
        Next
        Return str
    End Function

    ''' <summary>
    ''' The co-occurrence matrix shows the frequency of co-occurrences between different entities in the given text. Each row represents an entity, and each column represents another entity. The values in the matrix indicate how many times each entity appeared within the specified window size of the other entities. A value of 0 means that the two entities did not co-occur within the given window size.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="entityList"></param>
    ''' <param name="windowSize"></param>
    ''' <returns></returns>
    Public Shared Function iCoOccurrenceMatrix(text As String, entityList As List(Of String), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Integer))
        Dim coOccurrenceMatrix As New Dictionary(Of String, Dictionary(Of String, Integer))

        Dim words() As String = text.ToLower().Split(" "c) ' Convert the text to lowercase here
        For i As Integer = 0 To words.Length - 1
            If entityList.Contains(words(i)) Then
                Dim entity As String = words(i)
                If Not coOccurrenceMatrix.ContainsKey(entity) Then
                    coOccurrenceMatrix(entity) = New Dictionary(Of String, Integer)()
                End If

                For j As Integer = i - windowSize To i + windowSize
                    If j >= 0 AndAlso j < words.Length AndAlso i <> j AndAlso entityList.Contains(words(j)) Then
                        Dim coOccurringEntity As String = words(j)
                        If Not coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                            coOccurrenceMatrix(entity)(coOccurringEntity) = 0
                        End If

                        coOccurrenceMatrix(entity)(coOccurringEntity) += 1
                    End If
                Next
            End If
        Next

        Return coOccurrenceMatrix
    End Function

    ''' <summary>
    ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
    '''
    '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
    '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
    '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
    ''' </summary>
    ''' <param name="coOccurrenceMatrix"></param>
    ''' <returns></returns>
    Public Shared Function CalculatePMI(coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim pmiMatrix As New Dictionary(Of String, Dictionary(Of String, Double))

        For Each entity As String In coOccurrenceMatrix.Keys
            Dim entityOccurrences As Integer = coOccurrenceMatrix(entity).Sum(Function(kv) kv.Value)

            If Not pmiMatrix.ContainsKey(entity) Then
                pmiMatrix(entity) = New Dictionary(Of String, Double)()
            End If

            For Each coOccurringEntity As String In coOccurrenceMatrix(entity).Keys
                Dim coOccurringEntityOccurrences As Integer = coOccurrenceMatrix(entity)(coOccurringEntity)

                Dim pmi As Double = Math.Log((coOccurringEntityOccurrences * coOccurrenceMatrix.Count) / (entityOccurrences * coOccurrenceMatrix(coOccurringEntity).Sum(Function(kv) kv.Value)), 2)
                pmiMatrix(entity)(coOccurringEntity) = pmi
            Next
        Next

        Return pmiMatrix
    End Function
    Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Double)), entityList As List(Of String)) As String
        ' Prepare the header row
        Dim headerRow As String = "|               |"

        For Each entity As String In entityList
            If coOccurrenceMatrix.ContainsKey(entity) Then
                headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
            End If
        Next

        Dim str As String = ""
        ' Print the header row
        Console.WriteLine(headerRow)

        str &= headerRow & vbNewLine
        ' Print the co-occurrence matrix
        For Each entity As String In coOccurrenceMatrix.Keys
            Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

            For Each coOccurringEntity As String In entityList
                Dim count As Integer = 0
                If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                    count = coOccurrenceMatrix(entity)(coOccurringEntity)
                End If
                rowString &= $"{count.ToString().PadLeft(7)} "
            Next

            Console.WriteLine(rowString)
            str &= rowString & vbNewLine
        Next
        Return str
    End Function
    ''' <summary>
    ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
    '''
    '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
    '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
    '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
    ''' </summary>
    ''' <param name="coOccurrenceMatrix"></param>
    ''' <returns></returns>
    Public Shared Function GetPM_Matrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))

        Dim pmiMatrix As Dictionary(Of String, Dictionary(Of String, Double)) = CalculatePMI(coOccurrenceMatrix)
        Return pmiMatrix

    End Function


End Class
Public Class Word2WordMatrix
    Private matrix As Dictionary(Of String, Dictionary(Of String, Integer))

    Public Sub New()
        matrix = New Dictionary(Of String, Dictionary(Of String, Integer))
    End Sub
    Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
        Dim dataGridView As New DataGridView()
        dataGridView.Dock = DockStyle.Fill
        dataGridView.AutoGenerateColumns = False
        dataGridView.AllowUserToAddRows = False

        ' Add columns to the DataGridView
        Dim wordColumn As New DataGridViewTextBoxColumn()
        wordColumn.HeaderText = "Word"
        wordColumn.DataPropertyName = "Word"
        dataGridView.Columns.Add(wordColumn)

        For Each contextWord As String In matrix.Keys
            Dim contextColumn As New DataGridViewTextBoxColumn()
            contextColumn.HeaderText = contextWord
            contextColumn.DataPropertyName = contextWord
            dataGridView.Columns.Add(contextColumn)
        Next

        ' Populate the DataGridView with the matrix data
        For Each word As String In matrix.Keys
            Dim rowValues As New List(Of Object)
            rowValues.Add(word)

            For Each contextWord As String In matrix.Keys
                Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                rowValues.Add(count)
            Next

            dataGridView.Rows.Add(rowValues.ToArray())
        Next

        Return dataGridView
    End Function

    Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
        Dim dataGridView As New DataGridView()
        dataGridView.Dock = DockStyle.Fill
        dataGridView.AutoGenerateColumns = False
        dataGridView.AllowUserToAddRows = False

        ' Add columns to the DataGridView
        Dim wordColumn As New DataGridViewTextBoxColumn()
        wordColumn.HeaderText = "Word"
        wordColumn.DataPropertyName = "Word"
        dataGridView.Columns.Add(wordColumn)

        For Each contextWord As String In matrix.Keys
            Dim contextColumn As New DataGridViewTextBoxColumn()
            contextColumn.HeaderText = contextWord
            contextColumn.DataPropertyName = contextWord
            dataGridView.Columns.Add(contextColumn)
        Next

        ' Populate the DataGridView with the matrix data
        For Each word As String In matrix.Keys
            Dim rowValues As New List(Of Object)()
            rowValues.Add(word)

            For Each contextWord As String In matrix.Keys
                Dim count As Integer = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                rowValues.Add(count)
            Next

            dataGridView.Rows.Add(rowValues.ToArray())
        Next

        Return dataGridView
    End Function

    Public Sub AddDocument(document As String, contextWindow As Integer)
        Dim words As String() = document.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)

        For i As Integer = 0 To words.Length - 1
            Dim currentWord As String = words(i)

            If Not matrix.ContainsKey(currentWord) Then
                matrix(currentWord) = New Dictionary(Of String, Integer)()
            End If

            For j As Integer = Math.Max(0, i - contextWindow) To Math.Min(words.Length - 1, i + contextWindow)
                If i <> j Then
                    Dim contextWord As String = words(j)

                    If Not matrix(currentWord).ContainsKey(contextWord) Then
                        matrix(currentWord)(contextWord) = 0
                    End If

                    matrix(currentWord)(contextWord) += 1
                End If
            Next
        Next
    End Sub
    Public Shared Sub Main()
        ' Fill the matrix with your data
        Dim documents As List(Of String) = New List(Of String)()
        documents.Add("This is the first document.")
        documents.Add("The second document is here.")
        documents.Add("And this is the third document.")

        Dim contextWindow As Integer = 1
        Dim matrixBuilder As New Word2WordMatrix()

        For Each document As String In documents
            matrixBuilder.AddDocument(document, contextWindow)
        Next

        Dim wordWordMatrix As Dictionary(Of String, Dictionary(Of String, Integer)) = matrixBuilder.GetWordWordMatrix()

        ' Create the DataGridView control
        Dim dataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(wordWordMatrix)

        ' Create a form and add the DataGridView to it
        Dim form As New Form()
        form.Text = "Word-Word Matrix"
        form.Size = New Size(800, 600)
        form.Controls.Add(dataGridView)

        ' Display the form
        Application.Run(form)
    End Sub
    Public Function GetWordWordMatrix() As Dictionary(Of String, Dictionary(Of String, Integer))
        Return matrix
    End Function
End Class
