Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization
Imports LanguageModelling.Common_NLP_Tasks.LanguageModelling
Imports LanguageModelling.LanguageModels.BaseModels.LanguageModelFactory.Corpus.Document.Sentence
Imports LanguageModelling.LanguageModels.BaseModels.LanguageModelFactory.NgramModels
Imports LanguageModelling.LanguageModels.BaseModels.LanguageModelFactory.NgramModels.BaseModels
Imports LanguageModelling.LanguageModels.BaseModels.LanguageModelFactory.NgramModels.NgramFunctions
Imports LanguageModelling.LanguageModels.BaseModels.LanguageModelFactory.PredictiveLanguageModel
Imports LanguageModelling.NeuralNetworkFactory


Namespace LanguageModels

    Namespace BaseModels
        Public Module Helper
            ' Latent Dirichlet Allocation (LDA) algorithm
            Public Class Latent_Dirichlet_Allocation




                'Public Class Document
                '    Public Property Words As List(Of Word)
                'End Class



                Public Class WordCount
                    Public Property WordCount As Dictionary(Of String, Integer)

                    Public Sub New()
                        WordCount = New Dictionary(Of String, Integer)()
                    End Sub

                    Public Sub IncrementCount(word As Clause.Word)
                        If Not WordCount.ContainsKey(word.text) Then
                            WordCount(word.text) = 0
                        End If

                        WordCount(word.text) += 1
                    End Sub

                    Public Sub DecrementCount(word As Clause.Word)
                        If WordCount.ContainsKey(word.text) Then
                            WordCount(word.text) -= 1
                            If WordCount(word.text) = 0 Then
                                WordCount.Remove(word.text)
                            End If
                        End If
                    End Sub

                    Public Function GetCount(word As Clause.Word) As Integer
                        If WordCount.ContainsKey(word.text) Then
                            Return WordCount(word.text)
                        End If

                        Return 0
                    End Function
                End Class

                Private documents As List(Of Clause)
                Private numTopics As Integer
                Private topicWordCounts As Dictionary(Of Integer, WordCount)
                Private topicCounts As List(Of Integer)
                Private wordTopicAssignments As List(Of Integer)

                Public Sub New(documents As List(Of Clause), numTopics As Integer)
                    Me.documents = documents
                    Me.numTopics = numTopics
                    topicWordCounts = New Dictionary(Of Integer, WordCount)()
                    topicCounts = New List(Of Integer)()
                    wordTopicAssignments = New List(Of Integer)()
                End Sub

                Public Sub TrainModel(numIterations As Integer)
                    InitializeModel()

                    For i As Integer = 0 To numIterations - 1
                        For j As Integer = 0 To documents.Count - 1
                            SampleTopicsForDocument(documents(j))
                        Next
                    Next
                End Sub

                Private Sub InitializeModel()
                    Dim wordCount As Integer = 0

                    For Each document In documents
                        For Each word In document.Words
                            Dim topic = CInt(Math.Floor(Rnd() * numTopics))
                            wordTopicAssignments.Add(topic)
                            wordCount += 1

                            If Not topicWordCounts.ContainsKey(topic) Then
                                topicWordCounts(topic) = New WordCount()
                            End If

                            topicWordCounts(topic).IncrementCount(word)
                            topicCounts.Add(topic)
                        Next
                    Next

                    Console.WriteLine("Number of words: " & wordCount)
                End Sub

                Private Sub SampleTopicsForDocument(document As Clause)
                    For Each word In document.Words
                        Dim oldTopic = wordTopicAssignments(word.Position)
                        topicWordCounts(oldTopic).DecrementCount(word)

                        Dim topicDistribution = CalculateTopicDistribution(document, word)
                        Dim newTopic = SampleFromDistribution(topicDistribution)

                        topicWordCounts(newTopic).IncrementCount(word)
                        wordTopicAssignments(word.Position) = newTopic
                    Next
                End Sub

                Private Function CalculateTopicDistribution(document As Clause, word As Clause.Word) As Double()
                    Dim distribution(numTopics - 1) As Double

                    For i As Integer = 0 To numTopics - 1
                        distribution(i) = CalculateTopicProbability(i, document, word)
                    Next

                    Return distribution
                End Function

                Private Function CalculateTopicProbability(topic As Integer, document As Clause, word As Clause.Word) As Double
                    Dim wordCountInTopic = topicWordCounts(topic).GetCount(word)

                    Dim totalWordCountInTopic As Integer = 0

                    For Each assignment In wordTopicAssignments
                        If assignment = topic Then
                            totalWordCountInTopic += 1
                        End If
                    Next

                    Return (wordCountInTopic + 1) / (totalWordCountInTopic + topicCounts.Count)
                End Function

                Private Function SampleFromDistribution(distribution As Double()) As Integer
                    Dim rnd = New Random()

                    For i As Integer = 1 To distribution.Length - 1
                        distribution(i) += distribution(i - 1)
                    Next

                    Dim randomValue = rnd.NextDouble() * distribution(distribution.Length - 1)
                    Dim sample As Integer

                    For i As Integer = 0 To distribution.Length - 1
                        If randomValue < distribution(i) Then
                            sample = i
                            Exit For
                        End If
                    Next

                    Return sample
                End Function

                Public Sub PrintTopics()
                    For Each topic In topicWordCounts.Keys
                        Console.WriteLine("Topic " & topic)
                        Dim topicWordCount = topicWordCounts(topic).WordCount
                        Dim totalWordCount As Integer = 0

                        For Each assignment In wordTopicAssignments
                            If assignment = topic Then
                                totalWordCount += 1
                            End If
                        Next
                        For Each word In topicWordCount.Keys
                            Dim count = topicWordCount(word)
                            Dim probability = count / totalWordCount
                            Console.WriteLine("   " & word & ": " & probability)
                        Next

                        Console.WriteLine()
                    Next
                End Sub
            End Class

            ''' <summary>
            ''' Outputs Structure to Jason(JavaScriptSerializer)
            ''' </summary>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Public Function ToJson(ByRef iObject As Object) As String
                Dim Converter As New JavaScriptSerializer
                Return Converter.Serialize(iObject)
            End Function
        End Module

        Public Class LanguageModelFactory
            Public Class LangModelGenerator
                Inherits NgramLanguageModel

                Public Sub New(n As Integer)
                    MyBase.New(n)
                End Sub

                Public Function GenerateTextHigherOrder(numWords As Integer) As String
                    Dim generatedText As List(Of String) = New List(Of String)()

                    ' Generate text using higher order n-grams
                    Dim startingNgram = GetRandomNgram()
                    generatedText.AddRange(startingNgram.Split(" "c))

                    For i As Integer = 0 To numWords - ngramSize
                        Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                        Dim nextWord = GenerateNextWord(nGramPrefix)
                        generatedText.Add(nextWord)
                    Next

                    Return String.Join(" ", generatedText)
                End Function

                Public Function GenerateTextLongTermDependencyModel(numWords As Integer) As String
                    Dim generatedText As List(Of String) = New List(Of String)()

                    ' Generate text considering long-term dependencies
                    Dim startingNgram = GetRandomNgram()
                    generatedText.AddRange(startingNgram.Split(" "c))

                    For i As Integer = 0 To numWords - ngramSize
                        Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                        Dim nextWord = GenerateNextWord(nGramPrefix)
                        generatedText.Add(nextWord)

                        ' Update counts for longer n-grams
                        For j As Integer = ngramSize To 1 Step -1
                            Dim longerNgramPrefix = String.Join(" ", generatedText.Skip(i).Take(j - 1))
                            If ngramModel.ContainsKey(longerNgramPrefix) AndAlso ngramModel(longerNgramPrefix).ContainsKey(nextWord) Then
                                ngramModel(longerNgramPrefix)(nextWord) += 1
                            End If
                        Next
                    Next

                    Return String.Join(" ", generatedText)
                End Function

                Public Function GenerateUniqueSentence() As String
                    Dim sentence As New StringBuilder()
                    Dim currentNgram As String = GetRandomNgram()

                    ' Generate the first word of the sentence
                    Dim words As String() = currentNgram.Split()
                    sentence.Append(words(words.Length - 1))

                    ' Generate subsequent words until reaching an end token or a predefined length
                    Dim nextWord As String = ""
                    Dim maxLength As Integer = 20 ' Maximum length of the generated sentence

                    While Not String.IsNullOrEmpty(nextWord) AndAlso sentence.Length < maxLength
                        sentence.Append(" ")
                        sentence.Append(nextWord)
                        currentNgram = sentence.ToString(sentence.Length - ngramSize - 1, ngramSize)
                        nextWord = PredictNextWord(currentNgram)
                    End While

                    Return sentence.ToString()
                End Function

                Public Function GenerateUniqueSentence(ByRef currentNgram As String) As String
                    Dim sentence As New StringBuilder()

                    ' Generate the first word of the sentence
                    Dim words As String() = currentNgram.Split()
                    sentence.Append(words(words.Length - 1))

                    ' Generate subsequent words until reaching an end token or a predefined length
                    Dim nextWord As String = ""
                    Dim maxLength As Integer = 20 ' Maximum length of the generated sentence

                    While Not String.IsNullOrEmpty(nextWord) AndAlso sentence.Length < maxLength
                        sentence.Append(" ")
                        sentence.Append(nextWord)
                        currentNgram = sentence.ToString(sentence.Length - ngramSize - 1, ngramSize)
                        nextWord = PredictNextWord(currentNgram)
                    End While

                    Return sentence.ToString()
                End Function


                Public Shared Function GenerateCodePredictor(ByRef TrainingSamples As List(Of String)) As NgramLanguageModel
                    Dim Model As New NgramLanguageModel(25)
                    Model.Train(TrainingSamples)
                    Return Model
                End Function

                Public Shared Function GeneratePoemGenerator(ByRef TrainingSamples As List(Of String)) As NgramLanguageModel
                    Dim Model As New NgramLanguageModel(40)
                    Model.Train(TrainingSamples)
                    Return Model
                End Function

            End Class

            Public Shared Function CreateNgramLanguageModel(n As Integer) As NgramLanguageModel
                Select Case n
                    Case 1
                        Return New UnigramModel()
                    Case 2
                        Return New BigramLanguageModel()
                    Case 3
                        Return New TrigramLanguageModel()
                        ' Add more cases for other n-gram sizes as needed
                    Case Else
                        Throw New ArgumentException("Invalid n-gram size: " & n)
                End Select
            End Function
            Public Class NgramModels
                Public Class BaseModels
                    Public Class BigramLanguageModel
                        Inherits NgramLanguageModel
                        Private bigramModel As Dictionary(Of String, Dictionary(Of String, Integer))
                        Private contextCounts As New Dictionary(Of String, Integer)
                        Private ngramCounts As New Dictionary(Of String, Integer)()
                        Private ngramProbs As New Dictionary(Of String, Double)()
                        Private random As New Random()

                        Public Sub New()
                            MyBase.New(2)
                            bigramModel = New Dictionary(Of String, Dictionary(Of String, Integer))()
                        End Sub

                        Public Shared Function CreateBigrams(ByVal text As String, ByVal startChar As Char, ByVal stopChar As Char) As List(Of String)
                            Dim bigrams As New List(Of String)()

                            ' Add start character to the text
                            Dim modifiedText As String = startChar & text

                            ' Add stop character to the text
                            modifiedText &= stopChar

                            ' Iterate through the text to create bigrams
                            For i As Integer = 0 To modifiedText.Length - 2
                                Dim bigram As String = modifiedText.Substring(i, 2)
                                bigrams.Add(bigram)
                            Next

                            Return bigrams
                        End Function

                        Public Shared Function CreatePositionalEncoding(ByVal text As String, ByVal startChar As Char, ByVal stopChar As Char) As List(Of KeyValuePair(Of String, Integer))
                            Dim positionalEncoding As New List(Of KeyValuePair(Of String, Integer))()

                            Dim bigrams As List(Of String) = CreateBigrams(text, startChar, stopChar)

                            ' Assign positions to bigrams
                            For i As Integer = 0 To bigrams.Count - 1
                                Dim bigram As String = bigrams(i)
                                Dim position As Integer = i + 1 ' Position starts from 1

                                Dim encodedBigram As New KeyValuePair(Of String, Integer)(bigram, position)
                                positionalEncoding.Add(encodedBigram)
                            Next

                            Return positionalEncoding
                        End Function

                        Public Shared Function CreateSequenceEncoding(ByVal text As String, ByVal startChar As Char, ByVal stopChar As Char) As List(Of KeyValuePair(Of String, String))
                            Dim sequenceEncoding As New List(Of KeyValuePair(Of String, String))()

                            Dim bigrams As List(Of String) = CreateBigrams(text, startChar, stopChar)

                            ' Generate sequence encodings for bigrams
                            Dim encodingBuilder As New StringBuilder()
                            For Each bigram As String In bigrams
                                encodingBuilder.Append(bigram)
                                Dim encoding As String = encodingBuilder.ToString()
                                Dim encodedBigram As New KeyValuePair(Of String, String)(bigram, encoding)
                                sequenceEncoding.Add(encodedBigram)
                            Next

                            Return sequenceEncoding
                        End Function

                        Public Shared Function CreateUniqueBigrams(ByVal text As String, ByVal startChar As Char, ByVal stopChar As Char) As Dictionary(Of String, Integer)
                            Dim vocabulary As New Dictionary(Of String, Integer)()

                            Dim bigrams As List(Of String) = CreateBigrams(text, startChar, stopChar)

                            ' Calculate the frequencies of bigrams
                            For Each bigram As String In bigrams
                                If vocabulary.ContainsKey(bigram) Then
                                    vocabulary(bigram) += 1
                                Else
                                    vocabulary.Add(bigram, 1)
                                End If
                            Next

                            Return vocabulary
                        End Function

                        Public Shared Sub DisplayExampleII_Results()
                            Dim text As String = "This is a sample text."
                            Dim startChar As Char = "#"
                            Dim stopChar As Char = "$"

                            ' Example usage of CreateBigrams function
                            Dim bigrams As List(Of String) = CreateBigrams(text, startChar, stopChar)
                            Console.WriteLine("Bigrams:")
                            For Each bigram As String In bigrams
                                Console.WriteLine(bigram)
                            Next

                            Console.WriteLine()

                            ' Example usage of CreateUniqueBigrams function
                            Dim vocabulary As Dictionary(Of String, Integer) = CreateUniqueBigrams(text, startChar, stopChar)
                            Console.WriteLine("Vocabulary:")
                            For Each pair As KeyValuePair(Of String, Integer) In vocabulary
                                Console.WriteLine(pair.Key & ": " & pair.Value)
                            Next

                            Console.WriteLine()

                            ' Example usage of CreatePositionalEncoding function
                            Dim positionalEncoding As List(Of KeyValuePair(Of String, Integer)) = CreatePositionalEncoding(text, startChar, stopChar)
                            Console.WriteLine("Positional Encoding:")
                            For Each pair As KeyValuePair(Of String, Integer) In positionalEncoding
                                Console.WriteLine(pair.Key & ": " & pair.Value)
                            Next

                            Console.WriteLine()

                            ' Example usage of CreateSequenceEncoding function
                            Dim sequenceEncoding As List(Of KeyValuePair(Of String, String)) = CreateSequenceEncoding(text, startChar, stopChar)
                            Console.WriteLine("Sequence Encoding:")
                            For Each pair As KeyValuePair(Of String, String) In sequenceEncoding
                                Console.WriteLine(pair.Key & ": " & pair.Value)
                            Next

                            Console.ReadLine()
                        End Sub

                        Public Shared Sub DisplayExampleResults()
                            Dim model As New BigramLanguageModel()
                            Dim corpus As String = "The cat sat on the mat"

                            model.CreateModel(corpus)

                            Dim currentWord As String = "the"
                            Dim predictedWord As String = model.PredictNextWord(currentWord)

                            Console.WriteLine("Predicted next word for '" & currentWord & "': " & predictedWord)
                        End Sub

                        Public Sub CreateBigramModel(corpus As String)
                            Dim words As String() = corpus.Split()

                            For i As Integer = 0 To words.Length - 2
                                Dim currentWord As String = words(i)
                                Dim nextWord As String = words(i + 1)

                                If Not bigramModel.ContainsKey(currentWord) Then
                                    bigramModel(currentWord) = New Dictionary(Of String, Integer)()
                                End If

                                If Not bigramModel(currentWord).ContainsKey(nextWord) Then
                                    bigramModel(currentWord)(nextWord) = 0
                                End If

                                bigramModel(currentWord)(nextWord) += 1
                            Next
                        End Sub

                        Public Function GenerateSentence() As String
                            Dim sentence As String = ""
                            Dim prevToken As String = "<s>"

                            While Not prevToken.Equals("</s>")
                                Dim nextToken As String = GenerateNextToken(prevToken)
                                sentence += nextToken & " "
                                prevToken = nextToken
                            End While

                            Return sentence.Trim()
                        End Function

                        Public Function GenerateSentenceII() As String
                            Dim sb As New StringBuilder()

                            Dim sentenceLength = 10
                            Dim currentContext = "<s>"
                            For i = 0 To sentenceLength - 1
                                Dim word = GenerateNextWord(currentContext)
                                sb.Append(word & " ")
                                currentContext = word
                            Next

                            Return sb.ToString().Trim()
                        End Function

                        Public Function GetBigramProbability(currentWord As String, nextWord As String) As Double
                            If bigramModel.ContainsKey(currentWord) AndAlso bigramModel(currentWord).ContainsKey(nextWord) Then
                                Dim totalCount As Integer = bigramModel(currentWord).Values.Sum()
                                Dim bigramCount As Integer = bigramModel(currentWord)(nextWord)
                                Return CDbl(bigramCount) / totalCount
                            End If

                            Return 0.0
                        End Function

                        Public Function PredictNBigramextWord(currentWord As String) As String
                            Dim nextWordCandidates As New List(Of String)()

                            If bigramModel.ContainsKey(currentWord) Then
                                nextWordCandidates.AddRange(bigramModel(currentWord).Keys)
                            End If

                            If nextWordCandidates.Count > 0 Then
                                Dim maxProbability As Double = 0.0
                                Dim predictedWord As String = ""

                                For Each word As String In nextWordCandidates
                                    Dim probability As Double = GetProbability(currentWord, word)

                                    If probability > maxProbability Then
                                        maxProbability = probability
                                        predictedWord = word
                                    End If
                                Next

                                Return predictedWord
                            End If

                            Return "No prediction available"
                        End Function

                        Public Sub Train(ByVal sentences As List(Of String))
                            For Each sentence As String In sentences
                                Dim tokens As String() = TokenizeSentence(sentence)
                                Dim ngrams As List(Of String) = GenerateNgrams(tokens)

                                For Each ngram As String In ngrams
                                    If ngramCounts.ContainsKey(ngram) Then
                                        ngramCounts(ngram) += 1
                                    Else
                                        ngramCounts.Add(ngram, 1)
                                    End If
                                Next
                            Next

                            Dim totalNgrams As Integer = ngramCounts.Values.Sum()

                            For Each ngramCount As KeyValuePair(Of String, Integer) In ngramCounts
                                Dim ngram As String = ngramCount.Key
                                Dim count As Integer = ngramCount.Value

                                Dim prob As Double = count / totalNgrams
                                ngramProbs.Add(ngram, prob)
                            Next
                        End Sub

                        Public Function TrainII(ByVal corpus As List(Of String)) As Double
                            ' Collect bigram counts from the corpus
                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For i = 0 To words.Count - 2
                                    Dim bigram = words(i) & " " & words(i + 1)
                                    If ngramCounts.ContainsKey(bigram) Then
                                        ngramCounts(bigram) += 1
                                    Else
                                        ngramCounts.Add(bigram, 1)
                                    End If

                                    Dim context = words(i)
                                    If contextCounts.ContainsKey(context) Then
                                        contextCounts(context) += 1
                                    Else
                                        contextCounts.Add(context, 1)
                                    End If
                                Next
                            Next

                            ' Normalize counts to probabilities
                            For Each ngram In ngramCounts.Keys.ToList()
                                Dim context = ngram.Split(" ")(0)
                                ngramCounts(ngram) /= contextCounts(context)
                            Next

                            ' Return perplexity score
                            Return CalculatePerplexity(corpus)
                        End Function

                        Private Function CalculatePerplexity(ByVal corpus As List(Of String)) As Double
                            Dim totalLogProb As Double = 0.0
                            Dim totalWords As Integer = 0

                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For i = 0 To words.Count - 2
                                    Dim bigram = words(i) & " " & words(i + 1)
                                    totalLogProb += Math.Log(ngramCounts(bigram))
                                    totalWords += 1
                                Next
                            Next

                            Dim perplexity = Math.Exp(-(totalLogProb / totalWords))
                            Return perplexity
                        End Function

                        Private Function GenerateNextToken(ByVal prevToken As String) As String
                            Dim candidates As New List(Of String)()
                            Dim probabilities As New List(Of Double)()

                            For Each ngramProb As KeyValuePair(Of String, Double) In ngramProbs
                                Dim ngram As String = ngramProb.Key
                                Dim prob As Double = ngramProb.Value

                                If ngram.StartsWith(prevToken) Then
                                    Dim nextToken As String = ngram.Split(" "c)(1)
                                    candidates.Add(nextToken)
                                    probabilities.Add(prob)
                                End If
                            Next

                            Dim selectedIndex As Integer = RandomSelect(probabilities)
                            Return candidates(selectedIndex)
                        End Function

                        Private Function GenerateNextWord(ByVal context As String) As String
                            Dim candidateWords As New List(Of String)()

                            For Each ngram In ngramCounts.Keys
                                If ngram.StartsWith(context & " ") Then
                                    candidateWords.Add(ngram.Split(" ")(1))
                                End If
                            Next

                            Dim rand As New Random()
                            Dim randomIndex = rand.Next(0, candidateWords.Count)
                            Return candidateWords(randomIndex)
                        End Function

                        Private Function GenerateNgrams(ByVal tokens As String()) As List(Of String)
                            Dim ngrams As New List(Of String)()

                            For i As Integer = 1 To tokens.Length - 1
                                Dim ngram As String = tokens(i - 1) & " " & tokens(i)
                                ngrams.Add(ngram)
                            Next

                            Return ngrams
                        End Function

                        Private Function RandomSelect(ByVal probabilities As List(Of Double)) As Integer
                            Dim total As Double = probabilities.Sum()
                            Dim threshold As Double = random.NextDouble() * total

                            Dim cumulativeProb As Double = 0
                            For i As Integer = 0 To probabilities.Count - 1
                                cumulativeProb += probabilities(i)
                                If cumulativeProb >= threshold Then
                                    Return i
                                End If
                            Next

                            Return probabilities.Count - 1
                        End Function

                        Private Function Tokenize(ByVal sentence As String) As List(Of String)
                            Dim pattern As String = "\b\w+\b"
                            Dim regex As New Regex(pattern)
                            Dim matches = regex.Matches(sentence)

                            Dim words As New List(Of String)()
                            For Each match As Match In matches
                                words.Add(match.Value)
                            Next

                            Return words
                        End Function

                        Private Function TokenizeSentence(ByVal sentence As String) As String()
                            Return Split(sentence, " "c, StringSplitOptions.RemoveEmptyEntries)
                        End Function

                    End Class
                    Public Class TrigramLanguageModel
                        Inherits NgramLanguageModel

                        Private contextCounts As New Dictionary(Of String, Integer)

                        Private ngramCounts As New Dictionary(Of String, Integer)()

                        Private ngramProbs As New Dictionary(Of String, Double)()

                        Private random As New Random()

                        Public Sub New()
                            MyBase.New(3)
                        End Sub

                        Public Function GenerateSentence() As String
                            Dim sb As New StringBuilder()

                            Dim sentenceLength = 10
                            Dim currentContext = "<s> <s>"
                            For i = 0 To sentenceLength - 1
                                Dim word = GenerateNextWord(currentContext)
                                sb.Append(word & " ")
                                currentContext = currentContext.Split(" ")(1) & " " & word
                            Next

                            Return sb.ToString().Trim()
                        End Function

                        Public Function GenerateSentenceII() As String
                            Dim sentence As String = ""
                            Dim prevTokens As String() = {"<s>", "<s>"}

                            While Not prevTokens(1).Equals("</s>")
                                Dim nextToken As String = GenerateNextToken(prevTokens)
                                sentence += nextToken & " "
                                prevTokens(0) = prevTokens(1)
                                prevTokens(1) = nextToken
                            End While

                            Return sentence.Trim()
                        End Function

                        Public Function Train(ByVal corpus As List(Of String)) As Double
                            ' Collect trigram counts from the corpus
                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For i = 0 To words.Count - 3
                                    Dim trigram = words(i) & " " & words(i + 1) & " " & words(i + 2)
                                    If ngramCounts.ContainsKey(trigram) Then
                                        ngramCounts(trigram) += 1
                                    Else
                                        ngramCounts.Add(trigram, 1)
                                    End If

                                    Dim context = words(i) & " " & words(i + 1)
                                    If contextCounts.ContainsKey(context) Then
                                        contextCounts(context) += 1
                                    Else
                                        contextCounts.Add(context, 1)
                                    End If
                                Next
                            Next

                            ' Normalize counts to probabilities
                            For Each ngram In ngramCounts.Keys.ToList()
                                Dim context = ngram.Split(" ")(0) & " " & ngram.Split(" ")(1)
                                ngramCounts(ngram) /= contextCounts(context)
                            Next

                            ' Return perplexity score
                            Return CalculatePerplexity(corpus)
                        End Function

                        Public Sub TrainII(ByVal sentences As List(Of String))
                            For Each sentence As String In sentences
                                Dim tokens As String() = TokenizeSentence(sentence)
                                Dim ngrams As List(Of String) = GenerateNgrams(tokens)

                                For Each ngram As String In ngrams
                                    If ngramCounts.ContainsKey(ngram) Then
                                        ngramCounts(ngram) += 1
                                    Else
                                        ngramCounts.Add(ngram, 1)
                                    End If
                                Next
                            Next

                            Dim totalNgrams As Integer = ngramCounts.Values.Sum()

                            For Each ngramCount As KeyValuePair(Of String, Integer) In ngramCounts
                                Dim ngram As String = ngramCount.Key
                                Dim count As Integer = ngramCount.Value

                                Dim prob As Double = count / totalNgrams
                                ngramProbs.Add(ngram, prob)
                            Next
                        End Sub

                        Private Function CalculatePerplexity(ByVal corpus As List(Of String)) As Double
                            Dim totalLogProb As Double = 0.0
                            Dim totalWords As Integer = 0

                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For i = 0 To words.Count - 3
                                    Dim trigram = words(i) & " " & words(i + 1) & " " & words(i + 2)
                                    totalLogProb += Math.Log(ngramCounts(trigram))
                                    totalWords += 1
                                Next
                            Next

                            Dim perplexity = Math.Exp(-(totalLogProb / totalWords))
                            Return perplexity
                        End Function

                        Private Function GenerateNextToken(ByVal prevTokens As String()) As String
                            Dim candidates As New List(Of String)()
                            Dim probabilities As New List(Of Double)()

                            For Each ngramProb As KeyValuePair(Of String, Double) In ngramProbs
                                Dim ngram As String = ngramProb.Key
                                Dim prob As Double = ngramProb.Value

                                If ngram.StartsWith(prevTokens(0) & " " & prevTokens(1)) Then
                                    Dim nextToken As String = ngram.Split(" "c)(2)
                                    candidates.Add(nextToken)
                                    probabilities.Add(prob)
                                End If
                            Next

                            Dim selectedIndex As Integer = RandomSelect(probabilities)
                            Return candidates(selectedIndex)
                        End Function

                        Private Function GenerateNextWord(ByVal context As String) As String
                            Dim candidateWords As New List(Of String)()

                            For Each ngram In ngramCounts.Keys
                                If ngram.StartsWith(context & " ") Then
                                    candidateWords.Add(ngram.Split(" ")(2))
                                End If
                            Next

                            Dim rand As New Random()
                            Dim randomIndex = rand.Next(0, candidateWords.Count)
                            Return candidateWords(randomIndex)
                        End Function

                        Private Function GenerateNgrams(ByVal tokens As String()) As List(Of String)
                            Dim ngrams As New List(Of String)()

                            For i As Integer = 2 To tokens.Length - 1
                                Dim ngram As String = tokens(i - 2) & " " & tokens(i - 1) & " " & tokens(i)
                                ngrams.Add(ngram)
                            Next

                            Return ngrams
                        End Function

                        Private Function RandomSelect(ByVal probabilities As List(Of Double)) As Integer
                            Dim total As Double = probabilities.Sum()
                            Dim threshold As Double = random.NextDouble() * total

                            Dim cumulativeProb As Double = 0
                            For i As Integer = 0 To probabilities.Count - 1
                                cumulativeProb += probabilities(i)
                                If cumulativeProb >= threshold Then
                                    Return i
                                End If
                            Next

                            Return probabilities.Count - 1
                        End Function

                        Private Function Tokenize(ByVal sentence As String) As List(Of String)
                            Dim pattern As String = "\b\w+\b"
                            Dim regex As New Regex(pattern)
                            Dim matches = regex.Matches(sentence)

                            Dim words As New List(Of String)()
                            For Each match As Match In matches
                                words.Add(match.Value)
                            Next

                            Return words
                        End Function

                        Private Function TokenizeSentence(ByVal sentence As String) As String()
                            Return Split(sentence, StringSplitOptions.RemoveEmptyEntries)
                        End Function

                    End Class
                    Public Class UnigramModel

                        Inherits NgramLanguageModel
                        Private ngramCounts As New Dictionary(Of String, Integer)()
                        Private ngramProbs As New Dictionary(Of String, Double)()
                        Private random As New Random()

                        Public Sub New()
                            MyBase.New(1)
                        End Sub

                        Public Function GenerateSentence() As String
                            Dim sentence As String = ""
                            Dim prevToken As String = "<s>"

                            While Not prevToken.Equals("</s>")
                                Dim nextToken As String = GenerateNextToken(prevToken)
                                sentence += nextToken & " "
                                prevToken = nextToken
                            End While

                            Return sentence.Trim()
                        End Function

                        Public Function GenerateSentenceII() As String
                            Dim sb As New StringBuilder()

                            Dim sentenceLength = 10
                            For i = 0 To sentenceLength - 1
                                Dim word = GenerateNextWord()
                                sb.Append(word & " ")
                            Next

                            Return sb.ToString().Trim()
                        End Function

                        Public Function Train(ByVal corpus As List(Of String)) As Double
                            ' Collect unigram counts from the corpus
                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For Each word In words
                                    If ngramCounts.ContainsKey(word) Then
                                        ngramCounts(word) += 1
                                    Else
                                        ngramCounts.Add(word, 1)
                                    End If
                                Next
                            Next

                            ' Normalize counts to probabilities
                            Dim totalNgrams = ngramCounts.Values.Sum()
                            For Each ngram In ngramCounts.Keys.ToList()
                                ngramCounts(ngram) /= totalNgrams
                            Next

                            ' Return perplexity score
                            Return CalculatePerplexity(corpus)
                        End Function

                        Public Sub TrainII(ByVal sentences As List(Of String))
                            For Each sentence As String In sentences
                                Dim tokens As String() = TokenizeSentence(sentence)

                                For Each token As String In tokens
                                    If ngramCounts.ContainsKey(token) Then
                                        ngramCounts(token) += 1
                                    Else
                                        ngramCounts.Add(token, 1)
                                    End If
                                Next
                            Next

                            Dim totalTokens As Integer = ngramCounts.Values.Sum()

                            For Each ngramCount As KeyValuePair(Of String, Integer) In ngramCounts
                                Dim token As String = ngramCount.Key
                                Dim count As Integer = ngramCount.Value

                                Dim prob As Double = count / totalTokens
                                ngramProbs.Add(token, prob)
                            Next
                        End Sub

                        Private Function CalculatePerplexity(ByVal corpus As List(Of String)) As Double
                            Dim totalLogProb As Double = 0.0
                            Dim totalWords As Integer = 0

                            For Each sentence In corpus
                                Dim words = Tokenize(sentence)
                                For Each word In words
                                    totalLogProb += Math.Log(ngramCounts(word))
                                    totalWords += 1
                                Next
                            Next

                            Dim perplexity = Math.Exp(-(totalLogProb / totalWords))
                            Return perplexity
                        End Function

                        Private Function GenerateNextToken(ByVal prevToken As String) As String
                            Dim candidates As New List(Of String)()
                            Dim probabilities As New List(Of Double)()

                            For Each ngramProb As KeyValuePair(Of String, Double) In ngramProbs
                                Dim token As String = ngramProb.Key
                                Dim prob As Double = ngramProb.Value

                                candidates.Add(token)
                                probabilities.Add(prob)
                            Next

                            Dim selectedIndex As Integer = RandomSelect(probabilities)
                            Return candidates(selectedIndex)
                        End Function

                        Private Function GenerateNextWord() As String
                            Dim rand As New Random()
                            Dim randomIndex = rand.Next(0, ngramCounts.Keys.Count)
                            Return ngramCounts.Keys(randomIndex)
                        End Function

                        Private Function RandomSelect(ByVal probabilities As List(Of Double)) As Integer
                            Dim total As Double = probabilities.Sum()
                            Dim threshold As Double = random.NextDouble() * total

                            Dim cumulativeProb As Double = 0
                            For i As Integer = 0 To probabilities.Count - 1
                                cumulativeProb += probabilities(i)
                                If cumulativeProb >= threshold Then
                                    Return i
                                End If
                            Next

                            Return probabilities.Count - 1
                        End Function

                        Private Function Tokenize(ByVal sentence As String) As List(Of String)
                            Dim pattern As String = "\b\w+\b"
                            Dim regex As New Regex(pattern)
                            Dim matches = regex.Matches(sentence)

                            Dim words As New List(Of String)()
                            For Each match As Match In matches
                                words.Add(match.Value)
                            Next

                            Return words
                        End Function

                        Private Function TokenizeSentence(ByVal sentence As String) As String()
                            Return Split(sentence, " "c, StringSplitOptions.RemoveEmptyEntries)
                        End Function

                    End Class
                    '2. Add Language Modeling Functionality:
                    Public Class LanguageModel

                        Public iTrainedModel As NgramFunctions.NgramTrainer

                        Public Sub New(n As Integer, ByRef Corpus As List(Of String))

                            iTrainedModel = New NgramFunctions.NgramTrainer(New NgramLanguageModel(n))
                            iTrainedModel.Train(Corpus)
                        End Sub

                        Public Sub New(n As Integer)

                            iTrainedModel = New NgramFunctions.NgramTrainer(New NgramLanguageModel(n))
                        End Sub

                        Public Sub New()

                            iTrainedModel = New NgramFunctions.NgramTrainer(New NgramLanguageModel(2))
                        End Sub

                        Public Sub New(ByRef Corpus As List(Of String))

                            iTrainedModel = New NgramFunctions.NgramTrainer(New NgramLanguageModel(2))
                            iTrainedModel = iTrainedModel.TrainModel(Corpus)
                        End Sub

                        Public ReadOnly Property TrainedModel As NgramFunctions.NgramTrainer
                            Get
                                Return iTrainedModel
                            End Get
                        End Property



                    End Class
                End Class
                Public Class TestModels
                    Public Class CodeGenerator
                        Inherits NgramGenerator

                        Public Sub New(ByRef model As NgramLanguageModel)
                            MyBase.New(model)
                        End Sub

                        Public Structure CodePrediction
                            Public Code As String
                            Public Probability As Double
                        End Structure
                        Public Function PredictNextCode(tokens As List(Of String)) As String
                            Dim context As String = GetContext(tokens)
                            Dim nextToken As String = GetNextToken(context)

                            Return nextToken
                        End Function
                        Public Function PredictNextToken(Query As String) As String
                            ' Tokens representing the code context
                            Dim tokens As List(Of String) = getTokens(Query)

                            ' Predict the next code token
                            Dim predictedToken As String = Me.PredictNextCode(tokens)

                            ' Display the predicted token
                            Console.WriteLine("Predicted Next Token: " & predictedToken)
                            Return predictedToken
                        End Function
                        Public Function PredictNextCodeSegment(Code As List(Of String), ByRef numPredictions As Integer) As List(Of CodePrediction)
                            ' Predict the next code segment
                            ' Generate code predictions
                            Dim maxLength As Integer = 10
                            Dim predictions As New List(Of CodePrediction)
                            Dim seedPhrase As String = Code(Code.Count - 2) & " " & Code(Code.Count - 1)
                            For i As Integer = 1 To numPredictions
                                Dim prediction As String = GenerateText(maxLength, seedPhrase)
                                Dim probability As Double = CalculateProbability(Me, prediction)
                                Console.WriteLine("Predicted Next Code: ")
                                Console.WriteLine("Prediction {0}:", i)
                                Console.WriteLine(prediction)
                                Console.WriteLine("Probability: {0}", probability)
                                Console.WriteLine()
                                Dim newPrediciton As New CodePrediction
                                newPrediciton.Code = i
                                newPrediciton.Probability = probability
                                predictions.Add(newPrediciton)
                            Next
                            Return predictions
                        End Function
                        Public Shared Function TrainModelCodePredictor(data As List(Of String), ngramLength As Integer) As NgramLanguageModel
                            Dim model As NgramLanguageModel = New NgramLanguageModel(ngramLength)

                            For Each line As String In data
                                Dim tokens As String() = line.Split(" "c)
                                Dim sequence As List(Of String) = New List(Of String)()

                                For Each token As String In tokens
                                    sequence.Add(token)

                                    If sequence.Count = ngramLength + 1 Then
                                        Dim context As String = GetContextfrom(sequence.ToArray(), ngramLength - 1)
                                        Dim nextToken As String = sequence(ngramLength)

                                        If model.ngramModel.ContainsKey(context) Then
                                            Dim ngramCounts As Dictionary(Of String, Integer) = model.ngramModel(context)

                                            If ngramCounts.ContainsKey(nextToken) Then
                                                ngramCounts(nextToken) += 1
                                            Else
                                                ngramCounts.Add(nextToken, 1)
                                            End If
                                        Else
                                            Dim ngramCounts As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()
                                            ngramCounts.Add(nextToken, 1)
                                            model.ngramModel.Add(context, ngramCounts)
                                        End If

                                        sequence.RemoveAt(0)
                                    End If
                                Next
                            Next

                            Return model
                        End Function


                    End Class
                    Public Class AttentionNgramLanguageModel


                        Inherits NgramLanguageModel

                        Private attentionWeights As Dictionary(Of String, Double())

                        Public Sub New(n As Integer)
                            MyBase.New(n)
                        End Sub

                        Public Overrides Sub Train(sentences As List(Of String))
                            MyBase.Train(sentences)
                            CalculateAttentionWeights(sentences.ToString)
                        End Sub

                        Private Function ApplyAttention(tokens As String(), attentionValues As Double()) As String
                            Dim attentiveText As String = ""

                            For i As Integer = 0 To tokens.Length - 1
                                Dim token As String = tokens(i)
                                If attentionValues(i) > 0.5 Then
                                    attentiveText += token.ToUpper() + " "
                                Else
                                    attentiveText += token.ToLower() + " "
                                End If
                            Next

                            Return attentiveText.Trim()
                        End Function

                        Private Sub CalculateAttentionWeights(ByRef TrainingData As String)
                            attentionWeights = New Dictionary(Of String, Double())()

                            ' Calculate attention weights based on training data
                            For Each sentence As String In TrainingData
                                Dim tokens As String() = sentence.Split(" "c)
                                For Each token As String In tokens
                                    If Not attentionWeights.ContainsKey(token) Then
                                        attentionWeights(token) = New Double(tokens.Length - 1) {}
                                    End If
                                    For i As Integer = 0 To tokens.Length - 1
                                        attentionWeights(token)(i) += 1
                                    Next
                                Next
                            Next

                            ' Normalize attention weights
                            For Each tokenWeights As Double() In attentionWeights.Values
                                Dim sum As Double = tokenWeights.Sum()
                                For i As Integer = 0 To tokenWeights.Length - 1
                                    tokenWeights(i) /= sum
                                Next
                            Next
                        End Sub

                        Private Function GetAttentionValues(tokens As String()) As Double()
                            Dim attentionValues As Double() = New Double(tokens.Length - 1) {}

                            For i As Integer = 0 To tokens.Length - 1
                                Dim token As String = tokens(i)
                                If attentionWeights.ContainsKey(token) Then
                                    attentionValues(i) = attentionWeights(token)(i)
                                Else
                                    attentionValues(i) = 0.0
                                End If
                            Next

                            Return attentionValues
                        End Function

                    End Class

                End Class
                Public Class NgramLanguageModel

                    Public ngramEncodings As Dictionary(Of String, Integer)

                    Public ngramModel As Dictionary(Of String, Dictionary(Of String, Integer))

                    Public ngramSize As Integer

                    Private ReadOnly rand As Random

                    Public Sub New(n As Integer)
                        ngramModel = New Dictionary(Of String, Dictionary(Of String, Integer))()
                        ngramSize = n
                        ngramEncodings = New Dictionary(Of String, Integer)()
                        rand = New Random()

                    End Sub

                    Public ReadOnly Property NgramOrder As Integer
                        Get
                            Return ngramSize - 1
                        End Get
                    End Property

                    Public Shared Function CalculateProbability(ngramModel As NgramLanguageModel, prediction As String) As Double
                        Dim tokens As String() = prediction.Split(" "c)
                        Dim probability As Double = 1.0


                        For i As Integer = 0 To tokens.Length - 2
                            Dim context As String = ngramModel.GetContext(tokens, i)
                            Dim nextToken As String = tokens(i + 1)

                            If ngramModel.ngramModel.ContainsKey(context) Then
                                Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel.ngramModel(context)
                                Dim totalOccurrences As Integer = ngramCounts.Values.Sum()

                                If ngramCounts.ContainsKey(nextToken) Then
                                    Dim count As Integer = ngramCounts(nextToken)
                                    Dim tokenProbability As Double = count / totalOccurrences
                                    probability *= tokenProbability
                                Else
                                    probability = 0.0
                                    Exit For
                                End If
                            Else
                                probability = 0.0
                                Exit For
                            End If
                        Next

                        Return probability
                    End Function


                    Public Sub AddDocument(doc As String)
                        Dim words As String() = PreprocessText(doc)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1
                        Next
                    End Sub

                    Public Sub AddDocuments(ByRef Docs As List(Of String))
                        For Each item In Docs
                            Me.AddDocument(item)
                        Next
                    End Sub

                    Public Sub AddNgram(ngram As String)
                        ngramModel(ngram) = New Dictionary(Of String, Integer)()
                    End Sub

                    Public Sub CreateEncodedModel(corpus As String)
                        Dim words As String() = PreprocessText(corpus)
                        Dim numWords As Integer = words.Length - ngramSize
                        Dim position As Integer = 0

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1

                            If Not ngramEncodings.ContainsKey(currentNgram) Then
                                ngramEncodings(currentNgram) = position
                                position += 1
                            End If
                        Next
                    End Sub

                    Public Sub CreateModel(corpus As String)
                        Dim words As String() = PreprocessText(corpus)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1
                        Next
                    End Sub

                    Public Sub CreateModel(documents As List(Of String))
                        For Each document In documents
                            AddDocument(document)
                        Next
                    End Sub

                    Public Function EstimateProbability(nGramPrefix As String, word As String) As Double
                        If ngramModel.ContainsKey(nGramPrefix) AndAlso ngramModel(nGramPrefix).ContainsKey(word) Then
                            Dim nGramCount = ngramModel(nGramPrefix)(word)
                            Dim totalCount = ngramModel(nGramPrefix).Values.Sum()
                            Return nGramCount / totalCount
                        End If

                        Return 0.0
                    End Function

                    Public Function GenerateNextWord(nGramPrefix As String) As String
                        If ngramModel.ContainsKey(nGramPrefix) Then
                            Dim nGramCounts = ngramModel(nGramPrefix)
                            Dim totalOccurrences = nGramCounts.Values.Sum()

                            Dim randValue = rand.NextDouble()
                            Dim cumulativeProb = 0.0

                            For Each kvp In nGramCounts
                                cumulativeProb += kvp.Value / totalOccurrences
                                If cumulativeProb >= randValue Then
                                    Return kvp.Key
                                End If
                            Next
                        End If

                        Return ""
                    End Function

                    Public Overridable Function GenerateText(seedPhrase As String, length As Integer) As String
                        Dim generatedText As List(Of String) = seedPhrase.Split(" "c).ToList()

                        For i As Integer = 0 To length - ngramSize
                            Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                            Dim nextWord = GenerateNextWord(nGramPrefix)
                            generatedText.Add(nextWord)
                        Next

                        Return String.Join(" ", generatedText)
                    End Function

                    Public Overridable Function GenerateText(maxLength As Integer, seedPhrase As String) As String
                        Dim tokens As List(Of String) = New List(Of String)(seedPhrase.Split(" "c))

                        While tokens.Count < maxLength
                            Dim context As String = GetContextfrom(tokens.ToArray(), tokens.Count - 1)

                            If ngramModel.ContainsKey(context) Then
                                Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                                Dim totalOccurrences As Integer = ngramCounts.Values.Sum()
                                Dim randomNumber As Double = New Random().NextDouble()
                                Dim cumulativeProbability As Double = 0.0

                                For Each tokenCount As KeyValuePair(Of String, Integer) In ngramCounts
                                    Dim tokenProbability As Double = tokenCount.Value / totalOccurrences
                                    cumulativeProbability += tokenProbability

                                    If cumulativeProbability >= randomNumber Then
                                        tokens.Add(tokenCount.Key)
                                        Exit For
                                    End If
                                Next
                            Else
                                Exit While
                            End If
                        End While

                        Return String.Join(" ", tokens)
                    End Function


                    Public Function GetCount(ngram As String) As Integer

                        For Each item In ngramEncodings
                            If item.Key = ngram Then
                                Return ngramEncodings(ngram)
                            End If
                        Next

                        Return 0
                    End Function

                    Public Function GetEncoding(currentNgram As String) As Integer
                        Dim position As Integer = GetPosition(currentNgram)
                        Return position
                    End Function

                    Public Function GetNextToken(context As String) As String
                        Dim nextToken As String = ""

                        If ngramModel.ContainsKey(context) Then
                            Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                            nextToken = ngramCounts.OrderByDescending(Function(ngram) ngram.Value).FirstOrDefault().Key
                        End If

                        Return nextToken
                    End Function

                    Public Function GetNgrams() As String()
                        Return ngramModel.Keys.ToArray()
                    End Function

                    Public Function GetPosition(currentNgram As String) As Integer
                        If ngramEncodings.ContainsKey(currentNgram) Then
                            Return ngramEncodings(currentNgram)
                        End If

                        Return -1
                    End Function

                    Public Function GetProbability(ngram As String) As Double
                        Return GetCount(ngram) / ngramModel.Values.SelectMany(Function(dict) dict.Values).Sum()
                    End Function

                    Public Function GetProbability(currentNgram As String, nextWord As String) As Double
                        If ngramModel.ContainsKey(currentNgram) AndAlso ngramModel(currentNgram).ContainsKey(nextWord) Then
                            Dim totalCount As Integer = ngramModel(currentNgram).Values.Sum()
                            Dim ngramCount As Integer = ngramModel(currentNgram)(nextWord)
                            Return CDbl(ngramCount) / totalCount
                        End If

                        Return 0.0
                    End Function

                    Public Function GetRandomNgram() As String
                        Dim random As New Random()
                        Dim ngrams As String() = ngramModel.Keys.ToArray()
                        Dim randomIndex As Integer = random.Next(ngrams.Length)
                        Return ngrams(randomIndex)
                    End Function

                    Public Function getTokens(Query As String) As List(Of String)
                        Dim tokens As New List(Of String)
                        Dim Tok = Split(Query, " ")
                        For Each item In Tok
                            tokens.Add(item)
                        Next
                        Return tokens
                    End Function


                    Public Function LookupNgram(ngram As String) As Integer
                        If ngramModel.ContainsKey(ngram) Then
                            Return ngramModel(ngram).Values.Sum()
                        End If
                        Return 0
                    End Function


                    Public Function PredictNextWord(currentNgram As String) As String
                        If ngramModel.ContainsKey(currentNgram) Then
                            Dim nextWords As Dictionary(Of String, Integer) = ngramModel(currentNgram)
                            Return nextWords.OrderByDescending(Function(x) x.Value).FirstOrDefault().Key
                        End If

                        Return ""
                    End Function


                    Public Function PreprocessText(text As String) As String()
                        ' Preprocess the text by removing unnecessary characters and converting to lowercase
                        text = text.ToLower()
                        text = text.Replace(".", " .")
                        text = text.Replace(",", " ,")
                        text = text.Replace(";", " ;")
                        text = text.Replace(":", " :")
                        text = text.Replace("!", " !")
                        text = text.Replace("?", " ?")

                        ' Split the text into words
                        Return text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                    End Function

                    Public Sub RemoveDocument(doc As String)
                        Dim words As String() = PreprocessText(doc)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If ngramModel.ContainsKey(currentNgram) Then
                                Dim nextWords As Dictionary(Of String, Integer) = ngramModel(currentNgram)
                                If nextWords.ContainsKey(nextWord) Then
                                    nextWords(nextWord) -= 1
                                    If nextWords(nextWord) <= 0 Then
                                        nextWords.Remove(nextWord)
                                    End If
                                End If
                            End If
                        Next
                    End Sub

                    Public Sub RemoveNgram(ngram As String)
                        ngramModel.Remove(ngram)
                    End Sub

                    Public Overridable Sub Train(corpus As List(Of String))
                        For Each sentence In corpus
                            Dim words = sentence.Split(" "c)
                            For i As Integer = 0 To words.Length - ngramSize
                                Dim nGramPrefix = String.Join(" ", words, i, ngramSize - 1)
                                Dim nGramSuffix = words(i + ngramSize - 1)

                                If Not ngramModel.ContainsKey(nGramPrefix) Then
                                    ngramModel(nGramPrefix) = New Dictionary(Of String, Integer)()
                                End If

                                If Not ngramModel(nGramPrefix).ContainsKey(nGramSuffix) Then
                                    ngramModel(nGramPrefix)(nGramSuffix) = 0
                                End If

                                ngramModel(nGramPrefix)(nGramSuffix) += 1
                            Next
                        Next
                        For Each line In corpus
                            Dim tokens = line.Split()
                            For i As Integer = 0 To tokens.Length - NgramOrder
                                Dim context As String = GetContext(tokens, i)
                                Dim nextToken As String = tokens(i + NgramOrder)
                                UpdateNgramModel(context, nextToken)
                            Next
                        Next
                    End Sub


                    Public Function UpdateNgram(oldNgram As String, newNgram As String) As Boolean
                        If ngramModel.ContainsKey(oldNgram) AndAlso Not ngramModel.ContainsKey(newNgram) Then
                            ' Update ngramModel
                            ngramModel(newNgram) = ngramModel(oldNgram)
                            ngramModel.Remove(oldNgram)

                            ' Update ngramEncodings
                            If ngramEncodings.ContainsKey(oldNgram) Then
                                Dim position As Integer = ngramEncodings(oldNgram)
                                ngramEncodings.Remove(oldNgram)
                                ngramEncodings(newNgram) = position
                            End If

                            Return True
                        End If
                        Return False
                    End Function

                    Public Shared Function GetContextfrom(tokens As String(), index As Integer) As String
                        Return String.Join(" ", tokens.Take(index + 1))
                    End Function

                    Public Function GetContext(tokens As List(Of String)) As String
                        Dim contextTokens As List(Of String) = tokens.Skip(Math.Max(0, tokens.Count - NgramOrder)).ToList()
                        Return String.Join(" ", contextTokens)
                    End Function

                    Public Function GetContext(tokens As String(), index As Integer) As String
                        Dim contextTokens As New List(Of String)()
                        For i As Integer = index To index + NgramOrder - 1
                            contextTokens.Add(tokens(i))
                        Next
                        Return String.Join(" ", contextTokens)
                    End Function

                    Private Sub UpdateNgramModel(context As String, nextToken As String)
                        If Not ngramModel.ContainsKey(context) Then
                            ngramModel.Add(context, New Dictionary(Of String, Integer)())
                        End If

                        Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                        If ngramCounts.ContainsKey(nextToken) Then
                            ngramCounts(nextToken) += 1
                        Else
                            ngramCounts.Add(nextToken, 1)
                        End If
                    End Sub







                End Class

                Public Class NgramFunctions
                    Public Class NgramTrainer
                        Inherits NgramLanguageModel
                        ''' <summary>
                        ''' PlaceHolder ... this model is not to be used as a model
                        ''' </summary>
                        Public Sub New(ByRef model As NgramLanguageModel)
                            MyBase.New(0)

                            Me.ngramEncodings = model.ngramEncodings

                            Me.ngramModel = model.ngramModel

                            Me.ngramSize = model.ngramSize
                        End Sub
                        Public Sub TrainHigherOrder(corpus As List(Of String))
                            Train(corpus)

                            ' Additional training logic specific to higher order n-grams
                            For Each sentence In corpus
                                Dim words = sentence.Split(" "c)
                                For i As Integer = 0 To words.Length - ngramSize
                                    Dim nGramPrefix = String.Join(" ", words, i, ngramSize - 1)
                                    Dim nGramSuffix = words(i + ngramSize - 1)

                                    If Not ngramModel.ContainsKey(nGramPrefix) Then
                                        ngramModel(nGramPrefix) = New Dictionary(Of String, Integer)()
                                    End If

                                    If Not ngramModel(nGramPrefix).ContainsKey(nGramSuffix) Then
                                        ngramModel(nGramPrefix)(nGramSuffix) = 0
                                    End If

                                    ngramModel(nGramPrefix)(nGramSuffix) += 1
                                Next
                            Next
                        End Sub

                        Public Sub TrainLongTermDependencyModel(corpus As List(Of String))
                            Train(corpus)

                            ' Additional training logic for long-term dependency modeling
                            For Each sentence In corpus
                                Dim words = sentence.Split(" "c)
                                For i As Integer = 0 To words.Length - ngramSize
                                    Dim nGramPrefix = String.Join(" ", words, i, ngramSize - 1)
                                    Dim nGramSuffix = words(i + ngramSize - 1)

                                    If Not ngramModel.ContainsKey(nGramPrefix) Then
                                        ngramModel(nGramPrefix) = New Dictionary(Of String, Integer)()
                                    End If

                                    If Not ngramModel(nGramPrefix).ContainsKey(nGramSuffix) Then
                                        ngramModel(nGramPrefix)(nGramSuffix) = 0
                                    End If

                                    ngramModel(nGramPrefix)(nGramSuffix) += 1

                                    ' Update counts for longer n-grams
                                    For j As Integer = ngramSize To 1 Step -1
                                        Dim longerNgramPrefix = String.Join(" ", words, i, j - 1)
                                        If Not ngramModel.ContainsKey(longerNgramPrefix) Then
                                            ngramModel(longerNgramPrefix) = New Dictionary(Of String, Integer)()
                                        End If

                                        If Not ngramModel(longerNgramPrefix).ContainsKey(nGramSuffix) Then
                                            ngramModel(longerNgramPrefix)(nGramSuffix) = 0
                                        End If

                                        ngramModel(longerNgramPrefix)(nGramSuffix) += 1
                                    Next
                                Next
                            Next
                        End Sub

                        Public Shared Function TrainModel(corpus As String) As NgramLanguageModel
                            Dim ngramModel = LanguageModelFactory.CreateNgramLanguageModel(2)
                            ngramModel.CreateModel(corpus)
                            Return ngramModel
                        End Function

                        Public Shared Function TrainModel(data As List(Of String)) As NgramLanguageModel
                            ' Default to ngramLength of 2 if not specified
                            Return TrainModelCodePredictor(data, 2)
                        End Function

                        Public Shared Function TrainModelCodePredictor(data As List(Of String), ngramLength As Integer) As NgramLanguageModel
                            Dim model As NgramLanguageModel = New NgramLanguageModel(ngramLength)

                            For Each line As String In data
                                Dim tokens As String() = line.Split(" "c)
                                Dim sequence As List(Of String) = New List(Of String)()

                                For Each token As String In tokens
                                    sequence.Add(token)

                                    If sequence.Count = ngramLength + 1 Then
                                        Dim context As String = GetContextfrom(sequence.ToArray(), ngramLength - 1)
                                        Dim nextToken As String = sequence(ngramLength)

                                        If model.ngramModel.ContainsKey(context) Then
                                            Dim ngramCounts As Dictionary(Of String, Integer) = model.ngramModel(context)

                                            If ngramCounts.ContainsKey(nextToken) Then
                                                ngramCounts(nextToken) += 1
                                            Else
                                                ngramCounts.Add(nextToken, 1)
                                            End If
                                        Else
                                            Dim ngramCounts As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()
                                            ngramCounts.Add(nextToken, 1)
                                            model.ngramModel.Add(context, ngramCounts)
                                        End If

                                        sequence.RemoveAt(0)
                                    End If
                                Next
                            Next

                            Return model
                        End Function


                    End Class
                    Public Class NgramGenerator
                        Inherits NgramLanguageModel
                        ''' <summary>
                        ''' PlaceHolder ... this model is not to be used as a model
                        ''' </summary>
                        Public Sub New(ByRef model As NgramLanguageModel)
                            MyBase.New(0)

                            Me.ngramEncodings = model.ngramEncodings

                            Me.ngramModel = model.ngramModel

                            Me.ngramSize = model.ngramSize
                        End Sub

                        Public Function GenerateTextHigherOrder(numWords As Integer) As String
                            Dim generatedText As List(Of String) = New List(Of String)()

                            ' Generate text using higher order n-grams
                            Dim startingNgram = GetRandomNgram()
                            generatedText.AddRange(startingNgram.Split(" "c))

                            For i As Integer = 0 To numWords - ngramSize
                                Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                                Dim nextWord = GenerateNextWord(nGramPrefix)
                                generatedText.Add(nextWord)
                            Next

                            Return String.Join(" ", generatedText)
                        End Function

                        Public Function GenerateTextLongTermDependencyModel(numWords As Integer) As String
                            Dim generatedText As List(Of String) = New List(Of String)()

                            ' Generate text considering long-term dependencies
                            Dim startingNgram = GetRandomNgram()
                            generatedText.AddRange(startingNgram.Split(" "c))

                            For i As Integer = 0 To numWords - ngramSize
                                Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                                Dim nextWord = GenerateNextWord(nGramPrefix)
                                generatedText.Add(nextWord)

                                ' Update counts for longer n-grams
                                For j As Integer = ngramSize To 1 Step -1
                                    Dim longerNgramPrefix = String.Join(" ", generatedText.Skip(i).Take(j - 1))
                                    If ngramModel.ContainsKey(longerNgramPrefix) AndAlso ngramModel(longerNgramPrefix).ContainsKey(nextWord) Then
                                        ngramModel(longerNgramPrefix)(nextWord) += 1
                                    End If
                                Next
                            Next

                            Return String.Join(" ", generatedText)
                        End Function

                        Public Function GenerateUniqueSentence() As String
                            Dim sentence As New StringBuilder()
                            Dim currentNgram As String = GetRandomNgram()

                            ' Generate the first word of the sentence
                            Dim words As String() = currentNgram.Split()
                            sentence.Append(words(words.Length - 1))

                            ' Generate subsequent words until reaching an end token or a predefined length
                            Dim nextWord As String = ""
                            Dim maxLength As Integer = 20 ' Maximum length of the generated sentence

                            While Not String.IsNullOrEmpty(nextWord) AndAlso sentence.Length < maxLength
                                sentence.Append(" ")
                                sentence.Append(nextWord)
                                currentNgram = sentence.ToString(sentence.Length - ngramSize - 1, ngramSize)
                                nextWord = PredictNextWord(currentNgram)
                            End While

                            Return sentence.ToString()
                        End Function

                        Public Function GenerateUniqueSentence(ByRef currentNgram As String) As String
                            Dim sentence As New StringBuilder()

                            ' Generate the first word of the sentence
                            Dim words As String() = currentNgram.Split()
                            sentence.Append(words(words.Length - 1))

                            ' Generate subsequent words until reaching an end token or a predefined length
                            Dim nextWord As String = ""
                            Dim maxLength As Integer = 20 ' Maximum length of the generated sentence

                            While Not String.IsNullOrEmpty(nextWord) AndAlso sentence.Length < maxLength
                                sentence.Append(" ")
                                sentence.Append(nextWord)
                                currentNgram = sentence.ToString(sentence.Length - ngramSize - 1, ngramSize)
                                nextWord = PredictNextWord(currentNgram)
                            End While

                            Return sentence.ToString()
                        End Function
                        Public Function PredictSentence(sentence As String) As String
                            Dim words As String() = sentence.Split()
                            Dim predictedSentence As New StringBuilder(String.Join(" ", words))

                            Dim currentNgram As String = String.Join(" ", words, words.Length - ngramSize, ngramSize)
                            Dim nextWord As String = PredictNextWord(currentNgram)

                            While Not String.IsNullOrEmpty(nextWord)
                                predictedSentence.Append(" ").Append(nextWord)
                                currentNgram = predictedSentence.ToString(predictedSentence.Length - ngramSize - 1, ngramSize)
                                nextWord = PredictNextWord(currentNgram)
                            End While

                            Return predictedSentence.ToString()
                        End Function



                    End Class
                    Public Class NGramScorer
                        Inherits NgramLanguageModel
                        ''' <summary>
                        ''' PlaceHolder ... this model is not to be used as a model
                        ''' A response can be generated by adding new sets of response which can be scored, 
                        ''' the highest response is selected
                        ''' </summary>
                        Public Sub New(ByRef model As NgramLanguageModel)
                            MyBase.New(model.ngramSize)

                            Me.ngramEncodings = model.ngramEncodings

                            Me.ngramModel = model.ngramModel

                            Me.ngramSize = model.ngramSize
                        End Sub
                        ''' <summary>
                        ''' Response (lang model(highest score))
                        ''' Loads model For response scoring , 
                        ''' trains model on current responses
                        ''' </summary>
                        ''' <param name="responses"></param>
                        ''' <param name="ngramsize"></param>
                        Public Sub New(ByRef responses As List(Of String), ngramsize As Integer)
                            MyBase.New(ngramsize)
                            Dim str As String = ""
                            For Each item In responses
                                str &= " " & item
                                'Add to Vocab
                                AddDocument(item)
                            Next
                            'Encodes Model
                            CreateEncodedModel(str)
                            'TrainModel
                            Train(responses)
                            Dim trainer As New NgramFunctions.NgramTrainer(Me)
                            trainer.TrainHigherOrder(responses)
                            Me.ngramEncodings = trainer.ngramEncodings

                            Me.ngramModel = trainer.ngramModel

                            Me.ngramSize = trainer.ngramSize
                        End Sub
                        Public ReadOnly Property Model As NgramLanguageModel
                            Get
                                Return Me
                            End Get
                        End Property

                        ''' <summary>
                        ''' Loads model For response scoring , 
                        ''' trains model on current responses, Also past responses
                        ''' </summary>
                        ''' <param name="responses"></param>
                        ''' <param name="ngramsize"></param>
                        Public Sub AddNew(ByRef responses As List(Of String))

                            Dim str As String = ""
                            For Each item In responses
                                str &= " " & item
                                'Add to Vocab
                                AddDocument(item)
                            Next
                            'Encodes Model
                            CreateEncodedModel(str)
                            'TrainModel
                            Train(responses)
                            Dim trainer As New NgramFunctions.NgramTrainer(Me)
                            trainer.TrainHigherOrder(responses)
                            Me.ngramEncodings = trainer.ngramEncodings

                            Me.ngramModel = trainer.ngramModel


                        End Sub

                        Public Function GenerateResponse(question As String) As String
                            Dim words As String() = question.Split()
                            Dim ngram As String = String.Join(" ", words.Skip(words.Length - ngramModel.Count + 1))

                            Dim scores As Dictionary(Of String, Integer) = ScoreNGram(ngram)
                            Dim prediction As String = scores.OrderByDescending(Function(x) x.Value).FirstOrDefault().Key

                            Return prediction
                        End Function

                        Public Function ScoreNGram(ngram As String) As Dictionary(Of String, Integer)
                            If ngramModel.ContainsKey(ngram) Then
                                Return ngramModel(ngram)
                            Else
                                Return New Dictionary(Of String, Integer)
                            End If
                        End Function



                    End Class
                End Class
            End Class
            ''' <summary>
            ''' Corpus Language Model
            ''' Used to HoldDocuments : a corpus of documents Calculating detecting the 
            ''' known entitys and topics in the model; 
            ''' A known list of Entitys and Topics are required to create this model
            ''' This language model is ideally suited for NER / and other corpus interogations
            ''' 
            ''' </summary>
            ''' <summary>
            ''' Corpus Language Model
            ''' Used to HoldDocuments : a corpus of documents Calculating detecting the 
            ''' known entitys and topics in the model; 
            ''' A known list of Entitys and Topics are required to create this model
            ''' This language model is ideally suited for NER / and other corpus interogations
            ''' 
            ''' </summary>
            Public Class Corpus
                ''' <summary>
                ''' Serializes object to json
                ''' </summary>
                ''' <returns> </returns>
                Public Function ToJson() As String
                    Dim Converter As New JavaScriptSerializer
                    Return Converter.Serialize(Me)
                End Function
                ''' <summary>
                ''' Used to create NewCorpus - With Or Without a Recognition template
                ''' </summary>
                Public Class ProcessInputAPI
                    Private iCurrentOriginalText As String
                    Private KnownEntitys As Corpus.Recognition_Data

                    Public Sub New(ByRef KnownData As Corpus.Recognition_Data)
                        Me.KnownEntitys = KnownData
                    End Sub

                    Public Sub New()
                        KnownEntitys = New Corpus.Recognition_Data
                    End Sub

                    Public ReadOnly Property CurrentInput As String
                        Get
                            Return iCurrentOriginalText
                        End Get
                    End Property

                    Public Function ProcessDocument(ByRef InputText As String) As Corpus
                        Dim iCorpus As New Corpus(KnownEntitys)
                        iCorpus.AddDocument(InputText)
                        Return iCorpus
                    End Function

                    Public Function ProcessCorpus(ByRef InputText As List(Of String)) As Corpus
                        Dim iCorpus As New Corpus(KnownEntitys)
                        iCorpus.AddCorpus(InputText)
                        Return iCorpus
                    End Function

                End Class

                ''' <summary>
                ''' An array of characters (. ! ?) used to tokenize sentences.
                ''' </summary>
                Public Shared ReadOnly SentenceEndMarkers As Char() = {".", "!", "?"}

                Public CorpusContext As List(Of Vocabulary.FeatureContext)

                ''' <summary>
                ''' A list of strings representing the documents in the corpus.
                ''' </summary>
                Public CorpusDocs As List(Of String)

                ''' <summary>
                ''' A string representing the concatenated text of all documents in the corpus.
                ''' </summary>
                Public CorpusText As String

                ''' <summary>
                '''  A list of unique words in the corpus.
                ''' </summary>
                Public CorpusUniqueWords As List(Of String)

                ''' <summary>
                ''' TotalWords in Corpus
                ''' </summary>
                Public ReadOnly Property CorpusWordcount As Integer
                    Get
                        Return GetWordCount()
                    End Get
                End Property

                ''' <summary>
                '''  A list of Document objects representing individual documents in the corpus.
                ''' </summary>
                Public Documents As List(Of Document)

                ''' <summary>
                ''' A list of Entity structures representing detected entities in the corpus.
                ''' </summary>
                Public Entitys As List(Of Entity)

                ''' <summary>
                ''' A Vocabulary object representing the language model.
                ''' </summary>
                Public Langmodel As Vocabulary

                ''' <summary>
                ''' A Recognition_Data structure representing named entity recognition data.
                ''' </summary>
                Public NER As Recognition_Data

                ''' <summary>
                ''' A list of Topic structures representing detected topics in the corpus.
                ''' </summary>
                Public Topics As List(Of Topic)

                ''' <summary>
                ''' Initializes a new instance of the Corpus class.
                ''' </summary>
                ''' <param name="data">The recognition data for entity and topic detection.</param>
                Public Sub New(ByVal data As Recognition_Data)
                    NER = data
                    Documents = New List(Of Document)
                    Entitys = New List(Of Entity)
                    Topics = New List(Of Topic)
                    CorpusDocs = New List(Of String)
                    CorpusUniqueWords = New List(Of String)
                    CorpusText = String.Empty

                    Langmodel = New Vocabulary
                End Sub

                ''' <summary>
                ''' type of sentence
                ''' </summary>
                Public Enum SentenceType
                    Unknown = 0
                    Declaritive = 1
                    Interogative = 2
                    Exclamitory = 3
                    Conditional = 4
                    Inference = 5
                    Imperitive = 6
                End Enum

                ''' <summary>
                ''' Processes the text by removing unwanted characters, converting to lowercase, and removing extra whitespace.
                ''' </summary>
                ''' <param name="text"></param>
                ''' <returns></returns>
                Public Shared Function ProcessText(ByRef text As String) As String
                    ' Remove unwanted characters
                    Dim processedText As String = Regex.Replace(text, "[^a-zA-Z0-9\s]", "")

                    ' Convert to lowercase
                    processedText = processedText.ToLower()

                    ' Remove extra whitespace
                    processedText = Regex.Replace(processedText, "\s+", " ")

                    Return processedText
                End Function

                ''' <summary>
                ''' Adds a corpus of documents to the existing corpus.
                ''' </summary>
                ''' <param name="docs"></param>
                ''' <returns></returns>
                Public Function AddCorpus(ByRef docs As List(Of String)) As Corpus

                    'Add aCorpus of documents to the corpus

                    For Each item In docs

                        AddDocument(item)

                    Next
                    UpdateContext()
                    Return Me

                End Function

                ''' <summary>
                ''' Adds a document to the corpus and updates the corpus properties.
                ''' </summary>
                ''' <param name="Text"></param>
                Public Sub AddDocument(ByRef Text As String)
                    Dim Doc As New Document(Text)
                    Documents.Add(Doc.AddDocument(ProcessText(Text)))
                    'Update Corpus
                    CorpusDocs.Add(ProcessText(Text))

                    CorpusUniqueWords = GetUniqueWords()

                    Dim iText As String = ""
                    For Each item In Documents
                        iText &= item.ProcessedText & vbNewLine

                    Next
                    CorpusText = iText

                    '' corpus entitys and topics
                    Doc.Entitys = Entity.DetectEntitys(Doc.ProcessedText, NER.Entitys)
                    Doc.Topics = Topic.DetectTopics(Doc.ProcessedText, NER.Topics)
                    Entitys.AddRange(Doc.Entitys)
                    Entitys = Entitys

                    Topics.AddRange(Doc.Topics)
                    Topics = Topics
                    'Update VocabModel

                    Dim Wrds = Text.Split(" ")

                    For Each item In Wrds
                        Langmodel.AddNew(item, CorpusDocs)
                    Next
                End Sub

                ''' <summary>
                ''' Retrieves the list of unique words in the corpus.
                ''' </summary>
                ''' <returns></returns>
                Public Function GetUniqueWords() As List(Of String)
                    Dim lst As New List(Of String)
                    For Each item In Documents
                        lst.AddRange(item.UniqueWords)
                    Next
                    Return lst
                End Function

                ''' <summary>
                ''' Retrieves the total word count in the corpus.
                ''' </summary>
                ''' <returns></returns>
                Public Function GetWordCount() As Integer
                    Dim count As Integer = 0
                    For Each item In Documents
                        count += item.WordCount
                    Next
                    Return count
                End Function

                ''' <summary>
                ''' Updates the Features in the model (each document context)
                ''' by the topics discovered in the text, updating the individual documents and adding the
                ''' feature context to the corpus context
                ''' </summary>
                Private Sub UpdateContext()
                    CorpusContext = New List(Of Vocabulary.FeatureContext)
                    For Each Topic In Topics.Distinct
                        For Each doc In Documents
                            Dim Context = Vocabulary.FeatureContext.GetDocumentContext(Langmodel, doc, Topic.Topic)
                            doc.Features.Add(Context)
                            CorpusContext.Add(Context)
                        Next
                    Next

                End Sub

                ''' <summary>
                ''' Represents an individual document in the corpus. It contains properties such as word count, processed text, sentences, topics, etc.
                ''' </summary>
                Public Structure Document

                    Public ReadOnly Property WordCount As Integer
                        Get
                            Return GetWordCount()
                        End Get
                    End Property

                    Private Function GetWordCount() As Integer
                        Dim Str = Functs.TokenizeWords(OriginalText)
                        Return Str.Count
                    End Function

                    '''' <summary>
                    '''' COntains the Vocabulary for this document
                    '''' </summary>
                    Public DocumentVocabulary As Vocabulary

                    Public Entitys As List(Of Entity)

                    ''' <summary>
                    ''' Context can be updated by the corpus owner as required, these contexts
                    ''' can be used to score the document and provided higher embedding values
                    ''' </summary>
                    Public Features As List(Of Vocabulary.FeatureContext)

                    ''' <summary>
                    ''' Preserve original
                    ''' </summary>
                    Public OriginalText As String

                    ''' <summary>
                    ''' Cleaned Text
                    ''' </summary>
                    Public ProcessedText As String

                    ''' <summary>
                    ''' Sentences within Text
                    ''' </summary>
                    Public Sentences As List(Of Sentence)

                    Public Topics As List(Of Topic)
                    Public TopWords As List(Of String)
                    Public UniqueWords As List(Of String)

                    Public Sub New(ByRef originalText As String)

                        Me.OriginalText = originalText
                        Topics = New List(Of Topic)
                        TopWords = New List(Of String)
                        UniqueWords = New List(Of String)
                        Sentences = New List(Of Sentence)
                        DocumentVocabulary = New Vocabulary
                        Entitys = New List(Of Entity)
                    End Sub

                    Public Function AddDocument(ByRef Text As String) As Document
                        OriginalText = Text
                        'Remove unwanted symbols
                        ProcessedText = ProcessText(Text)

                        Dim Sents As List(Of String) = Text.Split(".").ToList
                        Dim Count As Integer = 0
                        For Each item In Sents
                            Count += 1
                            Dim Sent As New Sentence(item)
                            Me.Sentences.Add(Sent.AddSentence(item, Count))
                        Next
                        UniqueWords = Corpus.Functs.GetUniqueWordsInText(ProcessedText)
                        Dim IDocs As New List(Of String)
                        'Adds only its-self to its own personal corpus vocabulary(document Specific)
                        IDocs.Add(ProcessedText)
                        For Each item In UniqueWords
                            DocumentVocabulary.AddNew(item, IDocs)
                        Next
                        TopWords = Corpus.Functs.GetTopWordsInText(ProcessedText)

                        Return Me
                    End Function

                    Public Structure Sentence

                        Public Clauses As List(Of Clause)

                        Public Entitys As List(Of Entity)

                        Public OriginalSentence As String

                        Public Position As Integer

                        Public ProcessedSentence As String

                        Public UniqueWords As List(Of String)

                        Private iSentencetype As SentenceType

                        Public Sub New(originalSentence As String)
                            Me.New()
                            Me.OriginalSentence = originalSentence
                            Clauses = New List(Of Clause)
                            Entitys = New List(Of Entity)
                            UniqueWords = New List(Of String)
                        End Sub

                        Public ReadOnly Property ClauseCount As Integer
                            Get
                                Return Clauses.Count
                            End Get

                        End Property

                        Public ReadOnly Property SentenceType As String
                            Get
                                Select Case iSentencetype
                                    Case Corpus.SentenceType.Conditional
                                        Return "Conditional"
                                    Case Corpus.SentenceType.Declaritive
                                        Return "Declarative"
                                    Case Corpus.SentenceType.Exclamitory
                                        Return "exclamatory"
                                    Case Corpus.SentenceType.Imperitive
                                        Return "imperative"
                                    Case Corpus.SentenceType.Inference
                                        Return "inference"
                                    Case Corpus.SentenceType.Interogative
                                        Return "interrogative"
                                    Case Corpus.SentenceType.Unknown
                                        Return "unknown"
                                    Case Else
                                        Return "unknown"
                                End Select
                            End Get
                        End Property

                        Public ReadOnly Property WordCount As Integer
                            Get
                                Return GetWordCount(ProcessedSentence)
                            End Get
                        End Property

                        Public Shared Function GetClauses(ByRef Text As String) As List(Of Clause)
                            Dim clauses As New List(Of Clause)

                            '

                            If Text.Contains(",") Then
                                Dim iClauses As List(Of String) = Text.Split(",").ToList
                                For Each item In iClauses
                                    Dim Iclause As New Clause
                                    Iclause.Text = item
                                    Iclause.ClauseSeperator = ","
                                    Dim Words = Functs.TokenizeWords(Iclause.Text)
                                    Dim count As Integer = 0
                                    For Each wrd In Words
                                        count += 1
                                        Iclause.Words.Add(New Clause.Word(wrd, count))

                                    Next

                                    clauses.Add(Iclause)

                                Next
                            Else

                                'Add detect end punctuation use for

                                Dim Iclause As New Clause
                                Iclause.Words = New List(Of Clause.Word)
                                Iclause.Text = Text
                                'Use end punctuation
                                Iclause.ClauseSeperator = "."
                                Dim Words = Functs.TokenizeWords(Iclause.Text)
                                Dim count As Integer = 0
                                If Words.Count > 0 Then
                                    For Each wrd In Words

                                        count += 1
                                        Iclause.Words.Add(New Clause.Word(wrd, count))

                                    Next
                                End If
                                clauses.Add(Iclause)

                            End If
                            Return clauses
                        End Function

                        Public Function AddSentence(ByRef text As String, ByRef iPosition As Integer) As Sentence
                            OriginalSentence = text
                            ProcessedSentence = ProcessText(text)
                            Clauses = GetClauses(ProcessedSentence)
                            UniqueWords = Corpus.Functs.GetUniqueWordsInText(ProcessedSentence)

                            Position = iPosition
                            Return Me
                        End Function

                        Private Function GetWordCount(ByRef Text As String) As Integer
                            Dim Str = Functs.TokenizeWords(Text)
                            Return Str.Count
                        End Function

                        ''' <summary>
                        ''' Represents a clause within a sentence. It contains properties such as text, word count, words, etc.
                        ''' </summary>
                        Public Structure Clause

                            ''' <summary>
                            ''' Independent Clause / Dependant Clause
                            ''' </summary>
                            Public Clause As String

                            Public ClauseSeperator As String
                            Public ClauseType As SentenceType

                            ''' <summary>
                            ''' Note: if = "." then declarative, = "?" Question = "!" Exclamitory
                            ''' </summary>
                            Public EndPunctuation As String

                            Public Text As String
                            Public Words As List(Of Clause.Word)
                            Private mLearningPattern As String

                            Private mPredicate As String

                            Private mSubjectA As String

                            Private mSubjectB As String

                            ''' <summary>
                            ''' the learning pattern locates the Subjects in the sentence A# sat on #b
                            ''' </summary>
                            ''' <returns></returns>
                            Public Property LearningPattern As String
                                Get
                                    Return mLearningPattern
                                End Get
                                Set(value As String)
                                    mLearningPattern = value
                                End Set
                            End Property

                            ''' <summary>
                            ''' Predicate / Linking verb / Concept (Sat on) (is sitting on) (AtLocation) this is the
                            ''' dividing content in the sentence
                            ''' </summary>
                            ''' <returns></returns>
                            Public Property Predicate As String
                                Get
                                    Return mPredicate
                                End Get
                                Set(value As String)
                                    mPredicate = value
                                End Set
                            End Property

                            ''' <summary>
                            ''' First detected subject (the Cat)
                            ''' </summary>
                            ''' <returns></returns>
                            Public Property SubjectA As String
                                Get
                                    Return mSubjectA
                                End Get
                                Set(value As String)
                                    mSubjectA = value
                                End Set
                            End Property

                            ''' <summary>
                            ''' Second detected subject / Object (the mat)
                            ''' </summary>
                            ''' <returns></returns>
                            Public Property SubjectB As String
                                Get
                                    Return mSubjectB
                                End Get
                                Set(value As String)
                                    mSubjectB = value
                                End Set
                            End Property

                            Public ReadOnly Property WordCount As Integer
                                Get
                                    Return Words.Count
                                End Get

                            End Property

                            ''' <summary>
                            ''' Represents a word in the text
                            ''' </summary>
                            Public Structure Word

                                ''' <summary>
                                ''' Position of word in Sentence/Document
                                ''' </summary>
                                Public Position As Integer

                                ''' <summary>
                                ''' Word
                                ''' </summary>
                                Public text As String

                                Public Sub New(word As String, position As Integer)
                                    If word Is Nothing Then
                                        Throw New ArgumentNullException(NameOf(word))
                                    End If

                                    Me.text = word
                                    Me.Position = position

                                End Sub

                            End Structure

                        End Structure

                    End Structure

                End Structure
                Public Structure Entity
                    Public Property EndIndex As Integer
                    Public Property StartIndex As Integer
                    Public Property Type As String
                    Public Property Value As String
                    Public Shared Function DetectEntitys(ByRef text As String, EntityList As List(Of Entity)) As List(Of Entity)
                        Dim detectedEntitys As New List(Of Entity)()

                        ' Perform entity detection logic here
                        For Each item In EntityList
                            If text.Contains(item.Value) Then
                                detectedEntitys.Add(item)
                            End If
                        Next

                        Return detectedEntitys
                    End Function
                End Structure
                ''' <summary>
                ''' NER Data held(known) by the corpus
                ''' </summary>
                Public Class Recognition_Data
                    Public Entitys As List(Of Entity)
                    Public Topics As List(Of Topic)

                    Public Sub New()
                        Entitys = New List(Of Entity)
                        Topics = New List(Of Topic)
                    End Sub

                End Class

                Public Structure Term
                    Public DocNumber As List(Of Integer)

                    ''' <summary>
                    ''' Term Frequency
                    ''' </summary>
                    Dim Freq As Integer

                    ''' <summary>
                    ''' Inverse Document Frequency
                    ''' </summary>
                    Dim IDF As Double

                    ''' <summary>
                    ''' Value
                    ''' </summary>
                    Dim Term As String

                End Structure

                ''' <summary>
                ''' Represents a topic detected in the text. It has properties for the keyword and topic itself.
                ''' </summary>
                Public Structure Topic
                    Public Keyword As String
                    Public Topic As String

                    Public Shared Function DetectTopics(ByRef text As String, TopicList As List(Of Topic)) As List(Of Topic)
                        Dim detectedTopics As New List(Of Topic)()
                        For Each item In TopicList
                            If text.ToLower.Contains(item.Keyword) Then
                                detectedTopics.Add(item)
                            End If
                        Next

                        Return detectedTopics
                    End Function

                End Structure

                Public Class Functs

                    ''' <summary>
                    ''' Returns the top words in a given text
                    ''' </summary>
                    ''' <param name="text"></param>
                    ''' <returns></returns>
                    Public Shared Function GetTopWordsInText(ByRef text As String) As List(Of String)
                        Dim words As List(Of String) = Functs.TokenizeWords(text)
                        Dim wordCounts As New Dictionary(Of String, Integer)()

                        For Each word As String In words
                            If wordCounts.ContainsKey(word) Then
                                wordCounts(word) += 1
                            Else
                                wordCounts(word) = 1
                            End If
                        Next

                        ' Sort the words based on their counts in descending order
                        Dim sortedWords As List(Of KeyValuePair(Of String, Integer)) = wordCounts.OrderByDescending(Function(x) x.Value).ToList()

                        ' Get the top 10 words
                        Dim topWords As List(Of String) = sortedWords.Take(10).Select(Function(x) x.Key).ToList()

                        Return topWords
                    End Function

                    ''' <summary>
                    ''' Returns a list of the unique words in the text
                    ''' </summary>
                    ''' <param name="text"></param>
                    ''' <returns></returns>
                    Public Shared Function GetUniqueWordsInText(ByRef text As String) As List(Of String)
                        Dim words As List(Of String) = Functs.TokenizeWords(text)
                        Dim uniqueWords As List(Of String) = words.Distinct().ToList()
                        Return uniqueWords
                    End Function

                    Public Shared Sub PrintSentencesToConsole(ByRef iSentences As List(Of String))
                        For Each sentence In iSentences
                            Console.WriteLine(sentence)
                        Next
                    End Sub

                    ''' <summary>
                    ''' Tokenizes the text into sentences based on punctuation end markers.
                    ''' </summary>
                    ''' <param name="text">The text to tokenize.</param>
                    ''' <returns>A list of sentences.</returns>
                    Public Shared Function TokenizeSentences(ByVal text As String) As List(Of Document.Sentence)
                        Dim sentences As New List(Of Document.Sentence)()

                        ' Split text into sentences based on punctuation end markers
                        Dim pattern As String = $"(?<=[{String.Join("", SentenceEndMarkers)}])\s+"
                        Dim sentenceTexts As String() = Regex.Split(text, pattern)

                        For Each sentenceText As String In sentenceTexts
                            Dim isentence As New Document.Sentence()
                            isentence.OriginalSentence = sentenceText.Trim()

                            isentence.Clauses = Document.Sentence.GetClauses(text)
                            ' ... other sentence properties ...
                            sentences.Add(isentence)
                        Next

                        Return sentences
                    End Function

                    ''' <summary>
                    ''' Tokenizes the sentence into words.
                    ''' </summary>
                    ''' <param name="sentenceText">The text of the sentence.</param>
                    ''' <returns>A list of words.</returns>
                    Public Shared Function TokenizeWords(ByVal sentenceText As String) As List(Of String)
                        Dim words As New List(Of String)()

                        ' Split sentence into words
                        Dim wordPattern As String = "\b\w+\b"
                        Dim wordMatches As MatchCollection = Regex.Matches(sentenceText, wordPattern)

                        For Each match As Match In wordMatches
                            words.Add(match.Value.ToLower())
                        Next

                        Return words
                    End Function

                    Public Shared Function Top_N_Words(ByRef iDocContents As String, ByRef n As Integer) As List(Of String)
                        Dim words As String() = iDocContents.Split(" ")
                        Dim wordCount As New Dictionary(Of String, Integer)

                        ' Count the frequency of each word in the corpus
                        For Each word As String In words
                            If wordCount.ContainsKey(word) Then
                                wordCount(word) += 1
                            Else
                                wordCount.Add(word, 1)
                            End If
                        Next

                        ' Sort the dictionary by value (frequency) in descending order
                        Dim sortedDict = (From entry In wordCount Order By entry.Value Descending Select entry).Take(n)
                        Dim LSt As New List(Of String)
                        ' Print the top ten words and their frequency
                        For Each item In sortedDict
                            LSt.Add(item.Key)

                        Next
                        Return LSt
                    End Function

                End Class

                ''' <summary>
                ''' Represents the vocabulary model for the corpus.
                ''' (a record of words which can be looked up in the corpus)
                ''' It includes methods for adding new terms, calculating frequencies, TF-IDF, etc.
                ''' </summary>
                Public Class Vocabulary
                    Public Current As List(Of VocabularyEntry)

                    ''' <summary>
                    ''' Used for TDM Calc
                    ''' </summary>
                    Private Docs As List(Of String)

                    ''' <summary>
                    ''' Prepare vocabulary for use
                    ''' </summary>
                    Public Sub New()
                        Current = New List(Of VocabularyEntry)
                        Docs = New List(Of String)
                    End Sub
                    Public Function GetVocab() As List(Of String)
                        Dim lst As New List(Of String)
                        For Each item In Current
                            lst.Add(item.Text)
                        Next
                        Return lst
                    End Function
                    ''' <summary>
                    ''' Used to add Words or update a word in the vocabulary language model
                    ''' </summary>
                    ''' <param name="Term"></param>
                    ''' <param name="Docs">Current Collection of Corpus Documents(Updated) - 
                    ''' This is used when adding a new document and its terms - 
                    ''' the documents are not processed only the terms added the documents are just a record,
                    ''' enabling for other calculations to take place internally</param>
                    Public Sub AddNew(ByRef Term As String, ByRef Docs As List(Of String))
                        Me.Docs = Docs
                        Current.Add(New VocabularyEntry(Term,
                      CalcSequenceEncoding(Term),
                      CalcFrequency(Term),
                      CalcTF_IDF(Term)))

                    End Sub
                    ''' <summary>
                    ''' Used to add Words or update a word in the vocabulary language model
                    ''' </summary>
                    ''' <param name="Term"></param>

                    Public Sub AddNew(ByRef Term As String)

                        Current.Add(New VocabularyEntry(Term,
                      CalcSequenceEncoding(Term),
                      CalcFrequency(Term),
                      CalcTF_IDF(Term)))

                    End Sub
                    Private Function CalcFrequency(ByRef Word As String) As Double
                        ' Calculate frequency of term in the corpus (current)
                        Dim count As Integer = 0
                        For Each entry In Current
                            If entry.Text = Word Then

                                count += 1 + entry.Frequency
                            Else
                                Return 1
                            End If
                        Next
                        Return count
                    End Function

                    Private Function CalcInverseDocumentFrequency(ByRef Word As String, ByRef Docs As List(Of String)) As Double
                        ' Calculate Inverse Document Frequency for the given term in the corpus
                        Dim docsWithTerm As Integer = 0
                        For Each doc In Docs
                            If doc.Contains(Word) Then
                                docsWithTerm += 1
                            End If
                        Next
                        Dim idf As Double = Math.Log(Docs.Count / (docsWithTerm + 1)) ' Adding 1 to avoid division by zero
                        Return idf
                    End Function

                    Private Function CalcSequenceEncoding(ByRef Word As String) As Double
                        ' Calculate sequence encoding based on the order of appearance in the corpus?(no good)
                        Dim encoding As Double = 0.0
                        For Each entry In Current
                            If entry.Text = Word Then
                                encoding += 1
                            End If
                        Next
                        Return encoding
                    End Function

                    Private Function CalcTermFrequency(ByRef Word As String) As Double
                        ' Calculate Term Frequency for the given term in the corpus
                        Dim count As Integer = 0
                        For Each entry In Current
                            If entry.Text = Word Then
                                count += 1
                            End If
                        Next
                        Return count
                    End Function

                    Private Function CalcTF_IDF(ByRef Word As String) As Double
                        ' Calculate TF-IDF (Term Frequency-Inverse Document Frequency) for the given term in the corpus
                        Dim tf As Double = CalcTermFrequency(Word)
                        Dim idf As Double = CalcInverseDocumentFrequency(Word, Docs)
                        Return tf * idf
                    End Function

                    ''' <summary>
                    ''' Feature context is a way to add information with regards to the document,
                    ''' Addind context elements such as features.
                    ''' Given a Sentiment (positive) , by marking the words in this document
                    ''' present against the corpus vocabulary, it could be suggested that these would
                    ''' represent that topic in this document
                    ''' </summary>
                    Public Structure FeatureContext

                        ''' <summary>
                        ''' List of items Representing the context,
                        ''' All entrys contained in the vocabulary are marked with a tag (present)(true)
                        ''' if the are in the context else marked false
                        ''' giving a one-shot encoding for the context this collection represents,
                        ''' Ie:Sentiment/Topic etc
                        ''' </summary>
                        Public Present As List(Of VocabularyEntry)

                        Public Type As String

                        ''' <summary>
                        ''' Encodes a Feature into the model,
                        ''' Provide the label and the document words in the document
                        ''' will be marked present in the context
                        ''' Later these Oneshot encoding feature maybe used to increase the scoring vectors
                        ''' Adding context to the document for a specific feature such as sentiment / Emotion / Topic.
                        ''' Each topic should be encoded as a feature in the document
                        '''
                        ''' </summary>
                        ''' <param name="CorpusVocab">Current Vocabulary </param>
                        ''' <param name="iDocument"></param>
                        ''' <param name="Label"></param>
                        ''' <returns></returns>
                        Public Shared Function GetDocumentContext(ByRef CorpusVocab As Vocabulary, ByRef iDocument As Document, ByRef Label As String) As Vocabulary.FeatureContext
                            Dim iContext As New Vocabulary.FeatureContext
                            Dim NewVocab As List(Of Vocabulary.VocabularyEntry) = CorpusVocab.Current

                            For Each item In NewVocab
                                For Each _item In iDocument.UniqueWords
                                    If item.Text = _item Then
                                        'Encode Presence in text
                                        item.Present = True
                                    End If
                                Next
                            Next
                            iContext.Present = NewVocab
                            iContext.Type = Label
                            Return iContext
                        End Function

                    End Structure

                    Public Shared Sub Main()
                        'Create Vocabulary
                        Dim iCorpus As String = "the quick brown fox, jumped over the lazy dog."
                        Dim NewVocabulary = Vocabulary.CreateVocabulary(iCorpus, Vocabulary.VocabularyType.Word)
                        Console.WriteLine("vocabulary List: ")
                        Dim str As String = ""
                        For Each item In NewVocabulary
                            str &= "entry :" & item.Text & vbTab & "Value :" & item.Encoding & vbNewLine

                        Next
                        Console.WriteLine(str)
                        'Encode InputText
                        Dim InputText As String = iCorpus

                        Dim InputLayer As New InputTextRecord
                        InputLayer.Text = iCorpus
                        Console.WriteLine("Input layer: ")
                        InputLayer.Encoding = Encode.Encode_Text(InputText, NewVocabulary, VocabularyType.Word)
                        Console.WriteLine("Input Text: " & "[" & InputLayer.Text & "]" & vbNewLine)
                        Console.WriteLine("Input Embedding: ")
                        str = "["
                        For Each item In InputLayer.Encoding
                            str &= item & " "
                        Next
                        str &= "] "
                        Console.WriteLine(str)
                        Console.WriteLine(vbNewLine)
                        'get inputs
                        InputLayer.blocksize = 4
                        InputLayer.Inputblocks = InputTextRecord.GetBlocks(InputLayer.Encoding, InputLayer.blocksize)
                        Console.WriteLine("Input BlockSize: " & InputLayer.blocksize)
                        Console.WriteLine("Input Blocks ")
                        For Each lst In InputLayer.Inputblocks

                            Dim block As String = ""
                            For Each item In lst
                                block &= item & " "
                            Next
                            Console.WriteLine("[" & block & "]")
                        Next
                        Console.WriteLine(vbNewLine)
                        Dim ofset = 1
                        'get targets(add ofset to get targets further in the future   ofset < blocksize)

                        InputLayer.Targetblocks = InputTextRecord.GetTargetBlocks(InputLayer.Encoding, InputLayer.blocksize)

                        Console.WriteLine("Target BlockSize: " & InputLayer.blocksize)
                        Console.WriteLine("Target ofset    : " & ofset)
                        Console.WriteLine("Target Blocks  ")
                        For Each lst In InputLayer.Targetblocks

                            Dim block As String = ""
                            For Each item In lst
                                block &= item & " "
                            Next
                            Console.WriteLine("[" & block & "]")
                        Next
                        Console.ReadLine()

                    End Sub

                    Public Structure InputTextRecord
                        Public Text As String
                        Public Encoding As List(Of Integer)
                        Public Inputblocks As List(Of List(Of Integer))
                        Public Targetblocks As List(Of List(Of Integer))
                        Public blocksize As Integer

                        Public Shared Function GetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer, Optional Ofset As Integer = 0) As List(Of List(Of Integer))
                            Dim pos As Integer = 0
                            Dim newPos As Integer = Size
                            Dim blocks As New List(Of List(Of Integer))
                            Dim block As New List(Of Integer)
                            Do While pos < Embedding.Count - 1
                                For i = pos To newPos - 1
                                    If Ofset > 0 Then
                                        If i + Ofset < Embedding.Count - 1 Then

                                            block.Add(Embedding(i + Ofset))
                                            'block.Add(Embedding(i))
                                        Else
                                            block.Add(Embedding(i))
                                        End If
                                    Else
                                        block.Add(Embedding(i))
                                    End If

                                Next
                                blocks.Add(block)
                                block = New List(Of Integer)
                                pos = newPos

                                If newPos + Size < Embedding.Count Then
                                    newPos += Size
                                Else
                                    newPos = Embedding.Count
                                End If

                            Loop

                            Return blocks
                        End Function

                        Public Shared Function GetTargetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer) As List(Of List(Of Integer))
                            Embedding.RemoveAt(0)
                            GetBlocks(Embedding, Size)
                            Dim pos As Integer = 0
                            Dim newPos As Integer = Size
                            Dim blocks As New List(Of List(Of Integer))
                            Dim block As New List(Of Integer)
                            Do While pos < Embedding.Count - 1
                                For i = pos To newPos - 1
                                    block.Add(Embedding(i))

                                Next
                                blocks.Add(block)
                                block = New List(Of Integer)
                                pos = newPos
                                If newPos + Size < Embedding.Count - 1 Then
                                    newPos += Size
                                Else
                                    newPos = Embedding.Count
                                End If

                            Loop

                            Return blocks
                        End Function

                    End Structure

                    Public Class Encode

                        Public Shared Function Encode_Text(ByRef Text As String, ByRef Vocab As List(Of VocabularyEntry), ByRef Type As VocabularyType) As List(Of Integer)
                            Dim iOutput As New List(Of Integer)
                            Select Case Type
                                Case VocabularyType.Character
                                    Dim Chars = Tokenizer.TokenizeByCharacter(Text)

                                    For Each item In Chars
                                        If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                            iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                        End If
                                    Next
                                Case VocabularyType.Word
                                    Dim Words = Tokenizer.TokenizeByWord(Text)

                                    For Each item In Words
                                        If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                            iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                        End If
                                    Next
                                Case VocabularyType.Sentence
                                    Dim Sents = Tokenizer.TokenizeBySentence(Text)

                                    For Each item In Sents
                                        If CheckVocabulary(item.Value, Vocab) = True Then
                                            iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                        End If
                                    Next
                            End Select
                            Return iOutput
                        End Function

                        Public Shared Function EncodeChars(VocabList As List(Of String)) As List(Of VocabularyEntry)
                            Dim vocabulary As New List(Of VocabularyEntry)
                            Dim EncodingValue As Integer = 1
                            For Each item In VocabList
                                Dim newVocabRecord As New VocabularyEntry
                                newVocabRecord.Encoding = EncodingValue
                                newVocabRecord.Text = item
                                EncodingValue += 1
                                vocabulary.Add(newVocabRecord)
                            Next
                            Return vocabulary
                        End Function

                        Public Shared Function EncodeWords(VocabList As List(Of String)) As List(Of VocabularyEntry)
                            Dim vocabulary As New List(Of VocabularyEntry)
                            Dim EncodingValue As Integer = 1
                            For Each item In VocabList
                                Dim newVocabRecord As New VocabularyEntry
                                newVocabRecord.Encoding = EncodingValue
                                newVocabRecord.Text = item
                                EncodingValue += 1
                                vocabulary.Add(newVocabRecord)
                            Next
                            Return vocabulary
                        End Function

                        Public Shared Function AddNewEncoding(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                            Dim NewVocab As New List(Of VocabularyEntry)
                            If CheckVocabulary(Word, Vocab) = False Then
                                NewVocab = Vocab
                                Dim NewItem As New VocabularyEntry
                                NewItem.Text = Word
                                NewItem.Encoding = Vocab.Count
                                Return NewVocab
                            Else
                                Return Vocab
                            End If
                        End Function

                        Public Shared Function CheckVocabulary(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As Boolean
                            Dim Discovered As Boolean = False
                            For Each item In Vocab
                                If item.Text = Word Then
                                    Return True
                                End If
                            Next
                            Return False
                        End Function

                    End Class

                    Public Class Decode

                        Public Shared Function DecodeInteger(ByRef Lookup As Integer, ByRef Vocabulary As List(Of VocabularyEntry)) As String
                            For Each item In Vocabulary
                                If item.Encoding = Lookup Then
                                    Return item.Text
                                End If
                            Next
                            Return "Not found in vocabulary"
                        End Function

                        Public Shared Function DecodeText(ByRef Lookup As String, ByRef Vocabulary As List(Of VocabularyEntry)) As Integer
                            For Each item In Vocabulary
                                If item.Text = Lookup Then
                                    Return item.Encoding
                                End If
                            Next
                            Return "Not found in vocabulary"
                        End Function

                    End Class

                    Public Class VocabularyEntry
                        Public Text As String
                        Public Encoding As Integer
                        Public Frequency As Integer
                        Public Present As Boolean
                        Public SequenceEncoding As Integer
                        Public TF_IDF As Double

                        Public Sub New()

                        End Sub

                        Public Sub New(text As String, sequenceEncoding As Integer, frequency As Integer, tF_IDF As Double)
                            If text Is Nothing Then
                                Throw New ArgumentNullException(NameOf(text))
                            End If

                            Me.Text = text
                            Me.SequenceEncoding = sequenceEncoding
                            Me.Frequency = frequency
                            Me.TF_IDF = tF_IDF
                        End Sub

                    End Class

                    Public Enum VocabularyType
                        Character
                        Word
                        Sentence
                    End Enum

                    Private Shared Function CreateCharVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                        Dim RecordList = CreateUniqueChars(text)

                        Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeChars(RecordList)
                        Return vocabulary
                    End Function

                    Private Shared Function CreateWordVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                        Dim RecordList = CreateUniqueWords(text)

                        Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                        Return vocabulary
                    End Function

                    Private Shared Function CreateSentenceVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                        Dim RecordList = CreateUniqueSentences(text)

                        Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                        Return vocabulary
                    End Function

                    Public Shared Function UpdateVocabulary(ByRef Text As String, ByRef vocab As List(Of VocabularyEntry))
                        Return Encode.AddNewEncoding(Text, vocab)
                    End Function

                    Public Shared Function CreateUniqueSentences(ByRef Text As String) As List(Of String)
                        Dim Words = Tokenizer.TokenizeBySentence(Text)
                        Dim WordList As New List(Of String)
                        For Each item In Words
                            If WordList.Contains(item.Value.ToLower) = False Then
                                WordList.Add(item.Value.ToLower)
                            End If

                        Next

                        Return WordList
                    End Function

                    Public Shared Function CreateUniqueWords(ByRef Text As String) As List(Of String)
                        Dim Words = Tokenizer.TokenizeByWord(Text)
                        Dim WordList As New List(Of String)
                        For Each item In Words
                            If WordList.Contains(item.Value.ToLower) = False Then
                                WordList.Add(item.Value.ToLower)
                            End If

                        Next

                        Return WordList
                    End Function

                    Public Shared Function CreateUniqueChars(ByRef Text As String) As List(Of String)
                        Dim Chars = Tokenizer.TokenizeByCharacter(Text)
                        Dim CharList As New List(Of String)
                        For Each item In Chars
                            If CharList.Contains(item.Value.ToLower) = False Then
                                CharList.Add(item.Value.ToLower)
                            End If

                        Next

                        Return CharList
                    End Function

                    Public Shared Function CreateVocabulary(ByRef Text As String, vType As VocabularyType) As List(Of VocabularyEntry)
                        Select Case vType
                            Case VocabularyType.Character
                                Return CreateCharVocabulary(Text)
                            Case VocabularyType.Word
                                Return CreateWordVocabulary(Text)
                            Case VocabularyType.Sentence
                                Return CreateSentenceVocabulary(Text)
                        End Select
                        Return New List(Of VocabularyEntry)
                    End Function

                End Class

            End Class

            ''' <summary>
            ''' A Data object used to hold 
            ''' specific information about a document or corpus.
            ''' Information is contained in the entry's of the Entry's. 
            ''' Therefore the vocabulary can function as a single object of 
            ''' information by also by using its presence in a document or 
            ''' not as a contextual element such as to detect a topic 
            ''' if it is found to be present it may signify that 
            ''' this topic is indeed present. for other methodology s; 
            ''' It could also denote a sentiment there fore a document 
            ''' could have a collection of vocabulary items as contextual indicators. 
            ''' by summing thier presence (one hot encoding) (Scoring)
            ''' this has (simple Bag of words) and (Complex(ORganized) Bag of Words) usage
            ''' </summary>
            Public Class BagOfWords


                ''' <summary>
                ''' A vocabulary is a bag of words model ; 
                ''' Containing the unique words in a 
                ''' document or corpus of documents 
                ''' As a populated object is can become a 
                ''' data object providing a single dimension of context
                ''' </summary>
                Public Sub New()
                    CurrentVocabulary = New List(Of VocabularyEntry)
                End Sub
                ''' <summary>
                ''' Used to add context awareness for Document
                ''' </summary>
                Public Structure VocabularyContext
                    ''' <summary>
                    ''' Used to encode the Entrys with a context Awareness
                    ''' Holds versions of the current corpus vocabulary with context elements
                    ''' Which can be used to score the relevance of the document
                    ''' if a word in the vocabulary is 
                    ''' present the number of vocabulary terms which are present 
                    ''' can be used to score the relevance of this document
                    ''' providing a unique score especially when combined with other 
                    ''' encoding elements
                    ''' </summary>
                    Public Context As List(Of BagOfWords)
                    ''' <summary>
                    ''' Ie, Sentiment, Emotion, Entity, Topic, Intent , etc
                    ''' </summary>
                    Public ContextType As String
                    ''' <summary>
                    ''' Score of (sum of boolean true for terms in vocabulary present)
                    ''' </summary>
                    Public Score As Integer
                End Structure
                Public Enum VocabularyType
                    Character
                    Word
                    Sentence
                End Enum

                Public Property CurrentVocabulary As List(Of VocabularyEntry)
                Public Property Type As VocabularyType



                ' Enhanced vocabulary management: AddNewEntry function
                Public Shared Function AddNewEntry(ByRef Vocab As List(Of VocabularyEntry), ByVal Text As String) As List(Of VocabularyEntry)
                    If Not Vocab.Any(Function(entry) entry.Text = Text) Then

                        Vocab = Encode.AddNewEncoding(Text, Vocab)
                    Else

                    End If
                    Vocab = VocabularyEntry.UpdateFrequency(Vocab, Text)
                    Return Vocab
                End Function

                Public Shared Function CreateUniqueChars(ByRef Text As String) As List(Of String)
                    Dim Chars = Tokenizer.TokenizeByCharacter(Text)
                    Dim CharList As New List(Of String)
                    For Each item In Chars
                        If CharList.Contains(item.Value.ToLower) = False Then
                            CharList.Add(item.Value.ToLower)
                        End If

                    Next

                    Return CharList
                End Function

                Public Shared Function CreateUniqueSentences(ByRef Text As String) As List(Of String)
                    Dim Words = Tokenizer.TokenizeBySentence(Text)
                    Dim WordList As New List(Of String)
                    For Each item In Words
                        If WordList.Contains(item.Value.ToLower) = False Then
                            WordList.Add(item.Value.ToLower)
                        End If

                    Next

                    Return WordList
                End Function

                Public Shared Function CreateUniqueWords(ByRef Text As String) As List(Of String)
                    Dim Words = Tokenizer.TokenizeByWord(Text)
                    Dim WordList As New List(Of String)
                    For Each item In Words
                        If WordList.Contains(item.Value.ToLower) = False Then
                            WordList.Add(item.Value.ToLower)
                        End If
                    Next
                    Return WordList
                End Function

                Public Shared Function MakeVocabulary(ByRef Text As String, vType As VocabularyType) As List(Of VocabularyEntry)
                    Select Case vType
                        Case VocabularyType.Character
                            Return CreateCharVocabulary(Text)
                        Case VocabularyType.Word
                            Return CreateWordVocabulary(Text)
                        Case VocabularyType.Sentence
                            Return CreateSentenceVocabulary(Text)
                    End Select
                    Return New List(Of VocabularyEntry)
                End Function

                Public Shared Function UpdateVocabulary(ByRef Text As String, ByRef vocab As List(Of VocabularyEntry))
                    Return Encode.AddNewEncoding(Text, vocab)
                End Function

                Public Sub AddNewEntry(ByVal Text As String)
                    If Not CurrentVocabulary.Any(Function(entry) entry.Text = Text) Then

                        CurrentVocabulary = Encode.AddNewEncoding(Text, CurrentVocabulary)
                    Else

                    End If
                    CurrentVocabulary = VocabularyEntry.UpdateFrequency(CurrentVocabulary, Text)

                End Sub

                Public Sub CreateVocabulary(ByRef Text As String, vType As VocabularyType)
                    Select Case vType
                        Case VocabularyType.Character
                            CurrentVocabulary = CreateCharVocabulary(Text)
                            Type = VocabularyType.Character
                        Case VocabularyType.Word
                            CurrentVocabulary = CreateWordVocabulary(Text)
                            Type = VocabularyType.Word
                        Case VocabularyType.Sentence
                            CurrentVocabulary = CreateSentenceVocabulary(Text)
                            Type = VocabularyType.Sentence
                    End Select

                End Sub

                Public Shared Sub Main()

                    'Random Training data generator
                    Dim WordGen As String() = {"technology",
                        "sports", "science", "politics", "entertainment", "animals", "colors"}
                    Dim numTopics As Integer = WordGen.Count
                    Dim numDocuments As Integer = 2

                    ' Create the training set
                    Dim trainingSet As New List(Of Clause)

                    Dim random As New Random()

                    ' For i As Integer = 1 To numDocuments
                    Dim document As New Clause
                    Dim Words = WordGen
                    trainingSet.Add(document)
                    random.NextDouble()
                    ' Next

                    ' Create and train the LDA model
                    Dim lda As New Latent_Dirichlet_Allocation(trainingSet, numTopics)
                    lda.TrainModel(200)

                    ' Print the topics and word distributions
                    lda.PrintTopics()

                    Console.ReadLine()
                    Console.WriteLine()
                    Dim input() As Double = {1.0, 2.0, 3.0}

                    Dim output() As Double = Softmax.Softmax(input)

                    Console.WriteLine("Input: {0}", String.Join(", ", input))
                    Console.WriteLine("Softmax Output: {0}", String.Join(", ", output))
                    Console.ReadLine()
                    Dim xmatrix(,) As Integer = {{1, 2, 3, 9}, {4, 5, 6, 8}, {7, 8, 9, 9}}

                    Dim xresult(,) As Integer = Tril.Tril(xmatrix)

                    Console.WriteLine("Matrix:")
                    Tril.PrintMatrix(xmatrix)

                    Console.WriteLine("Tril Result:")
                    Tril.PrintMatrix(xresult)
                    Console.ReadLine()
                    'Create Vocabulary
                    Dim iCorpus As String = "the quick brown fox, jumped over the lazy dog."
                    Dim NewVocabulary = BagOfWords.MakeVocabulary(iCorpus, BagOfWords.VocabularyType.Character)
                    Console.WriteLine("vocabulary: ")
                    Dim str As String = ""
                    For Each item In NewVocabulary
                        str &= "entry :" & item.Text & vbTab & "Value :" & item.Encoding & vbNewLine

                    Next
                    Console.WriteLine(str)


                    'Encode InputText
                    Dim InputText As String = "Hello World."

                    Dim InputLayer As New InputTextRecord
                    InputLayer.Text = InputText

                    InputLayer.Encoding = Encode.Encode_Text(InputText, NewVocabulary, VocabularyType.Character)
                    Console.WriteLine("Input Text: " & "[" & InputLayer.Text & "]" & vbNewLine)
                    Console.WriteLine("Input Embedding: ")
                    str = "["
                    For Each item In InputLayer.Encoding
                        str &= item & " "
                    Next
                    str &= "] "
                    Console.WriteLine(str)
                    Console.WriteLine(vbNewLine)
                    'get inputs 
                    InputLayer.blocksize = 4
                    InputLayer.Inputblocks = InputTextRecord.GetBlocks(InputLayer.Encoding, InputLayer.blocksize)
                    Console.WriteLine("Input BlockSize: " & InputLayer.blocksize)
                    Console.WriteLine("Input Blocks ")
                    Console.WriteLine()

                    For Each lst In InputLayer.Inputblocks

                        Dim block As String = ""
                        For Each item In lst
                            block &= item & " "
                        Next
                        Console.WriteLine("[" & block & "]")
                    Next
                    Console.WriteLine(vbNewLine)
                    Dim ofset = 1
                    'get targets(add ofset to get targets further in the future   ofset < blocksize)
                    InputLayer.Targetblocks = InputTextRecord.GetBlocks(InputLayer.Encoding, InputLayer.blocksize, ofset)
                    Console.WriteLine("Target BlockSize: " & InputLayer.blocksize)
                    Console.WriteLine("Target ofset    : " & ofset)
                    Console.WriteLine("Target Blocks  ")
                    Console.WriteLine()

                    For Each lst In InputLayer.Targetblocks

                        Dim block As String = ""
                        For Each item In lst
                            block &= item & " "
                        Next
                        Console.WriteLine("[" & block & "]")
                    Next
                    Console.WriteLine()

                    Dim matrix(,) As Integer = ConvertToMatrix(InputLayer.Inputblocks)

                    Dim result2(,) As Integer = Tril.Tril(matrix)

                    Console.WriteLine("input Matrix:")
                    Tril.PrintMatrix(matrix)
                    Console.WriteLine()
                    Console.WriteLine("Apply Mask Attention Result:Inputs")
                    Tril.PrintMatrix(result2)
                    Console.WriteLine()
                    Console.WriteLine("SoftMax Result:Inputs")
                    Tril.PrintMatrix(Softmax.Softmax(result2))
                    Console.WriteLine()
                    Dim matrix2(,) As Integer = ConvertToMatrix(InputLayer.Targetblocks)

                    Dim result3(,) As Integer = Tril.Tril(matrix2)

                    Console.WriteLine("Target Matrix:")
                    Tril.PrintMatrix(matrix2)
                    Console.WriteLine()
                    Console.WriteLine("Masked Attention Result:Targets")
                    Tril.PrintMatrix(result3)
                    Console.WriteLine()
                    Console.WriteLine("SoftMax Result:Targets")
                    Tril.PrintMatrix(Softmax.Softmax(result3))
                    Console.WriteLine()
                    Console.ReadLine()


                End Sub
                Public Shared Function ConvertToMatrix(inputBlocks As List(Of List(Of Integer))) As Integer(,)
                    Dim numRows As Integer = inputBlocks.Count
                    Dim numColumns As Integer = inputBlocks(0).Count

                    Dim matrix(numRows - 1, numColumns - 1) As Integer

                    For i As Integer = 0 To inputBlocks.Count - 1
                        For j As Integer = 0 To inputBlocks(i).Count - 1
                            matrix(i, j) = inputBlocks(i)(j)
                        Next
                    Next

                    Return matrix
                End Function
                Public Shared Function ConvertToMatrix(inputBlocks As List(Of List(Of Integer)), targetBlocks As List(Of List(Of Integer))) As Integer(,)
                    Dim numRows As Integer = Math.Max(inputBlocks.Count, targetBlocks.Count)
                    Dim numColumns As Integer = Math.Max(GetMaxListSize(inputBlocks), GetMaxListSize(targetBlocks))

                    Dim matrix(numRows - 1, numColumns - 1) As Integer

                    For i As Integer = 0 To inputBlocks.Count - 1
                        For j As Integer = 0 To inputBlocks(i).Count - 1
                            matrix(i, j) = inputBlocks(i)(j)
                        Next
                    Next

                    For i As Integer = 0 To targetBlocks.Count - 1
                        For j As Integer = 0 To targetBlocks(i).Count - 1
                            matrix(i, j + inputBlocks(i).Count) = targetBlocks(i)(j)
                        Next
                    Next

                    Return matrix
                End Function

                Private Shared Function GetMaxListSize(lists As List(Of List(Of Integer))) As Integer
                    Dim maxSize As Integer = 0
                    For Each innerList In lists
                        maxSize = Math.Max(maxSize, innerList.Count)
                    Next
                    Return maxSize
                End Function
                Public Sub UpdateVocabulary(ByRef Text As String)
                    CurrentVocabulary = Encode.AddNewEncoding(Text, CurrentVocabulary)
                End Sub

                Private Shared Function CreateCharVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                    Dim RecordList = CreateUniqueChars(text)

                    Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeChars(RecordList)
                    Return vocabulary
                End Function

                Private Shared Function CreateSentenceVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                    Dim RecordList = CreateUniqueSentences(text)

                    Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                    Return vocabulary
                End Function

                Private Shared Function CreateWordVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                    Dim RecordList = CreateUniqueWords(text)

                    Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                    Return vocabulary
                End Function

                Public Structure InputTextRecord
                    Public blocksize As Integer
                    Public Encoding As List(Of Integer)
                    Public Inputblocks As List(Of List(Of Integer))
                    Public Targetblocks As List(Of List(Of Integer))
                    Public Text As String
                    Public Shared Function GetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer, Optional Ofset As Integer = 0) As List(Of List(Of Integer))
                        Dim pos As Integer = 0
                        Dim newPos As Integer = Size
                        Dim blocks As New List(Of List(Of Integer))
                        Dim block As New List(Of Integer)
                        Do While pos < Embedding.Count - 1
                            For i = pos To newPos - 1
                                If Ofset > 0 Then
                                    If i + Ofset < Embedding.Count - 1 Then

                                        block.Add(Embedding(i + Ofset))
                                        'block.Add(Embedding(i))
                                    Else
                                        block.Add(Embedding(i))
                                    End If
                                Else
                                    block.Add(Embedding(i))
                                End If



                            Next
                            blocks.Add(block)
                            block = New List(Of Integer)
                            pos = newPos

                            If newPos + Size < Embedding.Count - 1 Then
                                newPos += Size
                            Else
                                newPos = Embedding.Count
                            End If

                        Loop

                        Return blocks
                    End Function
                    Public Shared Function GetTargetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer) As List(Of List(Of Integer))
                        Dim pos As Integer = 0
                        Dim newPos As Integer = Size
                        Dim blocks As New List(Of List(Of Integer))
                        Dim block As New List(Of Integer)
                        Do While pos < Embedding.Count - 1
                            For i = pos To newPos - 1
                                block.Add(Embedding(i))

                            Next
                            blocks.Add(block)
                            block = New List(Of Integer)
                            pos = newPos
                            If newPos + Size < Embedding.Count - 1 Then
                                newPos += Size
                            Else
                                newPos = Embedding.Count
                            End If

                        Loop

                        Return blocks
                    End Function
                End Structure

                Public Structure VocabularyEntry
                    Public Encoding As Integer
                    Public Frequency As Integer
                    ''' <summary>
                    ''' Dimensions of the model(used to calculate dimesional positioning)
                    ''' </summary>
                    Public Model_Dimensions As Integer

                    Public Text As String
                    Private iDocumentPresence As Boolean
                    Public ReadOnly Property IsPresentInDocument As Boolean
                        Get
                            Return iDocumentPresence
                        End Get
                    End Property
                    Public ReadOnly Property PositionalEncoding As Double()
                        Get
                            Return CalcPositionalEncoding(Model_Dimensions)
                        End Get
                    End Property
                    Public Shared Function CalcPositionalEncoding(ByVal position As Integer, ByVal d_model As Integer) As Double()
                        ' Create an empty array to store the encoding
                        Dim encoding(d_model - 1) As Double

                        ' Loop through each dimension of the model and calculate the encoding value
                        For i As Integer = 0 To d_model - 1
                            If i Mod 2 = 0 Then
                                encoding(i) = Math.Sin(position / (10000 ^ (i / d_model)))
                            Else
                                encoding(i) = Math.Cos(position / (10000 ^ ((i - 1) / d_model)))
                            End If
                        Next

                        ' Return the encoding array
                        Return encoding
                    End Function

                    ' Enhanced vocabulary management: RemoveEntry function
                    Public Shared Function RemoveEntry(ByRef Vocab As List(Of VocabularyEntry), ByVal Text As String) As List(Of VocabularyEntry)
                        Vocab.RemoveAll(Function(entry) entry.Text = Text)
                        Return Vocab
                    End Function

                    ' Enhanced vocabulary management: UpdateFrequency function
                    Public Shared Function UpdateFrequency(ByRef Vocab As List(Of VocabularyEntry), ByVal Text As String) As List(Of VocabularyEntry)
                        Dim entry = Vocab.FirstOrDefault(Function(item) item.Text = Text)
                        If entry.Text IsNot Nothing Then
                            entry.Frequency += 1
                        End If
                        Return Vocab
                    End Function

                    ''' <summary>
                    ''' Removes this Entry from the lang model
                    ''' </summary>
                    ''' <param name="Vocab"> lang model</param>
                    ''' <returns>updated lang model</returns>
                    Public Function RemoveEntry(ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                        Dim QueryText As String = Text
                        Vocab.RemoveAll(Function(entry) entry.Text = QueryText)
                        Return Vocab
                    End Function

                    ''' <summary>
                    ''' Used to set a binary encoding for document presence 
                    ''' Given the vocabulary for the 
                    ''' Document the vocabulary is returned updated as not present
                    ''' </summary>
                    ''' <param name="Vocab"></param>
                    ''' <returns></returns>
                    Public Function SetIsNotPresent(ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                        Dim QueryText As String = Text
                        Dim entry = Vocab.FirstOrDefault(Function(item) item.Text = QueryText)
                        If entry.Text IsNot Nothing Then
                            entry.iDocumentPresence = False
                        End If
                        Return Vocab
                    End Function

                    ''' <summary>
                    ''' Usde to set binary encoding based on this 
                    ''' entrys presence in the document given the current documents vocabulary
                    ''' </summary>
                    ''' <param name="Vocab"></param>
                    ''' <returns></returns>
                    Public Function SetIsPresent(ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                        Dim QueryText As String = Text
                        Dim entry = Vocab.FirstOrDefault(Function(item) item.Text = QueryText)
                        If entry.Text IsNot Nothing Then
                            entry.iDocumentPresence = True
                        End If
                        Return Vocab
                    End Function

                    ''' <summary>
                    ''' Updates this items frequency in the Lang Model 
                    ''' </summary>
                    ''' <param name="Vocab">Lang Model</param>
                    ''' <returns>updated Lang model</returns>
                    Public Function UpdateFrequency(ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                        Dim QueryText As String = Text
                        Dim entry = Vocab.FirstOrDefault(Function(item) item.Text = QueryText)
                        If entry.Text IsNot Nothing Then
                            entry.Frequency += 1
                        End If
                        Return Vocab
                    End Function

                    Private Function CalcPositionalEncoding(ByVal d_model As Integer) As Double()
                        ' Create an empty array to store the encoding
                        Dim encoding(d_model - 1) As Double

                        ' Loop through each dimension of the model and calculate the encoding value
                        For i As Integer = 0 To d_model - 1
                            If i Mod 2 = 0 Then
                                encoding(i) = Math.Sin(Me.Encoding / (10000 ^ (i / d_model)))
                            Else
                                encoding(i) = Math.Cos(Me.Encoding / (10000 ^ ((i - 1) / d_model)))
                            End If
                        Next

                        ' Return the encoding array
                        Return encoding
                    End Function
                End Structure
                Public Class Decode
                    Public Shared Function DecodeInteger(ByRef Lookup As Integer, ByRef Vocabulary As List(Of VocabularyEntry))
                        For Each item In Vocabulary
                            If item.Encoding = Lookup Then
                                Return item.Text
                            End If
                        Next
                        Return "Not found in vocabulary"
                    End Function
                    Public Shared Function DecodeText(ByRef Lookup As String, ByRef Vocabulary As List(Of VocabularyEntry))
                        For Each item In Vocabulary
                            If item.Text = Lookup Then
                                Return item.Encoding
                            End If
                        Next
                        Return "Not found in vocabulary"
                    End Function
                End Class

                Public Class Encode
                    Public Shared Function AddNewEncoding(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                        Dim NewVocab As New List(Of VocabularyEntry)
                        If CheckVocabulary(Word, Vocab) = False Then
                            NewVocab = Vocab
                            Dim NewItem As New VocabularyEntry
                            NewItem.Text = Word
                            NewItem.Encoding = Vocab.Count
                            Return NewVocab
                        Else
                            Return Vocab
                        End If
                    End Function

                    Public Shared Function CheckVocabulary(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As Boolean
                        Dim Discovered As Boolean = False
                        For Each item In Vocab
                            If item.Text = Word Then
                                Return True
                            End If
                        Next
                        Return False
                    End Function

                    Public Shared Function Encode_Text(ByRef Text As String, ByRef Vocab As List(Of VocabularyEntry), ByRef Type As VocabularyType) As List(Of Integer)
                        Dim iOutput As New List(Of Integer)
                        Select Case Type
                            Case VocabularyType.Character
                                Dim Chars = Tokenizer.TokenizeByCharacter(Text)

                                For Each item In Chars
                                    If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                        iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                    End If
                                Next
                            Case VocabularyType.Word
                                Dim Words = Tokenizer.TokenizeByWord(Text)

                                For Each item In Words
                                    If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                        iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                    End If
                                Next
                            Case VocabularyType.Sentence
                                Dim Sents = Tokenizer.TokenizeBySentence(Text)

                                For Each item In Sents
                                    If CheckVocabulary(item.Value, Vocab) = True Then
                                        iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                    End If
                                Next
                        End Select
                        Return iOutput
                    End Function
                    Public Shared Function EncodeChars(VocabList As List(Of String)) As List(Of VocabularyEntry)
                        Dim vocabulary As New List(Of VocabularyEntry)
                        Dim EncodingValue As Integer = 1
                        For Each item In VocabList
                            Dim newVocabRecord As New VocabularyEntry
                            newVocabRecord.Encoding = EncodingValue
                            newVocabRecord.Text = item
                            EncodingValue += 1
                            vocabulary.Add(newVocabRecord)
                        Next
                        Return vocabulary
                    End Function
                    Public Shared Function EncodeWords(VocabList As List(Of String)) As List(Of VocabularyEntry)
                        Dim vocabulary As New List(Of VocabularyEntry)
                        Dim EncodingValue As Integer = 1
                        For Each item In VocabList
                            Dim newVocabRecord As New VocabularyEntry
                            newVocabRecord.Encoding = EncodingValue
                            newVocabRecord.Text = item
                            EncodingValue += 1
                            vocabulary.Add(newVocabRecord)
                        Next
                        Return vocabulary
                    End Function
                End Class
            End Class

            ''' <summary>
            ''' An Encoded Language model , 
            ''' With an internal vocabulary for Basic Encoding/Decoding
            ''' </summary>
            Public Class iLangModel
                Private LanguageModel As BaseModels.LanguageModelFactory.NgramModels.NgramLanguageModel
                Private Attend As FeedForwardNetwork
                Private csize As Integer
                Public Structure Vocabulary
                    Private iValues As List(Of Token)
                    Public ReadOnly Property Values As List(Of Token)
                        Get
                            Return iValues
                        End Get
                    End Property
                    Public Structure Token
                        Public Text
                        Public Vocabulary_ID As Integer
                        Public Encoding As Integer
                    End Structure
                    Private iVocabList As List(Of String)
                    Public ReadOnly Property VocabList As List(Of String)
                        Get
                            Return iVocabList
                        End Get
                    End Property

                    Public Function ADD_NEW(ByRef Token As String) As Boolean
                        If CheckExists(Token) = False Then
                            Dim NewTok As New Token
                            NewTok.Text = Token
                            NewTok.Vocabulary_ID = VocabList.Count + 1
                            iValues.Add(NewTok)
                            Return True
                        Else
                            Return False
                        End If
                    End Function
                    Public Function ADD_NEW(ByRef Token As String, Encoding As Integer) As Boolean
                        If CheckExists(Token) = False Then
                            Dim NewTok As New Token
                            NewTok.Text = Token
                            NewTok.Vocabulary_ID = VocabList.Count + 1
                            NewTok.Encoding = Encoding
                            iValues.Add(NewTok)
                            Return True
                        Else
                            Return False
                        End If
                    End Function
                    Public Function LOOKUP(ByRef Query As Integer) As String

                        If CheckExists(Query) = True Then Return VocabList(Query)


                        Return "Not Found"
                    End Function
                    Public Function CheckExists(ByRef Query As String) As Boolean
                        Return VocabList.Contains(Query)
                    End Function
                    Private Function CheckExists(ByRef Query As Integer) As Boolean
                        Return VocabList.Count < Query
                    End Function
                End Structure
                Private iVocabulary As Vocabulary
                Public ReadOnly Property EncodingVocabulary As Vocabulary
                    Get
                        Return iVocabulary
                    End Get
                End Property

                ''' <summary>
                ''' Create New Model
                ''' </summary>
                Public Sub New()
                    csize = 1
                    iVocabulary = New Vocabulary
                    LanguageModel = New BaseModels.LanguageModelFactory.NgramModels.NgramLanguageModel(2)
                    Attend = New FeedForwardNetwork(csize, 8, 1)

                End Sub
                ''' <summary>
                ''' Can be set with a known vocabulary
                ''' </summary>
                ''' <param name="iVocabulary"></param>
                Public Sub New(iVocabulary As Vocabulary)
                    Me.iVocabulary = iVocabulary
                End Sub
                ''' <summary>
                ''' This input is encoded as a single value, 
                ''' So Char by Char , Word by Word , 
                ''' Sent by Sent is decided outside the object
                ''' </summary>
                ''' <param name="uInputWord"></param>
                ''' <returns></returns>
                Public Function EncodeInput(ByRef uInputWord As String) As Integer


                    If EncodingVocabulary.CheckExists(uInputWord) = False Then
                        LanguageModel.AddDocument(uInputWord)
                        iVocabulary.ADD_NEW(uInputWord, LanguageModel.LookupNgram(uInputWord))
                        Return iVocabulary.LOOKUP(uInputWord)
                    Else
                        Return iVocabulary.LOOKUP(uInputWord)
                    End If

                End Function
                ''' <summary>
                ''' look up the value of the token provided 
                ''' </summary>
                ''' <param name="Query"></param>
                ''' <returns></returns>
                Public Function DecodeInput(ByRef Query As Integer) As String
                    Return iVocabulary.LOOKUP(Query)
                End Function
                Public Function forward(inputSequence As List(Of List(Of Double))) As List(Of List(Of Double))
                    'Here we want to see what the output is without positional encoding
                    Return ApplyFeedForwardNN(ApplyMuliHeadedAttention(inputSequence, 3, inputSequence.Count))
                End Function
                Public Sub Train(inputs As List(Of List(Of Double)), targets As List(Of List(Of Double)), epochs As Integer, learningRate As Double)
                    csize = inputs.ElementAt(0).Count
                    Attend.Train(ApplyMuliHeadedAttention(inputs, 3, inputs.Count), targets, epochs, learningRate)
                End Sub
                Public Function ApplyMuliHeadedAttention(inputSequence As List(Of List(Of Double)), numHeads As Integer, headSize As Integer, Optional Masked As Boolean = False) As List(Of List(Of Double))
                    Dim Attend As New MultiHeadedAttention(numHeads, headSize)
                    Return Attend.Forward(inputSequence, Masked)
                End Function
                Public Function PredictNext_LangModel(ByRef Userinput As String) As String
                    'Add Dynamic to corpus
                    Dim words = Split(Userinput, " ").ToList
                    For Each word In words
                        EncodeInput(word)
                    Next
                    'Load Question into vocabulary(as Question)
                    EncodeInput(Userinput)
                    'Return Prediction Word Or Sentence?
                    Return LanguageModel.PredictNextWord(Userinput)
                End Function
                Public Function PredictNext_Transformer(ByRef Userinput As String) As String
                    EncodeInput(Userinput)
                    Dim words = Split(Userinput, " ").ToList
                    'Load Positional Encodings
                    Dim Encoder As New PositionalEncoding(8, 8, iVocabulary.VocabList)
                    Dim InputSequence = Encoder.Encode(words)
                    'Transform
                    Dim Ouput = ApplyFeedForwardNN(ApplyMuliHeadedAttention(InputSequence, 3, InputSequence.Count))
                    'decode Positions
                    Dim decoder As New PositionalDecoder(8, 8, iVocabulary.VocabList)
                    'Build Decoded Output 
                    Dim str As String = ""
                    For Each item In decoder.Decode(Ouput)
                        str &= item & " "
                    Next
                    Return str

                End Function
                Public Function ApplyFeedForwardNN(inputSequence As List(Of List(Of Double))) As List(Of List(Of Double))

                    csize = inputSequence.ElementAt(0).Count

                    Return Attend.Forward(inputSequence)
                End Function
                Public Shared Function FlattenList(lst As List(Of List(Of Double))) As List(Of Integer)
                    Dim iFlat As New List(Of Integer)
                    For Each i In lst
                        For Each item In i
                            iFlat.Add(item)
                        Next
                    Next
                    Return iFlat
                End Function
                Private Class MultiHeadedAttention
                    Private Shared irand As Random = New Random()
                    Private ReadOnly headSize As Integer
                    Private ReadOnly numHeads As Integer

                    Public Sub New(numHeads As Integer, headSize As Integer)
                        Me.numHeads = numHeads
                        Me.headSize = headSize
                        Randomize()

                    End Sub
                    Private Shared Function GetRandomWeight() As Double

                        Return irand.NextDouble()
                    End Function
                    Private Shared Function InitializeWeights(rows As Integer, cols As Integer) As List(Of List(Of Double))
                        Dim weights As List(Of List(Of Double)) = New List(Of List(Of Double))
                        irand.NextDouble()

                        For i As Integer = 0 To rows - 1
                            Dim rowWeights As List(Of Double) = New List(Of Double)()
                            irand.NextDouble()
                            For j As Integer = 0 To cols - 1
                                rowWeights.Add(GetRandomWeight)
                            Next

                            weights.Add(rowWeights)
                        Next

                        Return weights
                    End Function

                    Private Function iMaskedAttention(query As List(Of List(Of Double)), key As List(Of List(Of Double)), value As List(Of List(Of Double))) As List(Of List(Of Double))
                        Dim attendedFeatures As List(Of List(Of Double)) = New List(Of List(Of Double))

                        For Each queryVector As List(Of Double) In query
                            Dim weightedValues As List(Of Double) = New List(Of Double)

                            For Each keyVector As List(Of Double) In key
                                Dim attentionValue As Double = 0.0

                                For i As Integer = 0 To headSize - 1
                                    attentionValue += queryVector(i) * keyVector(i)
                                Next

                                ' Apply masking by setting attention value to 0 for padding vectors
                                If keyVector.All(Function(x) x = 0) Then
                                    attentionValue = 0.0
                                End If

                                weightedValues.Add(attentionValue)
                            Next

                            attendedFeatures.Add(weightedValues)
                        Next

                        Return attendedFeatures
                    End Function
                    Private Function iAttention(query As List(Of List(Of Double)), key As List(Of List(Of Double)), value As List(Of List(Of Double))) As List(Of List(Of Double))
                        Dim attendedFeatures As List(Of List(Of Double)) = New List(Of List(Of Double))

                        For Each queryVector As List(Of Double) In query
                            Dim weightedValues As List(Of Double) = New List(Of Double)

                            For Each keyVector As List(Of Double) In key
                                Dim attentionValue As Double = 0.0

                                For i As Integer = 0 To headSize - 1
                                    attentionValue += queryVector(i) * keyVector(i)
                                Next

                                weightedValues.Add(attentionValue)
                            Next

                            attendedFeatures.Add(weightedValues)
                        Next

                        Return attendedFeatures
                    End Function
                    Public Function LinearTransformation(inputSequence As List(Of List(Of Double))) As List(Of List(Of Double))
                        Dim transformedSequence As List(Of List(Of Double)) = New List(Of List(Of Double))
                        irand.NextDouble()
                        Dim outputWeight As List(Of List(Of Double)) = InitializeWeights(numHeads * headSize, headSize)

                        For Each vector As List(Of Double) In inputSequence
                            Dim transformedVector As List(Of Double) = New List(Of Double)()

                            For j As Integer = 0 To headSize - 1
                                Dim transformedValue As Double = 0.0

                                For k As Integer = 0 To numHeads - 1
                                    transformedValue += vector(j + k * headSize) * outputWeight(j + k * headSize)(j)
                                Next

                                transformedVector.Add(transformedValue)
                            Next

                            transformedSequence.Add(transformedVector)
                        Next

                        Return transformedSequence
                    End Function
                    Public Function SplitByHead(inputSequence As List(Of List(Of Double)), numHeads As Integer) As List(Of List(Of List(Of Double)))
                        Dim splitInput As List(Of List(Of List(Of Double))) = New List(Of List(Of List(Of Double)))(numHeads)

                        For i As Integer = 0 To numHeads - 1
                            Dim headSequence As List(Of List(Of Double)) = New List(Of List(Of Double))()

                            For Each vector As List(Of Double) In inputSequence
                                Dim headVector As List(Of Double) = vector.GetRange(i * headSize, headSize)
                                headSequence.Add(headVector)
                            Next

                            splitInput.Add(headSequence)
                        Next

                        Return splitInput
                    End Function
                    Public Function ConcatenateHeads(headOutputs As List(Of List(Of List(Of Double)))) As List(Of List(Of Double))
                        Dim concatenatedOutput As List(Of List(Of Double)) = New List(Of List(Of Double))()

                        For i As Integer = 0 To headOutputs(0).Count - 1
                            Dim concatenatedVector As List(Of Double) = New List(Of Double)()

                            For Each headOutput As List(Of List(Of Double)) In headOutputs
                                concatenatedVector.AddRange(headOutput(i))
                            Next

                            concatenatedOutput.Add(concatenatedVector)
                        Next

                        Return concatenatedOutput
                    End Function
                    Public Function Transform(query As List(Of List(Of Double)), key As List(Of List(Of Double)), value As List(Of List(Of Double)), Optional useMaskedAttention As Boolean = False) As List(Of List(Of Double))
                        ' Split the query, key, and value into multiple heads
                        Dim splitQuery = SplitByHead(query, numHeads)
                        Dim splitKey = SplitByHead(key, numHeads)
                        Dim splitValue = SplitByHead(value, numHeads)

                        ' Apply attention mechanism for each head
                        Dim headOutputs As List(Of List(Of List(Of Double))) = New List(Of List(Of List(Of Double)))(numHeads)
                        For i As Integer = 0 To numHeads - 1
                            Dim q As List(Of List(Of Double)) = splitQuery(i)
                            Dim k As List(Of List(Of Double)) = splitKey(i)
                            Dim v As List(Of List(Of Double)) = splitValue(i)

                            Dim headOutput As List(Of List(Of Double))
                            If useMaskedAttention Then
                                headOutput = iMaskedAttention(q, k, v)
                            Else
                                headOutput = iAttention(q, k, v)
                            End If

                            headOutputs.Add(headOutput)
                        Next

                        ' Concatenate the head outputs
                        Dim concatenatedOutput As List(Of List(Of Double)) = ConcatenateHeads(headOutputs)

                        ' Apply linear transformation
                        Dim output As List(Of List(Of Double)) = LinearTransformation(concatenatedOutput)

                        Return output
                    End Function


                    Public Function Attention(inputSequence As List(Of List(Of Double)), inputVector As List(Of Double)) As List(Of Double)
                        Dim weightedValues As List(Of Double) = New List(Of Double)()

                        For Each sequenceVector As List(Of Double) In inputSequence
                            Dim attentionValue As Double = 0.0

                            For i As Integer = 0 To headSize - 1
                                attentionValue += inputVector(i) * sequenceVector(i)
                            Next

                            weightedValues.Add(attentionValue)
                        Next

                        Return weightedValues
                    End Function
                    Public Function MaskedAttention(inputSequence As List(Of List(Of Double)), inputVector As List(Of Double)) As List(Of Double)
                        Dim weightedValues As List(Of Double) = New List(Of Double)

                        For Each sequenceVector As List(Of Double) In inputSequence
                            Dim attentionValue As Double = 1

                            For i As Integer = 0 To headSize - 1
                                attentionValue += inputVector(i) * sequenceVector(i)
                            Next

                            ' Apply masking by setting attention value to 0 for padding vectors
                            If sequenceVector.All(Function(x) x = 0) Then
                                attentionValue = 0
                            End If

                            weightedValues.Add(attentionValue)
                        Next

                        Return weightedValues
                    End Function
                    Public Function Forward(inputSequence As List(Of List(Of Double)), Optional useMaskedAttention As Boolean = False) As List(Of List(Of Double))
                        Dim attendedFeatures As List(Of List(Of Double)) = New List(Of List(Of Double))()

                        For Each inputVector As List(Of Double) In inputSequence
                            Dim attendedVector As List(Of Double)
                            If useMaskedAttention Then
                                attendedVector = MaskedAttention(inputSequence, inputVector)
                            Else
                                attendedVector = Attention(inputSequence, inputVector)
                            End If

                            attendedFeatures.Add(attendedVector)
                        Next

                        Return attendedFeatures
                    End Function



                End Class
                Private Class FeedForwardNetwork
                    Public Enum Activation
                        ReLU
                        Sigmoid
                        Tanh
                    End Enum
                    Private ReadOnly hiddenSize As Integer
                    Private ReadOnly hiddenWeights As List(Of List(Of Double))
                    Private ReadOnly inputSize As Integer
                    Private ReadOnly layerNorm1 As LayerNormalization
                    Private ReadOnly layerNorm2 As LayerNormalization
                    Private ReadOnly outputSize As Integer
                    Private ReadOnly outputWeights As List(Of List(Of Double))

                    Private rand As Random = New Random()
                    Private outputGradients As List(Of List(Of Double))
                    Private hiddenGradients As List(Of List(Of Double))
                    Public Sub New(inputSize As Integer, hiddenSize As Integer, outputSize As Integer)
                        Me.inputSize = inputSize
                        Me.hiddenSize = hiddenSize
                        Me.outputSize = outputSize
                        Randomize()
                        Me.hiddenWeights = InitializeWeights(inputSize, hiddenSize)
                        Me.outputWeights = InitializeWeights(hiddenSize, outputSize)

                        ' Initialize layer normalization objects
                        Me.layerNorm1 = New LayerNormalization(hiddenSize)
                        Me.layerNorm2 = New LayerNormalization(outputSize)

                        ' Initialize positional encoding object
                        '   Me.positionalEncoder = New PositionalEncoderFF(hiddenSize)
                        outputGradients = New List(Of List(Of Double))
                        hiddenGradients = New List(Of List(Of Double))
                    End Sub


                    ''' <summary>
                    ''' Trains the feed-forward neural network using gradient descent optimization.
                    ''' </summary>
                    ''' <param name="inputs">The input training data.</param>
                    ''' <param name="targets">The target training data.</param>
                    ''' <param name="epochs">The number of training epochs.</param>
                    ''' <param name="learningRate">The learning rate for gradient descent.</param>
                    Public Sub Train(inputs As List(Of List(Of Double)), targets As List(Of List(Of Double)), epochs As Integer, learningRate As Double)
                        For epoch As Integer = 1 To epochs
                            Dim lossSum As Double = 0.0

                            For i As Integer = 0 To inputs.Count - 1
                                Dim inputVector As List(Of Double) = inputs(i)
                                Dim targetVector As List(Of Double) = targets(i)

                                ' Forward pass to compute the predicted output
                                Dim outputVector As List(Of Double) = Forward(inputs)(i)

                                ' Compute the loss (e.g., mean squared error)
                                Dim loss As Double = ComputeLoss(outputVector, targetVector)
                                lossSum += loss

                                ' Backpropagation to compute gradients
                                Backpropagation(inputVector, outputVector, targetVector)

                                ' Update the weights using gradient descent
                                UpdateWeights(learningRate)
                            Next

                            ' Compute the average loss for the epoch
                            Dim averageLoss As Double = lossSum / inputs.Count

                            ' Print the average loss for monitoring
                            Console.WriteLine("Epoch {0}: Average Loss = {1}", epoch, averageLoss)
                        Next
                    End Sub
                    ''' <summary>
                    ''' Computes the loss between the predicted output and the target output.
                    ''' </summary>
                    ''' <param name="outputVector">The predicted output vector.</param>
                    ''' <param name="targetVector">The target output vector.</param>
                    ''' <returns>The loss value.</returns>
                    Private Function ComputeLoss(outputVector As List(Of Double), targetVector As List(Of Double)) As Double
                        Dim loss As Double = 0.0
                        Dim n As Integer = outputVector.Count

                        For i As Integer = 0 To n - 1
                            loss += (outputVector(i) - targetVector(i)) ^ 2
                        Next

                        loss /= n

                        Return loss
                    End Function

                    ''' <summary>
                    ''' Performs backpropagation to compute the gradients.
                    ''' </summary>
                    ''' <param name="inputVector">The input vector.</param>
                    ''' <param name="outputVector">The predicted output vector.</param>
                    ''' <param name="targetVector">The target output vector.</param>
                    Private Sub Backpropagation(inputVector As List(Of Double), outputVector As List(Of Double), targetVector As List(Of Double))
                        ' Compute the gradient of the output layer
                        outputGradients = New List(Of List(Of Double))()
                        Dim outputDelta As List(Of Double) = New List(Of Double)()

                        For i As Integer = 0 To outputSize - 1
                            Dim derivative As Double = outputVector(i) - targetVector(i)
                            outputDelta.Add(derivative)
                        Next

                        ' Compute the gradient of the hidden layer
                        hiddenGradients = New List(Of List(Of Double))()
                        Dim hiddenDelta As List(Of Double) = New List(Of Double)()

                        For i As Integer = 0 To hiddenSize - 1
                            Dim derivative As Double = HiddenActivationDerivative(inputVector, i) * WeightedSum(outputDelta, outputWeights, i)
                            hiddenDelta.Add(derivative)
                        Next

                        outputGradients.Add(outputDelta)
                        hiddenGradients.Add(hiddenDelta)
                    End Sub
                    ''' <summary>
                    ''' Computes the weighted sum of the inputs using the specified weights and index.
                    ''' </summary>
                    ''' <param name="inputs">The input vector.</param>
                    ''' <param name="weights">The weight matrix.</param>
                    ''' <param name="index">The index of the neuron.</param>
                    ''' <returns>The weighted sum.</returns>
                    Private Function WeightedSum(inputs As List(Of Double), weights As List(Of List(Of Double)), index As Integer) As Double
                        Dim sum As Double = 0.0

                        For i As Integer = 0 To inputs.Count - 1
                            sum += inputs(i) * weights(i)(index)
                        Next

                        Return sum
                    End Function

                    ''' <summary>
                    ''' Updates the weights of the neural network using gradient descent.
                    ''' </summary>
                    ''' <param name="learningRate">The learning rate for gradient descent.</param>
                    Private Sub UpdateWeights(learningRate As Double)
                        ' Update the weights between the hidden and output layers
                        For i As Integer = 0 To hiddenSize - 1
                            For j As Integer = 0 To outputSize - 1
                                Dim weightChange As Double = -learningRate * outputGradients(0)(j)
                                outputWeights(i)(j) += weightChange
                            Next
                        Next

                        ' Update the weights between the input and hidden layers
                        For i As Integer = 0 To inputSize - 1
                            For j As Integer = 0 To hiddenSize - 1
                                Dim weightChange As Double = -learningRate * hiddenGradients(0)(j)
                                hiddenWeights(i)(j) += weightChange
                            Next
                        Next
                    End Sub

                    ''' <summary>
                    ''' Computes the derivative of the activation function used in the hidden layer.
                    ''' </summary>
                    ''' <param name="inputVector">The input vector.</param>
                    ''' <param name="index">The index of the neuron.</param>
                    ''' <returns>The derivative value.</returns>
                    Private Function HiddenActivationDerivative(inputVector As List(Of Double), index As Integer) As Double
                        Dim sum As Double = 0.0

                        For i As Integer = 0 To inputSize - 1
                            sum += inputVector(i) * hiddenWeights(i)(index)
                        Next

                        Dim output As Double = HiddenActivation(sum)
                        Return output * (1 - output)
                    End Function
                    ''' <summary>
                    ''' Applies the activation function to the hidden layer outputs.
                    ''' </summary>
                    ''' <param name="input">The input value.</param>
                    ''' <returns>The activated value.</returns>
                    Private Function HiddenActivation(input As Double) As Double
                        ' Use the sigmoid function as the activation function for the hidden layer
                        Return 1.0 / (1.0 + Math.Exp(-input))
                    End Function
                    ''' <summary>
                    ''' Applies the activation function to the output layer outputs.
                    ''' </summary>
                    ''' <param name="input">The input value.</param>
                    ''' <returns>The activated value.</returns>
                    Private Function OutputActivation(input As Double) As Double
                        ' Use the identity function as the activation function for the output layer
                        Return input
                    End Function

                    Private Shared Function TrainTest() As FeedForwardNetwork
                        ' Create the input and target training data
                        Dim inputs As New List(Of List(Of Double))()
                        Dim targets As New List(Of List(Of Double))()

                        ' AND logic gate training data
                        inputs.Add(New List(Of Double)() From {0, 0})
                        inputs.Add(New List(Of Double)() From {0, 1})
                        inputs.Add(New List(Of Double)() From {1, 0})
                        inputs.Add(New List(Of Double)() From {1, 1})

                        targets.Add(New List(Of Double)() From {0})
                        targets.Add(New List(Of Double)() From {0})
                        targets.Add(New List(Of Double)() From {0})
                        targets.Add(New List(Of Double)() From {1})

                        ' Create a feed-forward neural network with 2 input neurons, 2 hidden neurons, and 1 output neuron
                        Dim network As New FeedForwardNetwork(inputSize:=2, hiddenSize:=2, outputSize:=1)

                        ' Train the network using the training data for 100 epochs with a learning rate of 0.1
                        network.Train(inputs, targets, epochs:=100, learningRate:=0.1)

                        ' Test the trained network
                        Console.WriteLine("Testing the trained network:")

                        For i As Integer = 0 To inputs.Count - 1
                            Dim inputVector As List(Of Double) = inputs(i)
                            Dim targetVector As List(Of Double) = targets(i)

                            Dim outputVector = network.Forward(inputs)

                            Console.WriteLine("Input: {0}, Target: {1}, Output: {2}", String.Join(", ", inputVector), String.Join(", ", targetVector), String.Join(", ", outputVector))
                        Next

                        Return network
                    End Function
                    Public Shared Sub Main()
                        ' Create an instance of the FeedForwardNetwork
                        Dim feedForwardNN As FeedForwardNetwork = TrainTest()

                        ' Define the input sequence for the logical AND operation
                        Dim inputSequence As List(Of List(Of Double)) = New List(Of List(Of Double))() From
            {
                New List(Of Double)() From {0, 0},
                New List(Of Double)() From {0, 1},
                New List(Of Double)() From {1, 0},
                New List(Of Double)() From {1, 1}
            }

                        ' Apply the forward pass to get the predicted outputs
                        Dim output As List(Of List(Of Double)) = feedForwardNN.Forward(inputSequence)

                        ' Display the input sequence and predicted outputs
                        Console.WriteLine("Input Sequence:")
                        For Each inputVector As List(Of Double) In inputSequence
                            Console.WriteLine(String.Join(", ", inputVector))
                        Next

                        Console.WriteLine("Predicted Outputs:")
                        For Each outputVector As List(Of Double) In output
                            Console.WriteLine(Math.Round(outputVector(0))) ' Round the output to the nearest integer (0 or 1)
                        Next

                        Console.ReadLine()
                    End Sub

                    Public Function Forward(inputs As List(Of List(Of Double))) As List(Of List(Of Double))
                        Dim hiddenOutputs As List(Of List(Of Double)) = LinearTransformation(inputs, hiddenWeights, hiddenSize)

                        ' Apply layer normalization and residual connections
                        Dim norm1Outputs As List(Of List(Of Double)) = layerNorm1.Normalize(hiddenOutputs)
                        Dim residual1Outputs As List(Of List(Of Double)) = AddResidualConnections(hiddenOutputs, norm1Outputs)
                        're-inject input focusing(update memory)
                        residual1Outputs = AddResidualConnections(residual1Outputs, inputs)
                        Dim activatedOutputs As List(Of List(Of Double)) = ApplyActivation(residual1Outputs, Activation.ReLU)
                        Dim finalOutputs As List(Of List(Of Double)) = LinearTransformation(activatedOutputs, outputWeights, outputSize)
                        rand.NextDouble()
                        ' Apply layer normalization and residual connections
                        Dim norm2Outputs As List(Of List(Of Double)) = layerNorm2.Normalize(finalOutputs)
                        Dim residual2Outputs As List(Of List(Of Double)) = AddResidualConnections(finalOutputs, norm2Outputs)
                        Return residual2Outputs
                    End Function

                    Private Function AddResidualConnections(inputs As List(Of List(Of Double)), outputs As List(Of List(Of Double))) As List(Of List(Of Double))
                        Dim residualOutputs As List(Of List(Of Double)) = New List(Of List(Of Double))(inputs.Count)

                        For i As Integer = 0 To inputs.Count - 1
                            Dim residualVector As List(Of Double) = New List(Of Double)(inputs(i).Count)

                            For j As Integer = 0 To inputs(i).Count - 1
                                residualVector.Add(inputs(i)(j) + outputs(i)(j))
                                rand.NextDouble()
                            Next

                            residualOutputs.Add(residualVector)
                        Next

                        Return residualOutputs
                    End Function

                    Private Function ApplyActivation(inputs As List(Of List(Of Double)), activation As Activation) As List(Of List(Of Double))
                        Dim activatedOutputs As List(Of List(Of Double)) = New List(Of List(Of Double))(inputs.Count)

                        For Each inputVector As List(Of Double) In inputs
                            Dim activatedVector As List(Of Double) = New List(Of Double)(inputVector.Count)

                            For Each inputVal As Double In inputVector
                                Select Case activation
                                    Case Activation.ReLU
                                        activatedVector.Add(Math.Max(0, inputVal))
                                    Case Activation.Sigmoid
                                        activatedVector.Add(1 / (1 + Math.Exp(-inputVal)))
                                    Case Activation.Tanh
                                        activatedVector.Add(Math.Tanh(inputVal))
                                End Select
                            Next

                            activatedOutputs.Add(activatedVector)
                        Next

                        Return activatedOutputs
                    End Function

                    Private Function GetRandomWeight() As Double
                        Return rand.NextDouble()
                    End Function

                    Private Function InitializeWeights(inputSize As Integer, outputSize As Integer) As List(Of List(Of Double))
                        Dim weights As List(Of List(Of Double)) = New List(Of List(Of Double))(inputSize)
                        rand.NextDouble()
                        For i As Integer = 0 To inputSize - 1
                            Dim weightVector As List(Of Double) = New List(Of Double)(outputSize)
                            rand.NextDouble()
                            For j As Integer = 0 To outputSize - 1
                                weightVector.Add(GetRandomWeight())
                            Next

                            weights.Add(weightVector)
                        Next

                        Return weights
                    End Function

                    Private Function LinearTransformation(inputs As List(Of List(Of Double)), weights As List(Of List(Of Double)), outputSize As Integer) As List(Of List(Of Double))
                        Dim output As List(Of List(Of Double)) = New List(Of List(Of Double))(inputs.Count)

                        For Each inputVector As List(Of Double) In inputs
                            Dim outputVector As List(Of Double) = New List(Of Double)(outputSize)

                            For j As Integer = 0 To outputSize - 1
                                Dim sum As Double = 0

                                For i As Integer = 0 To inputVector.Count - 1
                                    sum += inputVector(i) * weights(i)(j)
                                Next

                                outputVector.Add(sum)
                            Next

                            output.Add(outputVector)
                        Next

                        Return output
                    End Function

                    Public Class LayerNormalization
                        Private ReadOnly epsilon As Double
                        Private ReadOnly hiddenSize As Integer

                        Public Sub New(hiddenSize As Integer, Optional epsilon As Double = 0.000001)
                            Me.hiddenSize = hiddenSize
                            Me.epsilon = epsilon
                        End Sub

                        Public Function Normalize(inputs As List(Of List(Of Double))) As List(Of List(Of Double))
                            Dim normalizedOutputs As List(Of List(Of Double)) = New List(Of List(Of Double))(inputs.Count)

                            For Each inputVector As List(Of Double) In inputs
                                Dim mean As Double = inputVector.Average()
                                Dim variance As Double = inputVector.Select(Function(x) (x - mean) * (x - mean)).Sum() / hiddenSize
                                Dim stdDev As Double = Math.Sqrt(variance + epsilon)

                                Dim normalizedVector As List(Of Double) = inputVector.Select(Function(x) (x - mean) / stdDev).ToList()
                                normalizedOutputs.Add(normalizedVector)
                            Next

                            Return normalizedOutputs
                        End Function
                    End Class

                End Class
                Private Class NgramLanguageModel

                    Public ngramEncodings As Dictionary(Of String, Integer)

                    Public ngramModel As Dictionary(Of String, Dictionary(Of String, Integer))

                    Public ngramSize As Integer

                    Private ReadOnly rand As Random

                    Public Sub New(n As Integer)
                        ngramModel = New Dictionary(Of String, Dictionary(Of String, Integer))()
                        ngramSize = n
                        ngramEncodings = New Dictionary(Of String, Integer)()
                        rand = New Random()

                    End Sub

                    Public ReadOnly Property NgramOrder As Integer
                        Get
                            Return ngramSize - 1
                        End Get
                    End Property

                    Public Shared Function CalculateProbability(ngramModel As NgramLanguageModel, prediction As String) As Double
                        Dim tokens As String() = prediction.Split(" "c)
                        Dim probability As Double = 1.0


                        For i As Integer = 0 To tokens.Length - 2
                            Dim context As String = ngramModel.GetContext(tokens, i)
                            Dim nextToken As String = tokens(i + 1)

                            If ngramModel.ngramModel.ContainsKey(context) Then
                                Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel.ngramModel(context)
                                Dim totalOccurrences As Integer = ngramCounts.Values.Sum()

                                If ngramCounts.ContainsKey(nextToken) Then
                                    Dim count As Integer = ngramCounts(nextToken)
                                    Dim tokenProbability As Double = count / totalOccurrences
                                    probability *= tokenProbability
                                Else
                                    probability = 0.0
                                    Exit For
                                End If
                            Else
                                probability = 0.0
                                Exit For
                            End If
                        Next

                        Return probability
                    End Function


                    Public Sub AddDocument(doc As String)
                        Dim words As String() = PreprocessText(doc)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1
                        Next
                    End Sub

                    Public Sub AddDocuments(ByRef Docs As List(Of String))
                        For Each item In Docs
                            Me.AddDocument(item)
                        Next
                    End Sub

                    Public Sub AddNgram(ngram As String)
                        ngramModel(ngram) = New Dictionary(Of String, Integer)()
                    End Sub

                    Public Sub CreateEncodedModel(corpus As String)
                        Dim words As String() = PreprocessText(corpus)
                        Dim numWords As Integer = words.Length - ngramSize
                        Dim position As Integer = 0

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1

                            If Not ngramEncodings.ContainsKey(currentNgram) Then
                                ngramEncodings(currentNgram) = position
                                position += 1
                            End If
                        Next
                    End Sub

                    Public Sub CreateModel(corpus As String)
                        Dim words As String() = PreprocessText(corpus)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If Not ngramModel.ContainsKey(currentNgram) Then
                                ngramModel(currentNgram) = New Dictionary(Of String, Integer)()
                            End If

                            If Not ngramModel(currentNgram).ContainsKey(nextWord) Then
                                ngramModel(currentNgram)(nextWord) = 0
                            End If

                            ngramModel(currentNgram)(nextWord) += 1
                        Next
                    End Sub

                    Public Sub CreateModel(documents As List(Of String))
                        For Each document In documents
                            AddDocument(document)
                        Next
                    End Sub

                    Public Function EstimateProbability(nGramPrefix As String, word As String) As Double
                        If ngramModel.ContainsKey(nGramPrefix) AndAlso ngramModel(nGramPrefix).ContainsKey(word) Then
                            Dim nGramCount = ngramModel(nGramPrefix)(word)
                            Dim totalCount = ngramModel(nGramPrefix).Values.Sum()
                            Return nGramCount / totalCount
                        End If

                        Return 0.0
                    End Function

                    Public Function GenerateNextWord(nGramPrefix As String) As String
                        If ngramModel.ContainsKey(nGramPrefix) Then
                            Dim nGramCounts = ngramModel(nGramPrefix)
                            Dim totalOccurrences = nGramCounts.Values.Sum()

                            Dim randValue = rand.NextDouble()
                            Dim cumulativeProb = 0.0

                            For Each kvp In nGramCounts
                                cumulativeProb += kvp.Value / totalOccurrences
                                If cumulativeProb >= randValue Then
                                    Return kvp.Key
                                End If
                            Next
                        End If

                        Return ""
                    End Function

                    Public Overridable Function GenerateText(seedPhrase As String, length As Integer) As String
                        Dim generatedText As List(Of String) = seedPhrase.Split(" "c).ToList()

                        For i As Integer = 0 To length - ngramSize
                            Dim nGramPrefix = String.Join(" ", generatedText.Skip(i).Take(ngramSize - 1))
                            Dim nextWord = GenerateNextWord(nGramPrefix)
                            generatedText.Add(nextWord)
                        Next

                        Return String.Join(" ", generatedText)
                    End Function

                    Public Overridable Function GenerateText(maxLength As Integer, seedPhrase As String) As String
                        Dim tokens As List(Of String) = New List(Of String)(seedPhrase.Split(" "c))

                        While tokens.Count < maxLength
                            Dim context As String = GetContextfrom(tokens.ToArray(), tokens.Count - 1)

                            If ngramModel.ContainsKey(context) Then
                                Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                                Dim totalOccurrences As Integer = ngramCounts.Values.Sum()
                                Dim randomNumber As Double = New Random().NextDouble()
                                Dim cumulativeProbability As Double = 0.0

                                For Each tokenCount As KeyValuePair(Of String, Integer) In ngramCounts
                                    Dim tokenProbability As Double = tokenCount.Value / totalOccurrences
                                    cumulativeProbability += tokenProbability

                                    If cumulativeProbability >= randomNumber Then
                                        tokens.Add(tokenCount.Key)
                                        Exit For
                                    End If
                                Next
                            Else
                                Exit While
                            End If
                        End While

                        Return String.Join(" ", tokens)
                    End Function


                    Public Function GetCount(ngram As String) As Integer

                        For Each item In ngramEncodings
                            If item.Key = ngram Then
                                Return ngramEncodings(ngram)
                            End If
                        Next

                        Return 0
                    End Function

                    Public Function GetEncoding(currentNgram As String) As Integer
                        Dim position As Integer = GetPosition(currentNgram)
                        Return position
                    End Function

                    Public Function GetNextToken(context As String) As String
                        Dim nextToken As String = ""

                        If ngramModel.ContainsKey(context) Then
                            Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                            nextToken = ngramCounts.OrderByDescending(Function(ngram) ngram.Value).FirstOrDefault().Key
                        End If

                        Return nextToken
                    End Function

                    Public Function GetNgrams() As String()
                        Return ngramModel.Keys.ToArray()
                    End Function

                    Public Function GetPosition(currentNgram As String) As Integer
                        If ngramEncodings.ContainsKey(currentNgram) Then
                            Return ngramEncodings(currentNgram)
                        End If

                        Return -1
                    End Function

                    Public Function GetProbability(ngram As String) As Double
                        Return GetCount(ngram) / ngramModel.Values.SelectMany(Function(dict) dict.Values).Sum()
                    End Function

                    Public Function GetProbability(currentNgram As String, nextWord As String) As Double
                        If ngramModel.ContainsKey(currentNgram) AndAlso ngramModel(currentNgram).ContainsKey(nextWord) Then
                            Dim totalCount As Integer = ngramModel(currentNgram).Values.Sum()
                            Dim ngramCount As Integer = ngramModel(currentNgram)(nextWord)
                            Return CDbl(ngramCount) / totalCount
                        End If

                        Return 0.0
                    End Function

                    Public Function GetRandomNgram() As String
                        Dim random As New Random()
                        Dim ngrams As String() = ngramModel.Keys.ToArray()
                        Dim randomIndex As Integer = random.Next(ngrams.Length)
                        Return ngrams(randomIndex)
                    End Function

                    Public Function getTokens(Query As String) As List(Of String)
                        Dim tokens As New List(Of String)
                        Dim Tok = Split(Query, " ")
                        For Each item In Tok
                            tokens.Add(item)
                        Next
                        Return tokens
                    End Function


                    Public Function LookupNgram(ngram As String) As Integer
                        If ngramModel.ContainsKey(ngram) Then
                            Return ngramModel(ngram).Values.Sum()
                        End If
                        Return 0
                    End Function


                    Public Function PredictNextWord(currentNgram As String) As String
                        If ngramModel.ContainsKey(currentNgram) Then
                            Dim nextWords As Dictionary(Of String, Integer) = ngramModel(currentNgram)
                            Return nextWords.OrderByDescending(Function(x) x.Value).FirstOrDefault().Key
                        End If

                        Return ""
                    End Function


                    Public Function PreprocessText(text As String) As String()
                        ' Preprocess the text by removing unnecessary characters and converting to lowercase
                        text = text.ToLower()
                        text = text.Replace(".", " .")
                        text = text.Replace(",", " ,")
                        text = text.Replace(";", " ;")
                        text = text.Replace(":", " :")
                        text = text.Replace("!", " !")
                        text = text.Replace("?", " ?")

                        ' Split the text into words
                        Return text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                    End Function

                    Public Sub RemoveDocument(doc As String)
                        Dim words As String() = PreprocessText(doc)
                        Dim numWords As Integer = words.Length - ngramSize

                        For i As Integer = 0 To numWords
                            Dim currentNgram As String = String.Join(" ", words, i, ngramSize)
                            Dim nextWord As String = words(i + ngramSize)

                            If ngramModel.ContainsKey(currentNgram) Then
                                Dim nextWords As Dictionary(Of String, Integer) = ngramModel(currentNgram)
                                If nextWords.ContainsKey(nextWord) Then
                                    nextWords(nextWord) -= 1
                                    If nextWords(nextWord) <= 0 Then
                                        nextWords.Remove(nextWord)
                                    End If
                                End If
                            End If
                        Next
                    End Sub

                    Public Sub RemoveNgram(ngram As String)
                        ngramModel.Remove(ngram)
                    End Sub

                    Public Overridable Sub Train(corpus As List(Of String))
                        For Each sentence In corpus
                            Dim words = sentence.Split(" "c)
                            For i As Integer = 0 To words.Length - ngramSize
                                Dim nGramPrefix = String.Join(" ", words, i, ngramSize - 1)
                                Dim nGramSuffix = words(i + ngramSize - 1)

                                If Not ngramModel.ContainsKey(nGramPrefix) Then
                                    ngramModel(nGramPrefix) = New Dictionary(Of String, Integer)()
                                End If

                                If Not ngramModel(nGramPrefix).ContainsKey(nGramSuffix) Then
                                    ngramModel(nGramPrefix)(nGramSuffix) = 0
                                End If

                                ngramModel(nGramPrefix)(nGramSuffix) += 1
                            Next
                        Next
                        For Each line In corpus
                            Dim tokens = line.Split()
                            For i As Integer = 0 To tokens.Length - NgramOrder
                                Dim context As String = GetContext(tokens, i)
                                Dim nextToken As String = tokens(i + NgramOrder)
                                UpdateNgramModel(context, nextToken)
                            Next
                        Next
                    End Sub


                    Public Function UpdateNgram(oldNgram As String, newNgram As String) As Boolean
                        If ngramModel.ContainsKey(oldNgram) AndAlso Not ngramModel.ContainsKey(newNgram) Then
                            ' Update ngramModel
                            ngramModel(newNgram) = ngramModel(oldNgram)
                            ngramModel.Remove(oldNgram)

                            ' Update ngramEncodings
                            If ngramEncodings.ContainsKey(oldNgram) Then
                                Dim position As Integer = ngramEncodings(oldNgram)
                                ngramEncodings.Remove(oldNgram)
                                ngramEncodings(newNgram) = position
                            End If

                            Return True
                        End If
                        Return False
                    End Function

                    Public Shared Function GetContextfrom(tokens As String(), index As Integer) As String
                        Return String.Join(" ", tokens.Take(index + 1))
                    End Function

                    Public Function GetContext(tokens As List(Of String)) As String
                        Dim contextTokens As List(Of String) = tokens.Skip(Math.Max(0, tokens.Count - NgramOrder)).ToList()
                        Return String.Join(" ", contextTokens)
                    End Function

                    Public Function GetContext(tokens As String(), index As Integer) As String
                        Dim contextTokens As New List(Of String)()
                        For i As Integer = index To index + NgramOrder - 1
                            contextTokens.Add(tokens(i))
                        Next
                        Return String.Join(" ", contextTokens)
                    End Function

                    Private Sub UpdateNgramModel(context As String, nextToken As String)
                        If Not ngramModel.ContainsKey(context) Then
                            ngramModel.Add(context, New Dictionary(Of String, Integer)())
                        End If

                        Dim ngramCounts As Dictionary(Of String, Integer) = ngramModel(context)
                        If ngramCounts.ContainsKey(nextToken) Then
                            ngramCounts(nextToken) += 1
                        Else
                            ngramCounts.Add(nextToken, 1)
                        End If
                    End Sub
                End Class
                Public Class PositionalDecoder
                    ''' <summary>
                    ''' Only a list of the vocabulary words (to create an index) 
                    ''' this should be the same list used to encode (Must be Set)
                    ''' </summary>
                    Public Vocabulary As New List(Of String)

                    Private ReadOnly decodingMatrix As List(Of List(Of Double))
                    Public Sub New(maxLength As Integer, embeddingSize As Integer, ByRef vocab As List(Of String))
                        decodingMatrix = New List(Of List(Of Double))()
                        Vocabulary = vocab
                        ' Create the decoding matrix
                        For pos As Integer = 0 To maxLength - 1
                            Dim decodingRow As List(Of Double) = New List(Of Double)()

                            For i As Integer = 0 To embeddingSize - 1
                                Dim angle As Double = pos / Math.Pow(10000, (2 * i) / embeddingSize)
                                decodingRow.Add(Math.Sin(angle))
                                decodingRow.Add(Math.Cos(angle))
                            Next

                            decodingMatrix.Add(decodingRow)
                        Next
                    End Sub
                    Public Function Decode(encodedInputs As List(Of List(Of Double))) As List(Of String)
                        Dim decodedTokens As List(Of String) = New List(Of String)()

                        For Each encoding As List(Of Double) In encodedInputs
                            ' Retrieve the token index based on the encoding
                            Dim tokenIndex As Integer = GetTokenIndex(encoding)

                            ' Retrieve the token based on the index
                            If tokenIndex >= 0 Then
                                Dim token As String = GetToken(tokenIndex)
                                decodedTokens.Add(token)
                            Else
                                ' Handle unknown encodings if necessary
                            End If
                        Next

                        Return decodedTokens
                    End Function
                    Public Function iDecode(encodedInputs As List(Of List(Of Double))) As List(Of String)
                        Dim decodedTokens As List(Of String) = New List(Of String)()

                        For Each encoding As List(Of Double) In encodedInputs
                            ' Retrieve the token index based on the encoding
                            Dim tokenIndex As Integer = GetTokenIndex(encoding)

                            ' Retrieve the token based on the index
                            If tokenIndex >= 0 Then
                                Dim token As String = GetToken(tokenIndex)
                                decodedTokens.Add(token)
                            Else
                                ' Handle unknown encodings if necessary
                            End If
                        Next

                        Return decodedTokens
                    End Function

                    Private Function GetToken(tokenIndex As Integer) As String
                        ' Retrieve the token based on the index
                        ' For simplicity, let's assume a fixed vocabulary
                        Dim vocabulary As List(Of String) = GetVocabulary()

                        If tokenIndex >= 0 AndAlso tokenIndex < vocabulary.Count Then
                            Return vocabulary(tokenIndex)
                        Else
                            Return "Unknown" ' Unknown token
                        End If
                    End Function

                    Private Function GetTokenIndex(encoding As List(Of Double)) As Integer
                        ' Retrieve the index of the token based on the encoding
                        ' For simplicity, let's assume a fixed vocabulary
                        Dim vocabulary As List(Of String) = GetVocabulary()

                        For i As Integer = 0 To decodingMatrix.Count - 1
                            If encoding.SequenceEqual(decodingMatrix(i)) Then
                                Return i
                            End If
                        Next

                        Return -1 ' Token not found
                    End Function
                    Private Function GetVocabulary() As List(Of String)
                        ' Return the vocabulary list
                        ' Modify this function as per your specific vocabulary
                        Return Vocabulary
                    End Function
                End Class
                Public Class PositionalEncoding
                    Private ReadOnly encodingMatrix As List(Of List(Of Double))
                    Private Vocabulary As New List(Of String)
                    Public Sub New(maxLength As Integer, embeddingSize As Integer, ByRef vocab As List(Of String))
                        encodingMatrix = New List(Of List(Of Double))
                        Vocabulary = vocab
                        ' Create the encoding matrix
                        For pos As Integer = 0 To maxLength - 1
                            Dim encodingRow As List(Of Double) = New List(Of Double)()

                            For i As Integer = 0 To embeddingSize - 1
                                Dim angle As Double = pos / Math.Pow(10000, (2 * i) / embeddingSize)
                                encodingRow.Add(Math.Sin(angle))
                                encodingRow.Add(Math.Cos(angle))
                            Next

                            encodingMatrix.Add(encodingRow)
                        Next
                    End Sub

                    Public Function Encode(inputTokens As List(Of String)) As List(Of List(Of Double))
                        Dim encodedInputs As List(Of List(Of Double)) = New List(Of List(Of Double))()

                        For pos As Integer = 0 To inputTokens.Count - 1
                            Dim token As String = inputTokens(pos)
                            Dim tokenEncoding As List(Of Double) = New List(Of Double)()

                            ' Retrieve the positional encoding for the token
                            tokenEncoding = encodingMatrix(pos)

                            encodedInputs.Add(tokenEncoding)
                        Next

                        Return encodedInputs
                    End Function

                    Public Function iEncode(inputTokens As List(Of String)) As List(Of List(Of Double))
                        Dim encodedInputs As List(Of List(Of Double)) = New List(Of List(Of Double))()

                        For Each token As String In inputTokens
                            Dim tokenEncoding As List(Of Double) = New List(Of Double)()

                            ' Find the index of the token in the vocabulary
                            ' For simplicity, let's assume a fixed vocabulary
                            Dim tokenIndex As Integer = GetTokenIndex(token)

                            ' Retrieve the positional encoding for the token
                            If tokenIndex >= 0 Then
                                tokenEncoding = encodingMatrix(tokenIndex)
                            Else
                                ' Handle unknown tokens if necessary
                            End If

                            encodedInputs.Add(tokenEncoding)
                        Next

                        Return encodedInputs
                    End Function
                    Private Function GetTokenIndex(token As String) As Integer
                        ' Retrieve the index of the token in the vocabulary
                        ' For simplicity, let's assume a fixed vocabulary
                        Dim vocabulary As List(Of String) = GetVocabulary()
                        Return vocabulary.IndexOf(token)
                    End Function

                    Private Function GetVocabulary() As List(Of String)
                        ' Return the vocabulary list
                        ' Modify this function as per your specific vocabulary
                        Return Vocabulary
                    End Function
                End Class
            End Class
            Public Class PredictiveLanguageModel
                Public ReadOnly Model As NgramLanguageModel
                Private Vocab As Corpus.Vocabulary
                'A Local Vocab List is held For references
                Public ReadOnly VocabList As List(Of String) = Vocab.GetVocab
                Private EncoderDecoder As EncodeDecoder
                Public ReadOnly Dmodel As Integer
                ''' <summary>
                ''' Context Value
                ''' </summary>
                Public ReadOnly MaxLength As Integer

                ''' <summary>
                ''' 
                ''' </summary>
                ''' <param name="MaxLength">Length of Potential Content input: 
                ''' For Code Generation projects A larger Context size may be desired
                ''' For Next Word Prediction  -  Bigram - Ngram model is enough. 
                ''' Tasked based decision</param>
                Public Sub New(ByRef MaxLength As Integer, ByRef Dmodel As Integer)

                    Model = New NgramLanguageModel(MaxLength)
                    Vocab = New Corpus.Vocabulary()
                    Dmodel = Dmodel
                    EncoderDecoder = New EncodeDecoder(Model, Dmodel, Vocab)
                    MaxLength = MaxLength


                End Sub
                Public Sub New(ByRef Pretrained As PredictiveLanguageModel)
                    Me.Model = Pretrained.Model
                    Me.Vocab = Vocab
                    Me.EncoderDecoder = Pretrained.EncoderDecoder
                    Me.Dmodel = Pretrained.Dmodel
                    Me.MaxLength = Pretrained.MaxLength
                End Sub
                Public Function ExportModel() As PredictiveLanguageModel
                    Return Me
                End Function
                ''' <summary>
                ''' This adds a token to the Language model
                ''' </summary>
                ''' <param name="Term"></param>
                Public Sub AddToken_ToVocabulary(ByRef Term As String)
                    Model.AddNgram(Term)
                    Vocab.AddNew(Term)
                End Sub
                ''' <summary>
                ''' Returns Input Vector For sentence
                ''' </summary>
                ''' <param name="Query">Input Vector</param>
                ''' <returns></returns>
                Public Function GetInputVector(ByRef Query As String) As List(Of Integer)

                    Return GET_Vocab_IDs(Query)
                End Function
                ''' <summary>
                ''' Create Training Blocks and Targets
                ''' </summary>
                ''' <param name="iCorpus">Training Corpus as single String</param>
                ''' <param name="BlockSize">best to be lower than the max Sentence lengths</param>
                ''' <returns></returns>
                Public Function CreateTrainingData(ByRef iCorpus As String, ByRef BlockSize As Integer) As Corpus.Vocabulary.InputTextRecord
                    Dim InputLayer As New Corpus.Vocabulary.InputTextRecord

                    InputLayer.Text = iCorpus
                    InputLayer.blocksize = BlockSize
                    'Encode Whole Text
                    InputLayer.Encoding = GET_Vocab_IDs(iCorpus)
                    'Grab Input
                    InputLayer.Inputblocks = Corpus.Vocabulary.InputTextRecord.GetBlocks(InputLayer.Encoding, BlockSize)
                    'Grab Targets
                    InputLayer.Targetblocks = Corpus.Vocabulary.InputTextRecord.GetTargetBlocks(InputLayer.Encoding, BlockSize)

                    Return InputLayer
                End Function
                Private Function GET_Vocab_IDs(ByRef Query As String) As List(Of Integer)
                    Dim Toks = Tokenizer.TokenizeByWord(Query)
                    Dim Str As New List(Of Integer)
                    For Each tok In Toks
                        Str.Add(EncoderDecoder.GetVocab_ID(tok.Value))
                    Next
                    Return Str
                End Function
                Private Class EncodeDecoder
                    Private EmbeddingEncoder As PositionalEncoderDecoder
                    Private Model As NgramLanguageModel
                    Private Vocab As Corpus.Vocabulary
                    'A Local Vocab List is held For references
                    Private VocabList As List(Of String) = Vocab.GetVocab
                    Public Sub New(ByRef Model As NgramLanguageModel, ByRef D_model As Integer, ByRef Vocab As Corpus.Vocabulary)
                        Me.Model = Model
                        Me.Vocab = Vocab
                        EmbeddingEncoder = New PositionalEncoderDecoder(Model.ngramSize, D_model, VocabList)

                    End Sub
                    Public Function GetModel_ID(ByRef Token As String) As Integer
                        Return Model.GetEncoding(Token)
                    End Function
                    Public Function GetVocab_ID(ByRef Token As String) As Integer
                        Return Corpus.Vocabulary.Decode.DecodeText(Token, Vocab.Current) & " "
                    End Function
                    Public Function DecodeVocab_ID(ByRef Vocab_id As Integer) As String

                        Return Corpus.Vocabulary.Decode.DecodeInteger(Vocab_id, Vocab.Current)

                    End Function
                    Public Function DecodeModel_ID(ByRef Model_id As Integer) As String

                        Return Model.GetPosition(Model_id)
                    End Function
                    Public Function EncodePos(ByRef Tokens As List(Of String)) As List(Of List(Of Double))

                        EmbeddingEncoder.Vocabulary = VocabList
                        Return EmbeddingEncoder.Encode(Tokens)
                    End Function
                    Public Function DecodePos(ByRef Tokens As List(Of List(Of Double))) As List(Of String)

                        EmbeddingEncoder.Vocabulary = VocabList
                        Return EmbeddingEncoder.Decode(Tokens)
                    End Function
                    Private Class PositionalEncoderDecoder
                        Private ReadOnly encodingMatrix As List(Of List(Of Double))
                        Public Vocabulary As New List(Of String)

                        Public Sub New(maxLength As Integer, Dmodel As Integer, ByRef vocab As List(Of String))
                            encodingMatrix = New List(Of List(Of Double))
                            Vocabulary = vocab
                            ' Create the encoding matrix
                            For pos As Integer = 0 To maxLength - 1
                                Dim encodingRow As List(Of Double) = New List(Of Double)()

                                For i As Integer = 0 To Dmodel - 1
                                    Dim angle As Double = pos / Math.Pow(10000, (2 * i) / Dmodel)
                                    encodingRow.Add(Math.Sin(angle))
                                    encodingRow.Add(Math.Cos(angle))
                                Next

                                encodingMatrix.Add(encodingRow)
                            Next

                            Vocabulary = vocab

                        End Sub

                        Public Function Decode(encodedInputs As List(Of List(Of Double))) As List(Of String)
                            Dim decodedTokens As List(Of String) = New List(Of String)

                            For Each encoding As List(Of Double) In encodedInputs
                                ' Retrieve the token index based on the encoding
                                Dim tokenIndex As Integer = GetTokenIndex(encoding)

                                ' Retrieve the token based on the index
                                If tokenIndex >= 0 Then
                                    Dim token As String = GetToken(tokenIndex)
                                    decodedTokens.Add(token)
                                Else
                                    ' Handle unknown encodings if necessary
                                End If
                            Next

                            Return decodedTokens
                        End Function

                        Public Function Encode(inputTokens As List(Of String)) As List(Of List(Of Double))
                            Dim encodedInputs As List(Of List(Of Double)) = New List(Of List(Of Double))

                            For pos As Integer = 0 To inputTokens.Count - 1
                                Dim token As String = inputTokens(pos)
                                Dim tokenEncoding As List(Of Double) = New List(Of Double)()

                                ' Retrieve the positional encoding for the token
                                tokenEncoding = encodingMatrix(pos)

                                encodedInputs.Add(tokenEncoding)
                            Next

                            Return encodedInputs
                        End Function

                        Private Function GetToken(tokenIndex As Integer) As String
                            ' Retrieve the token based on the index
                            ' For simplicity, let's assume a fixed vocabulary
                            Dim vocabulary As List(Of String) = GetVocabulary()

                            If tokenIndex >= 0 AndAlso tokenIndex < vocabulary.Count Then
                                Return vocabulary(tokenIndex)
                            Else
                                Return "Unknown" ' Unknown token
                            End If
                        End Function

                        Private Function GetTokenIndex(token As String) As Integer
                            ' Retrieve the index of the token in the vocabulary
                            ' For simplicity, let's assume a fixed vocabulary
                            Dim vocabulary As List(Of String) = GetVocabulary()
                            Return vocabulary.IndexOf(token)
                        End Function

                        Private Function GetTokenIndex(encoding As List(Of Double)) As Integer
                            ' Retrieve the index of the token based on the encoding
                            ' For simplicity, let's assume a fixed vocabulary
                            Dim vocabulary As List(Of String) = GetVocabulary()

                            For i As Integer = 0 To encodingMatrix.Count - 1
                                If encoding.SequenceEqual(encodingMatrix(i)) Then
                                    Return i
                                End If
                            Next

                            Return -1 ' Token not found
                        End Function

                        Private Function GetVocabulary() As List(Of String)
                            ' Return the vocabulary list
                            ' Modify this function as per your specific vocabulary
                            Return Vocabulary
                        End Function

                    End Class
                End Class
                Public Class Tokenizer

                    ''' <summary>
                    ''' Recognized Tokens
                    ''' </summary>
                    Public Enum TokenType
                        GramaticalPunctuation
                        EncapuslationPunctuationStart
                        EncapuslationPunctuationEnd
                        MoneyPunctuation
                        MathPunctuation
                        CodePunctuation
                        AlphaBet
                        Number
                        SeperatorPunctuation
                        Ignore
                    End Enum

                    Public Const ClassId As String = "2899E490-7702-401C-BAB3-38FF97BC1AC9"
                    Public Const EventsId As String = "CD994307-F53E-401A-AC6D-3CFDD86FD6F1"
                    Public Const InterfaceId As String = "8B1145F1-5D13-4059-829B-B531310144B5"

                    Public Shared ReadOnly AlphaBet() As String = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
                        "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
                        "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"}

                    Public Shared ReadOnly CodePunctuation() As String = {"\", "#", "@", "^"}
                    Public Shared ReadOnly EncapuslationPunctuationEnd() As String = {"}", "]", ">", ")"}
                    Public Shared ReadOnly EncapuslationPunctuationStart() As String = {"{", "[", "<", "("}
                    Public Shared ReadOnly GramaticalPunctuation() As String = {".", "?", "!", ":", ";"}
                    Public Shared ReadOnly MathPunctuation() As String = {"+", "-", "=", "/", "*", "%", "PLUS", "ADD", "MINUS", "SUBTRACT", "DIVIDE", "DIFFERENCE", "TIMES", "MULTIPLY", "PERCENT", "EQUALS"}
                    Public Shared ReadOnly MoneyPunctuation() As String = {"£", "$"}

                    Public Shared ReadOnly Number() As String = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                    "30", "40", "50", "60", "70", "80", "90", "00", "000", "0000", "00000", "000000", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen",
                    "nineteen", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety", "hundred", "thousand", "million", "Billion"}

                    Public Shared ReadOnly SeperatorPunctuation() = {" ", ",", "|"}

                    ' these are the common word delimiters
                    Public Shared Delimiters() As Char = {CType(" ", Char), CType(".", Char),
                                    CType(",", Char), CType("?", Char),
                                    CType("!", Char), CType(";", Char),
                                    CType(":", Char), Chr(10), Chr(13), vbTab}

                    'Tokenizer
                    ''' <summary>
                    ''' Returns Characters in String as list
                    ''' </summary>
                    ''' <param name="InputStr"></param>
                    ''' <returns></returns>
                    Public Function Tokenizer(ByRef InputStr As String) As List(Of String)
                        Tokenizer = New List(Of String)
                        InputStr = GetValidTokens(InputStr)

                        Dim Endstr As Integer = InputStr.Length
                        For i = 1 To Endstr
                            Tokenizer.Add(InputStr(i - 1))
                        Next
                    End Function

                    ''' <summary>
                    ''' Return Tokens in string divided by seperator
                    ''' </summary>
                    ''' <param name="InputStr"></param>
                    ''' <param name="Divider">  </param>
                    ''' <returns></returns>
                    Public Function Tokenizer(ByRef InputStr As String, ByRef Divider As String) As List(Of String)
                        Tokenizer = New List(Of String)
                        InputStr = GetValidTokens(InputStr)
                        Dim Tokens() As String = InputStr.Split(Divider)

                        For Each item In Tokens
                            Tokenizer.Add(item)
                        Next
                    End Function

                    ''' <summary>
                    ''' each character can be defined as a particular token enabling for removal of unwanted token types
                    ''' </summary>
                    ''' <param name="CharStr"></param>
                    ''' <returns></returns>
                    Public Function GetTokenType(ByRef CharStr As String) As TokenType
                        For Each item In SeperatorPunctuation
                            If CharStr = item Then Return TokenType.SeperatorPunctuation
                        Next
                        For Each item In GramaticalPunctuation
                            If CharStr = item Then Return TokenType.GramaticalPunctuation
                        Next
                        For Each item In EncapuslationPunctuationStart
                            If CharStr = item Then Return TokenType.EncapuslationPunctuationStart
                        Next
                        For Each item In EncapuslationPunctuationEnd
                            If CharStr = item Then Return TokenType.EncapuslationPunctuationEnd
                        Next
                        For Each item In MoneyPunctuation
                            If CharStr = item Then Return TokenType.MoneyPunctuation
                        Next
                        For Each item In MathPunctuation
                            If CharStr = item Then Return TokenType.MathPunctuation
                        Next
                        For Each item In CodePunctuation
                            If CharStr = item Then Return TokenType.CodePunctuation
                        Next
                        For Each item In AlphaBet
                            If CharStr = item Then Return TokenType.AlphaBet
                        Next
                        For Each item In Number
                            If CharStr = item Then Return TokenType.Number
                        Next
                        Return TokenType.Ignore
                    End Function

                    ''' <summary>
                    ''' Returns valid tokens only tokens that are not defined are removed
                    ''' </summary>
                    ''' <param name="InputStr"></param>
                    ''' <returns></returns>
                    Private Function GetValidTokens(ByRef InputStr As String) As String
                        Dim EndStr As Integer = InputStr.Length
                        Dim CharStr As String = ""
                        For i = 0 To EndStr - 1
                            If GetTokenType(InputStr(i)) <> TokenType.Ignore Then
                                CharStr = CharStr.AddSuffix(InputStr(i))
                            Else

                            End If
                        Next
                        Return CharStr
                    End Function

                    ''' <summary>
                    ''' Removes Tokens From String by Type
                    ''' </summary>
                    ''' <param name="UserStr"></param>
                    ''' <param name="nType">  </param>
                    ''' <returns></returns>
                    Public Function RemoveTokenType(ByRef UserStr As String, ByRef nType As TokenType) As String

                        Select Case nType
                            Case TokenType.GramaticalPunctuation
                                For Each item In GramaticalPunctuation
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.AlphaBet
                                For Each item In AlphaBet
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.CodePunctuation
                                For Each item In CodePunctuation
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.EncapuslationPunctuationEnd
                                For Each item In EncapuslationPunctuationEnd
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.EncapuslationPunctuationStart
                                For Each item In EncapuslationPunctuationStart
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.Ignore
                            Case TokenType.MathPunctuation
                                For Each item In MathPunctuation
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.MoneyPunctuation
                                For Each item In MoneyPunctuation
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.Number
                                For Each item In Number
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next
                            Case TokenType.SeperatorPunctuation
                                For Each item In SeperatorPunctuation
                                    If UCase(UserStr).Contains(UCase(item)) = True Then
                                        UserStr = UCase(UserStr).Remove(UCase(item))
                                    End If
                                Next

                        End Select
                        Return UserStr
                    End Function

                    'Form Extensions
                    ''' <summary>
                    ''' Counts tokens in string
                    ''' </summary>
                    ''' <param name="InputStr"></param>
                    ''' <param name="Delimiter"></param>
                    ''' <returns></returns>
                    Public Shared Function CountTokens(ByRef InputStr As String, ByRef Delimiter As String) As Integer
                        Dim Words() As String = Split(InputStr, Delimiter)
                        Return Words.Count
                    End Function

                    ''' <summary>
                    ''' Checks if input contains Ecapuslation Punctuation
                    ''' </summary>
                    ''' <param name="Userinput"></param>
                    ''' <returns></returns>
                    Public Function ContainsEncapsulated(ByRef Userinput As String) As Boolean
                        Dim Start = False
                        Dim Ending = False
                        ContainsEncapsulated = False
                        For Each item In EncapuslationPunctuationStart
                            If Userinput.Contains(item) = True Then Start = True
                        Next
                        For Each item In EncapuslationPunctuationEnd
                            If Userinput.Contains(item) = True Then Ending = True
                        Next
                        If Start And Ending = True Then
                            ContainsEncapsulated = True
                        End If
                    End Function

                    ''' <summary>
                    ''' Gets encapsulated items found in userinput
                    ''' </summary>
                    ''' <param name="USerinput"></param>
                    ''' <returns></returns>
                    Public Function GetEncapsulated(ByRef Userinput As String) As List(Of String)
                        GetEncapsulated = New List(Of String)
                        Do Until ContainsEncapsulated(Userinput) = False
                            GetEncapsulated.Add(ExtractEncapsulated(Userinput))
                        Loop
                    End Function

                    ''' <summary>
                    ''' Extracts first Encapsulated located in string
                    ''' </summary>
                    ''' <param name="Userinput"></param>
                    ''' <returns></returns>
                    Public Function ExtractEncapsulated(ByRef Userinput As String) As String
                        ExtractEncapsulated = Userinput
                        If ContainsEncapsulated(ExtractEncapsulated) = True Then
                            If ExtractEncapsulated.Contains("(") = True And ExtractEncapsulated.Contains(")") = True Then
                                ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("(", ")")
                            End If
                            If Userinput.Contains("[") = True And Userinput.Contains("]") = True Then
                                ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("[", "]")
                            End If
                            If Userinput.Contains("{") = True And Userinput.Contains("}") = True Then
                                ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("{", "}")
                            End If
                            If Userinput.Contains("<") = True And Userinput.Contains(">") = True Then
                                ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("<", ">")
                            End If
                        End If
                    End Function

                    ''' <summary>
                    ''' Normalizes the input string by converting it to lowercase and removing punctuation and extra whitespace.
                    ''' </summary>
                    ''' <param name="input">The input string.</param>
                    ''' <returns>The normalized input string.</returns>
                    Public Function NormalizeInput(input As String) As String
                        ' Convert to lowercase
                        Dim normalizedInput As String = input.ToLower()

                        ' Remove punctuation
                        normalizedInput = Regex.Replace(normalizedInput, "[^\w\s]", "")

                        ' Remove extra whitespace
                        normalizedInput = Regex.Replace(normalizedInput, "\s+", " ")

                        Return normalizedInput
                    End Function

                    ''' <summary>
                    ''' Tokenizes the input string by character.
                    ''' </summary>
                    ''' <param name="input">The input string.</param>
                    ''' <returns>The list of character tokens.</returns>
                    Public Shared Function TokenizeByCharacter(input As String) As List(Of Token)
                        Dim tokens As New List(Of Token)

                        For i As Integer = 0 To input.Length - 1
                            Dim token As New Token(input(i).ToString())
                            tokens.Add(token)
                        Next

                        Return tokens
                    End Function

                    ''' <summary>
                    ''' Tokenizes the input string by word.
                    ''' </summary>
                    ''' <param name="input">The input string.</param>
                    ''' <returns>The list of word tokens.</returns>
                    Public Shared Function TokenizeByWord(input As String) As List(Of Token)
                        Dim tokens As New List(Of Token)
                        Dim words As String() = input.Split(" "c)

                        For i As Integer = 0 To words.Length - 1
                            Dim token As New Token(words(i))
                            tokens.Add(token)
                        Next

                        Return tokens
                    End Function

                    ''' <summary>
                    ''' Tokenizes (Sentence) the input string by sentence.
                    ''' </summary>
                    ''' <param name="input">The input string.</param>
                    ''' <returns>The list of sentence tokens.</returns>
                    Public Shared Function TokenizeBySentence(input As String) As List(Of Token)
                        Dim tokens As New List(Of Token)
                        Dim sentences As String() = input.Split("."c)

                        For i As Integer = 0 To sentences.Length - 1
                            Dim token As New Token(sentences(i))
                            tokens.Add(token)
                        Next

                        Return tokens
                    End Function

                    ''' <summary>
                    ''' Tokenizes(Words) the input string by whitespace.
                    ''' </summary>
                    ''' <param name="input">The input string.</param>
                    ''' <returns>The list of tokens.</returns>
                    Public Shared Function Tokenize(input As String) As List(Of String)
                        ' Simple tokenization by splitting on whitespace
                        Return New List(Of String)(input.Split({" "c}, StringSplitOptions.RemoveEmptyEntries))
                    End Function

                    ''' <summary>
                    ''' Checks if string is a reserved VBscipt Keyword
                    ''' </summary>
                    ''' <param name="keyword"></param>
                    ''' <returns></returns>
                    Public Function IsReservedWord(ByVal keyword As String) As Boolean
                        Dim IsReserved = False
                        Select Case LCase(keyword)
                            Case "and" : IsReserved = True
                            Case "as" : IsReserved = True
                            Case "boolean" : IsReserved = True
                            Case "byref" : IsReserved = True
                            Case "byte" : IsReserved = True
                            Case "byval" : IsReserved = True
                            Case "call" : IsReserved = True
                            Case "case" : IsReserved = True
                            Case "class" : IsReserved = True
                            Case "const" : IsReserved = True
                            Case "currency" : IsReserved = True
                            Case "debug" : IsReserved = True
                            Case "dim" : IsReserved = True
                            Case "do" : IsReserved = True
                            Case "double" : IsReserved = True
                            Case "each" : IsReserved = True
                            Case "else" : IsReserved = True
                            Case "elseif" : IsReserved = True
                            Case "empty" : IsReserved = True
                            Case "end" : IsReserved = True
                            Case "endif" : IsReserved = True
                            Case "enum" : IsReserved = True
                            Case "eqv" : IsReserved = True
                            Case "event" : IsReserved = True
                            Case "exit" : IsReserved = True
                            Case "false" : IsReserved = True
                            Case "for" : IsReserved = True
                            Case "function" : IsReserved = True
                            Case "get" : IsReserved = True
                            Case "goto" : IsReserved = True
                            Case "if" : IsReserved = True
                            Case "imp" : IsReserved = True
                            Case "implements" : IsReserved = True
                            Case "in" : IsReserved = True
                            Case "integer" : IsReserved = True
                            Case "is" : IsReserved = True
                            Case "let" : IsReserved = True
                            Case "like" : IsReserved = True
                            Case "long" : IsReserved = True
                            Case "loop" : IsReserved = True
                            Case "lset" : IsReserved = True
                            Case "me" : IsReserved = True
                            Case "mod" : IsReserved = True
                            Case "new" : IsReserved = True
                            Case "next" : IsReserved = True
                            Case "not" : IsReserved = True
                            Case "nothing" : IsReserved = True
                            Case "null" : IsReserved = True
                            Case "on" : IsReserved = True
                            Case "option" : IsReserved = True
                            Case "optional" : IsReserved = True
                            Case "or" : IsReserved = True
                            Case "paramarray" : IsReserved = True
                            Case "preserve" : IsReserved = True
                            Case "private" : IsReserved = True
                            Case "public" : IsReserved = True
                            Case "raiseevent" : IsReserved = True
                            Case "redim" : IsReserved = True
                            Case "rem" : IsReserved = True
                            Case "resume" : IsReserved = True
                            Case "rset" : IsReserved = True
                            Case "select" : IsReserved = True
                            Case "set" : IsReserved = True
                            Case "shared" : IsReserved = True
                            Case "single" : IsReserved = True
                            Case "static" : IsReserved = True
                            Case "stop" : IsReserved = True
                            Case "sub" : IsReserved = True
                            Case "then" : IsReserved = True
                            Case "to" : IsReserved = True
                            Case "true" : IsReserved = True
                            Case "type" : IsReserved = True
                            Case "typeof" : IsReserved = True
                            Case "until" : IsReserved = True
                            Case "variant" : IsReserved = True
                            Case "wend" : IsReserved = True
                            Case "while" : IsReserved = True
                            Case "with" : IsReserved = True
                            Case "xor" : IsReserved = True
                        End Select
                        Return IsReserved
                    End Function

                    Public Class Token

                        ''' <summary>
                        ''' Initializes a new instance of the Token class.
                        ''' </summary>
                        ''' <param name="value">The string value of the token.</param>
                        Public Sub New(value As String)
                            If value Is Nothing Then
                                Throw New ArgumentNullException(NameOf(value))
                            End If

                            Me.Value = value
                        End Sub

                        ''' <summary>
                        ''' Initializes a new instance of the Token class with sequence encoding.
                        ''' </summary>
                        ''' <param name="value">The string value of the token.</param>
                        ''' <param name="sequenceEncoding">The sequence encoding value of the token.</param>
                        Public Sub New(value As String, sequenceEncoding As Integer)
                            Me.New(value)
                            Me.SequenceEncoding = sequenceEncoding
                        End Sub

                        ''' <summary>
                        ''' Gets or sets the embeddings of the token.
                        ''' </summary>
                        Public Property Embeddings As List(Of Double)

                        ''' <summary>
                        ''' Gets or sets the string value of the token.
                        ''' </summary>
                        Public Property Value As String

                        ''' <summary>
                        ''' Gets or sets the sequence encoding value of the token.
                        ''' </summary>
                        Public Property SequenceEncoding As Integer

                        ''' <summary>
                        ''' Gets or sets the positional encoding value of the token.
                        ''' </summary>
                        Public Property PositionalEncoding As Integer

                        ''' <summary>
                        ''' Gets or sets the frequency of the token in the language model corpus.
                        ''' </summary>
                        Public Property Frequency As Double

                        ''' <summary>
                        ''' Gets or sets the embedding vector of the token.
                        ''' </summary>
                        Public Property Embedding As Double

                        ''' <summary>
                        ''' Calculates the similarity between this token and the given token.
                        ''' </summary>
                        ''' <param name="token">The other token.</param>
                        ''' <returns>The similarity value between the tokens.</returns>
                        Public Function CalculateSimilarity(token As Token) As Double
                            If Embeddings IsNot Nothing AndAlso token.Embeddings IsNot Nothing Then
                                Dim dotProduct As Double = 0.0
                                Dim magnitudeA As Double = 0.0
                                Dim magnitudeB As Double = 0.0

                                For i As Integer = 0 To Embeddings.Count - 1
                                    dotProduct += Embeddings(i) * token.Embeddings(i)
                                    magnitudeA += Math.Pow(Embeddings(i), 2)
                                    magnitudeB += Math.Pow(token.Embeddings(i), 2)
                                Next

                                magnitudeA = Math.Sqrt(magnitudeA)
                                magnitudeB = Math.Sqrt(magnitudeB)

                                If magnitudeA = 0.0 OrElse magnitudeB = 0.0 Then
                                    Return 0.0
                                Else
                                    Return dotProduct / (magnitudeA * magnitudeB)
                                End If
                            Else
                                Return 0.0
                            End If
                        End Function

                        Public Function CalculateSelfAttention(tokens As List(Of Token)) As Double
                            Dim total As Double = 0.0
                            For Each token As Token In tokens
                                total += CalcSimilarity(token)
                            Next
                            Return Math.Log(Math.Sqrt(total))
                        End Function

                        Private Function CalcSimilarity(token As Token) As Double
                            If Embeddings IsNot Nothing AndAlso token.Embeddings IsNot Nothing Then
                                Dim dotProduct As Double = 0.0
                                For i As Integer = 0 To Embeddings.Count - 1
                                    dotProduct += Embeddings(i) * token.Embeddings(i)
                                Next
                                Return dotProduct
                            End If
                            Return 0.0
                        End Function

                        ''' <summary>
                        ''' Calculates the self-attention of the token within the given list of tokens.
                        ''' </summary>
                        ''' <param name="tokens">The list of tokens.</param>
                        ''' <returns>The self-attention value of the token.</returns>
                        Public Function CalculateAttention(tokens As List(Of Token)) As Double
                            Dim qVector As List(Of Double) = Me.Embeddings
                            Dim kMatrix As New List(Of Double)
                            Dim vMatrix As New List(Of Double)

                            ' Create matrices K and V
                            For Each token In tokens
                                kMatrix.Add(token.Embedding)
                                vMatrix.Add(token.Embedding)
                            Next

                            ' Compute self-attention
                            Dim attention As Double = 0.0
                            Dim sqrtKLength As Double = Math.Sqrt(kMatrix(0))

                            For i As Integer = 0 To kMatrix.Count - 1
                                Dim kVector As List(Of Double) = kMatrix
                                Dim dotProduct As Double = 0.0

                                ' Check vector dimensions
                                If qVector.Count = kVector.Count Then
                                    For j As Integer = 0 To qVector.Count - 1
                                        dotProduct += qVector(j) * kVector(j)
                                    Next

                                    dotProduct /= sqrtKLength
                                    attention += dotProduct * vMatrix(i) ' We consider only the first element of the value vector for simplicity
                                Else
                                    ' Handle case when vector dimensions do not match
                                    Console.WriteLine("Vector dimensions do not match.")
                                End If
                            Next

                            Return attention
                        End Function

                    End Class

                    'Form Extensions
                    ''' <summary>
                    ''' Counts tokens in string
                    ''' </summary>
                    ''' <param name="InputStr"></param>
                    ''' <param name="Delimiter"></param>
                    ''' <returns></returns>

                    Public Function CountPossibleTokens(ByRef InputStr As String, ByRef Delimiter As String) As Integer
                        Dim Words() As String = Split(InputStr, Delimiter)
                        Return Words.Count
                    End Function

                    Public Function AlphanumericOnly(ByRef Str As String) As String
                        Str = GetValidTokens(Str)
                        Str = RemoveTokenType(Str, TokenType.CodePunctuation)
                        Str = RemoveTokenType(Str, TokenType.EncapuslationPunctuationEnd)
                        Str = RemoveTokenType(Str, TokenType.EncapuslationPunctuationStart)
                        Str = RemoveTokenType(Str, TokenType.MathPunctuation)
                        Str = RemoveTokenType(Str, TokenType.MoneyPunctuation)
                        Str = RemoveTokenType(Str, TokenType.GramaticalPunctuation)
                        Str = Str.Remove(",")
                        Str = Str.Remove("|")
                        Str = Str.Remove("_")
                        Return Str
                    End Function

                End Class


            End Class


        End Class

    End Namespace

End Namespace