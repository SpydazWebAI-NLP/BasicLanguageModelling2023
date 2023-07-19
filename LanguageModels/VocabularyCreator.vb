Imports System.Web.Script.Serialization

Namespace LanguageModels

    Public Structure WordVector
        Dim Freq As Integer
        Public NormalizedEncoding As Integer
        Public OneHotEncoding As Integer
        Public PositionalEncoding As Double()
        Dim PositionalEncodingVector As List(Of Double)
        Dim SequenceEncoding As Integer
        Dim Token As String

        ''' <summary>
        ''' adds positional encoding to list of word_vectors (ie encoded document)
        ''' Presumes a dimensional model of 512
        ''' </summary>
        ''' <param name="DccumentStr">Current Document</param>
        ''' <returns></returns>
        Public Shared Function AddPositionalEncoding(ByRef DccumentStr As List(Of WordVector)) As List(Of WordVector)
            ' Define the dimension of the model
            Dim d_model As Integer = 512
            ' Loop through each word in the sentence and apply positional encoding
            Dim i As Integer = 0
            For Each wrd In DccumentStr

                wrd.PositionalEncoding = CalcPositionalEncoding(i, d_model)
                i += 1
            Next
            Return DccumentStr
        End Function

        ''' <summary>
        ''' creates a list of word vectors sorted by frequency, from the text given
        ''' </summary>
        ''' <param name="Sentence"></param> document
        ''' <returns>vocabulary sorted in order of frequency</returns>
        Public Shared Function CreateSortedVocabulary(ByRef Sentence As String) As List(Of WordVector)
            Dim Vocabulary = WordVector.CreateVocabulary(Sentence)
            Dim NewDict As New List(Of WordVector)
            Dim Words() = Sentence.Split(" ")
            ' Count the frequency of each word
            Dim wordCounts As Dictionary(Of String, Integer) = Words.GroupBy(Function(w) w).ToDictionary(Function(g) g.Key, Function(g) g.Count())
            'Get the top ten words
            Dim TopTen As List(Of KeyValuePair(Of String, Integer)) = wordCounts.OrderByDescending(Function(w) w.Value).Take(10).ToList()

            Dim SortedDict As New List(Of WordVector)
            'Create Sorted List
            Dim Sorted As List(Of KeyValuePair(Of String, Integer)) = wordCounts.OrderByDescending(Function(w) w.Value).ToList()
            'Create Sorted Dictionary
            For Each item In Sorted

                Dim NewToken As New WordVector
                NewToken.Token = item.Key
                NewToken.SequenceEncoding = LookUpSeqEncoding(Vocabulary, item.Key)
                NewToken.Freq = item.Value
                SortedDict.Add(NewToken)

            Next

            Return SortedDict
        End Function

        ''' <summary>
        ''' Creates a unique list of words
        ''' Encodes words by their order of appearance in the text
        ''' </summary>
        ''' <param name="Sentence">document text</param>
        ''' <returns>EncodedWordlist (current vocabulary)</returns>
        Public Shared Function CreateVocabulary(ByRef Sentence As String) As List(Of WordVector)
            Dim inputString As String = "This is a sample sentence."
            If Sentence IsNot Nothing Then
                inputString = Sentence
            End If
            Dim uniqueCharacters As New List(Of String)

            Dim Dictionary As New List(Of WordVector)
            Dim Words() = Sentence.Split(" ")
            'Create unique tokens
            For Each c In Words
                If Not uniqueCharacters.Contains(c) Then
                    uniqueCharacters.Add(c)
                End If
            Next
            'Iterate through unique tokens assigning integers
            For i As Integer = 0 To uniqueCharacters.Count - 1
                'create token entry
                Dim newToken As New WordVector
                newToken.Token = uniqueCharacters(i)
                newToken.SequenceEncoding = i + 1
                'Add to vocab
                Dictionary.Add(newToken)

            Next
            Return UpdateVocabularyFrequencys(Sentence, Dictionary)
        End Function

        ''' <summary>
        ''' Creates embeddings for the sentence provided using the generated vocabulary
        ''' </summary>
        ''' <param name="Sentence"></param>
        ''' <param name="Vocabulary"></param>
        ''' <returns></returns>
        Public Shared Function EncodeWordsToVectors(ByRef Sentence As String, ByRef Vocabulary As List(Of WordVector)) As List(Of WordVector)

            Sentence = Sentence.ToLower
            If Vocabulary Is Nothing Then
                Vocabulary = CreateVocabulary(Sentence)
            End If
            Dim words() As String = Sentence.Split(" ")
            Dim Dict As New List(Of WordVector)
            For Each item In words
                Dim RetSent As New WordVector
                RetSent = GetToken(Vocabulary, item)
                Dict.Add(RetSent)
            Next
            Return NormalizeWords(Sentence, AddPositionalEncoding(Dict))
        End Function

        ''' <summary>
        ''' Decoder
        ''' </summary>
        ''' <param name="Vocabulary">Encoded Wordlist</param>
        ''' <param name="Token">desired token</param>
        ''' <returns></returns>
        Public Shared Function GetToken(ByRef Vocabulary As List(Of WordVector), ByRef Token As String) As WordVector
            For Each item In Vocabulary
                If item.Token = Token Then Return item
            Next
            Return New WordVector
        End Function

        ''' <summary>
        ''' finds the frequency of this token in the sentence
        ''' </summary>
        ''' <param name="Token">token to be defined</param>
        ''' <param name="InputStr">string containing token</param>
        ''' <returns></returns>
        Public Shared Function GetTokenFrequency(ByRef Token As String, ByRef InputStr As String) As Integer
            GetTokenFrequency = 0

            If InputStr.Contains(Token) = True Then
                For Each item In WordVector.GetWordFrequencys(InputStr, " ")
                    If item.Token = Token Then
                        GetTokenFrequency = item.Freq
                    End If
                Next
            End If
        End Function

        ''' <summary>
        ''' Returns frequencys for words
        ''' </summary>
        ''' <param name="_Text"></param>
        ''' <param name="Delimiter"></param>
        ''' <returns></returns>
        Public Shared Function GetTokenFrequencys(ByVal _Text As String, ByVal Delimiter As String) As List(Of WordVector)
            Dim Words As New WordVector
            Dim ListOfWordFrequecys As New List(Of WordVector)
            Dim WordList As List(Of String) = _Text.Split(Delimiter).ToList
            Dim groups = WordList.GroupBy(Function(value) value)
            For Each grp In groups
                Words.Token = grp(0)
                Words.Freq = grp.Count
                ListOfWordFrequecys.Add(Words)
            Next
            Return ListOfWordFrequecys
        End Function

        ''' <summary>
        ''' For Legacy Functionality
        ''' </summary>
        ''' <param name="_Text"></param>
        ''' <param name="Delimiter"></param>
        ''' <returns></returns>
        Public Shared Function GetWordFrequencys(ByVal _Text As String, ByVal Delimiter As String) As List(Of WordVector)
            GetTokenFrequencys(_Text, Delimiter)
        End Function

        ''' <summary>
        ''' Decoder - used to look up a token identity using its sequence encoding
        ''' </summary>
        ''' <param name="EncodedWordlist">Encoded VectorList(vocabulary)</param>
        ''' <param name="EncodingValue">Sequence Encoding Value</param>
        ''' <returns></returns>
        Public Shared Function LookUpBySeqEncoding(ByRef EncodedWordlist As List(Of WordVector), ByRef EncodingValue As Integer) As String
            For Each item In EncodedWordlist
                If item.SequenceEncoding = EncodingValue Then Return item.Token
            Next
            Return EncodingValue
        End Function

        ''' <summary>
        ''' Encoder - used to look up a tokens sequence encoding, in a vocabulary
        ''' </summary>
        ''' <param name="EncodedWordlist">Encoded VectorList(vocabulary) </param>
        ''' <param name="Token">Desired Token</param>
        ''' <returns></returns>
        Public Shared Function LookUpSeqEncoding(ByRef EncodedWordlist As List(Of WordVector), ByRef Token As String) As Integer
            For Each item In EncodedWordlist
                If item.Token = Token Then Return item.SequenceEncoding
            Next
            Return 0
        End Function

        ''' <summary>
        ''' Adds Normalization to Vocabulary(Word-based)
        ''' </summary>
        ''' <param name="Sentence">Doc</param>
        ''' <param name="dict">Vocabulary</param>
        ''' <returns></returns>
        Public Shared Function NormalizeWords(ByRef Sentence As String, ByRef dict As List(Of WordVector)) As List(Of WordVector)
            Dim Count = CountWords(Sentence)
            For Each item In dict
                item.NormalizedEncoding = Count / item.Freq
            Next
            Return dict
        End Function

        ''' <summary>
        ''' Encodes a list of word-vector by a list of strings
        ''' If a token is found in the list it is encoded with a binary 1 if false then 0
        ''' This is useful for categorizing and adding context to the word vector
        ''' </summary>
        ''' <param name="WordVectorList">list of tokens to be encoded (categorized)</param>
        ''' <param name="Vocabulary">Categorical List, Such as a list of positive sentiment</param>
        ''' <returns></returns>
        Public Shared Function OneShotEncoding(ByVal WordVectorList As List(Of WordVector),
                                ByRef Vocabulary As List(Of String)) As List(Of WordVector)
            Dim EncodedList As New List(Of WordVector)
            For Each item In WordVectorList
                Dim Found As Boolean = False
                For Each RefItem In Vocabulary
                    If item.Token = RefItem Then
                        Found = True
                    Else

                    End If
                Next
                If Found = True Then
                    Dim newWordvector As WordVector = item
                    newWordvector.OneHotEncoding = True
                End If
                EncodedList.Add(item)
            Next
            Return EncodedList
        End Function

        ''' <summary>
        ''' Creates a List of Bigram WordVectors Based on the text
        ''' to create the vocabulary file use @ProduceBigramVocabulary
        ''' </summary>
        ''' <param name="Sentence"></param>
        ''' <returns>Encoded list of bigrams and vectors (vocabulary)with frequencies </returns>
        Public Shared Function ProduceBigramDocument(ByRef sentence As String) As List(Of WordVector)

            ' Convert sentence to lowercase and split into words
            Dim words As String() = sentence.ToLower().Split()
            Dim GeneratedBigramsList As New List(Of String)
            Dim bigrams As New Dictionary(Of String, Integer)
            'We start at the first word And go up to the second-to-last word
            For i As Integer = 0 To words.Length - 2
                Dim bigram As String = words(i) & " " & words(i + 1)
                'We check If the bigrams dictionary already contains the bigram.
                'If it does, we increment its frequency by 1.
                'If it doesn't, we add it to the dictionary with a frequency of 1.
                GeneratedBigramsList.Add(bigram)
                If bigrams.ContainsKey(bigram) Then
                    bigrams(bigram) += 1
                Else
                    bigrams.Add(bigram, 1)
                End If
            Next

            'we Loop through the bigrams dictionary(of frequncies) And encode a integer to the bi-gram

            Dim bigramVocab As New List(Of WordVector)
            Dim a As Integer = 1
            For Each kvp As KeyValuePair(Of String, Integer) In bigrams
                Dim newvect As New WordVector
                newvect.Token = kvp.Key
                newvect.Freq = kvp.Value

                bigramVocab.Add(newvect)
            Next
            'create a list from the generated bigrams and
            ''add frequecies from vocabulary of frequecies
            Dim nVocab As New List(Of WordVector)
            Dim z As Integer = 0
            For Each item In GeneratedBigramsList
                'create final token
                Dim NewToken As New WordVector
                NewToken.Token = item
                'add current position in document
                NewToken.SequenceEncoding = GeneratedBigramsList(z)
                'add frequency
                For Each Lookupitem In bigramVocab
                    If item = Lookupitem.Token Then
                        NewToken.Freq = Lookupitem.Freq
                    Else
                    End If
                Next
                'add token
                nVocab.Add(NewToken)
                'update current index
                z += 1
            Next

            'Return bigram document with sequence and frequencys
            Return nVocab
        End Function

        ''' <summary>
        ''' Creates a Vocabulary of unique bigrams from sentence with frequencies adds a sequence vector based on
        ''' its appearence in the text, if item is repeated at multiple locations it is not reflected here
        ''' </summary>
        ''' <param name="Sentence"></param>
        ''' <returns>Encoded list of unique bigrams and vectors (vocabulary)with frequencies </returns>
        Public Shared Function ProduceBigramVocabulary(ByRef sentence As String) As List(Of WordVector)

            ' Convert sentence to lowercase and split into words
            Dim words As String() = sentence.ToLower().Split()
            Dim GeneratedBigramsList As New List(Of String)
            Dim bigrams As New Dictionary(Of String, Integer)
            'We start at the first word And go up to the second-to-last word
            For i As Integer = 0 To words.Length - 2
                Dim bigram As String = words(i) & " " & words(i + 1)
                'We check If the bigrams dictionary already contains the bigram.
                'If it does, we increment its frequency by 1.
                'If it doesn't, we add it to the dictionary with a frequency of 1.
                GeneratedBigramsList.Add(bigram)
                If bigrams.ContainsKey(bigram) Then
                    bigrams(bigram) += 1
                Else
                    bigrams.Add(bigram, 1)
                End If
            Next

            'we Loop through the bigrams dictionary(of frequncies) And encode a integer to the bi-gram

            Dim bigramVocab As New List(Of WordVector)
            Dim a As Integer = 0
            For Each kvp As KeyValuePair(Of String, Integer) In bigrams
                Dim newvect As New WordVector
                newvect.Token = kvp.Key
                newvect.Freq = kvp.Value
                newvect.SequenceEncoding = a + 1
                bigramVocab.Add(newvect)
            Next

            'Return bigram document with sequence and frequencys
            Return bigramVocab
        End Function

        ''' <summary>
        ''' Adds Frequencies to a sequentially encoded word-vector list
        ''' </summary>
        ''' <param name="Sentence">current document</param>
        ''' <param name="EncodedWordlist">Current Vocabulary</param>
        ''' <returns>an encoded word-Vector list with Frequencys attached</returns>
        Public Shared Function UpdateVocabularyFrequencys(ByRef Sentence As String, ByVal EncodedWordlist As List(Of WordVector)) As List(Of WordVector)

            Dim NewDict As New List(Of WordVector)
            Dim Words() = Sentence.Split(" ")
            ' Count the frequency of each word
            Dim wordCounts As Dictionary(Of String, Integer) = Words.GroupBy(Function(w) w).ToDictionary(Function(g) g.Key, Function(g) g.Count())
            'Get the top ten words
            Dim TopTen As List(Of KeyValuePair(Of String, Integer)) = wordCounts.OrderByDescending(Function(w) w.Value).Take(10).ToList()

            'Create Standard Dictionary
            For Each EncodedItem In EncodedWordlist
                For Each item In wordCounts
                    If EncodedItem.Token = item.Key Then
                        Dim NewToken As New WordVector
                        NewToken = EncodedItem
                        NewToken.Freq = item.Value
                        NewDict.Add(NewToken)
                    End If
                Next
            Next

            Return NewDict
        End Function

        ''' <summary>
        ''' Outputs Structure to Jason(JavaScriptSerializer)
        ''' </summary>
        ''' <returns></returns>
        Public Function ToJson() As String
            Dim Converter As New JavaScriptSerializer
            Return Converter.Serialize(Me)
        End Function



#Region "Positional Encoding"

        Private Shared Function CalcPositionalEncoding(ByVal position As Integer, ByVal d_model As Integer) As Double()
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

#End Region

#Region "Normalization"

        ''' <summary>
        ''' returns number of Chars in text
        ''' </summary>
        ''' <param name="Sentence">Document</param>
        ''' <returns>number of Chars</returns>
        Private Shared Function CountChars(ByRef Sentence As String) As Integer
            Dim uniqueCharacters As New List(Of String)
            For Each c As Char In Sentence
                uniqueCharacters.Add(c.ToString)
            Next
            Return uniqueCharacters.Count
        End Function

        ''' <summary>
        ''' Returns number of words in text
        ''' </summary>
        ''' <param name="Sentence">Document</param>
        ''' <returns>number of words</returns>
        Private Shared Function CountWords(ByRef Sentence As String) As Integer
            Dim Words() = Sentence.Split(" ")
            Return Words.Count
        End Function

#End Region

    End Structure



End Namespace

