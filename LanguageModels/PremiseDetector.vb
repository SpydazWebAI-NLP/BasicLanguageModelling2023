Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports LanguageModels.Recognition.Classifier.SentenceClassifier
Namespace Recognition
    Public Class RuleBasedEntityRecognizer
        Private Shared entityPatterns As Dictionary(Of String, String)

        ''' <summary>
        ''' Represents a captured word and its associated information.
        ''' </summary>
        Public Structure CapturedWord
            ''' <summary>
            ''' The captured word.
            ''' </summary>
            Public Property Word As String
            ''' <summary>
            ''' The list of preceding words.
            ''' </summary>
            Public Property PrecedingWords As List(Of String)
            ''' <summary>
            ''' The list of following words.
            ''' </summary>
            Public Property FollowingWords As List(Of String)
            ''' <summary>
            ''' The person associated with the word.
            ''' </summary>
            Public Property Person As String
            ''' <summary>
            ''' The location associated with the word.
            ''' </summary>
            Public Property Location As String
            ''' <summary>
            ''' The recognized entity.
            ''' </summary>
            Public Property Entity As String
            ''' <summary>
            ''' Indicates whether the word is recognized as an entity.
            ''' </summary>
            Public Property IsEntity As Boolean
            ''' <summary>
            ''' The entity type of the word.
            ''' </summary>
            Public Property EntityType As String
            ''' <summary>
            ''' The list of entity types associated with the word.
            ''' </summary>
            Public Property EntityTypes As List(Of String)
            ''' <summary>
            ''' Indicates whether the word is the focus term.
            ''' </summary>
            Public Property IsFocusTerm As Boolean
            ''' <summary>
            ''' Indicates whether the word is a preceding word.
            ''' </summary>
            Public Property IsPreceding As Boolean
            ''' <summary>
            ''' Indicates whether the word is a following word.
            ''' </summary>
            Public Property IsFollowing As Boolean
            ''' <summary>
            ''' The context words.
            ''' </summary>
            Public Property ContextWords As List(Of String)

            ''' <summary>
            ''' Initializes a new instance of the <see cref="CapturedWord"/> structure.
            ''' </summary>
            ''' <param name="word">The captured word.</param>
            ''' <param name="precedingWords">The list of preceding words.</param>
            ''' <param name="followingWords">The list of following words.</param>
            ''' <param name="person">The person associated with the word.</param>
            ''' <param name="location">The location associated with the word.</param>
            Public Sub New(ByVal word As String, ByVal precedingWords As List(Of String), ByVal followingWords As List(Of String), ByVal person As String, ByVal location As String)
                Me.Word = word
                Me.PrecedingWords = precedingWords
                Me.FollowingWords = followingWords
                Me.Person = person
                Me.Location = location
            End Sub
        End Structure
        Public Enum EntityPositionPrediction
            None
            Before
            After
        End Enum
        ''' <summary>
        ''' Performs a Sub-search within the given context words to recognize entities.
        ''' </summary>
        ''' <param name="contextWords">(applied After) The context words to search within.</param>
        ''' <param name="targetWord">The target word to recognize entities in.</param>
        ''' <returns>A list of captured words with entity information.</returns>
        Public Function PerformAfterSubSearch(ByVal contextWords As List(Of String), ByVal targetWord As String) As List(Of CapturedWord)
            Dim recognizedEntities As New List(Of CapturedWord)()
            Dim NewPat As String = targetWord
            For Each contextWord As String In contextWords

                NewPat &= " " & contextWord
                Dim entities As List(Of CapturedWord) = RecognizeEntities(contextWord & " " & targetWord)

                If entities.Count > 0 Then
                    recognizedEntities.AddRange(entities)
                End If
            Next

            Return recognizedEntities
        End Function
        ''' <summary>
        ''' Performs a subsearch within the given context words to recognize entities.
        ''' </summary>
        ''' <param name="contextWords">(Applied before) The context words to search within.</param>
        ''' <param name="targetWord">The target word to recognize entities in.</param>
        ''' <returns>A list of captured words with entity information.</returns>
        Public Function PerformBeforeSubSearch(ByVal contextWords As List(Of String), ByVal targetWord As String) As List(Of CapturedWord)
            Dim recognizedEntities As New List(Of CapturedWord)()
            Dim NewPat As String = targetWord
            For Each contextWord As String In contextWords

                NewPat = contextWord & " " & NewPat
                Dim entities As List(Of CapturedWord) = RecognizeEntities(contextWord & " " & targetWord)

                If entities.Count > 0 Then
                    recognizedEntities.AddRange(entities)
                End If
            Next

            Return recognizedEntities
        End Function

        Public Shared Sub Main()
            Dim recognizer As New RuleBasedEntityRecognizer()

            ' Configure entity patterns
            recognizer.ConfigureEntityPatterns()

            ' Example input text
            Dim inputText As String = "John went to the store and met Mary."

            ' Capture words with entity context
            Dim capturedWords As List(Of RuleBasedEntityRecognizer.CapturedWord) = recognizer.CaptureWordsWithEntityContext(inputText, "store", 2, 2)

            ' Display captured words and their entity information
            For Each capturedWord As RuleBasedEntityRecognizer.CapturedWord In capturedWords
                Console.WriteLine("Word: " & capturedWord.Word)
                Console.WriteLine("Is Entity: " & capturedWord.IsEntity)
                Console.WriteLine("Entity Types: " & String.Join(", ", capturedWord.EntityTypes))
                Console.WriteLine("Is Focus Term: " & capturedWord.IsFocusTerm)
                Console.WriteLine("Is Preceding: " & capturedWord.IsPreceding)
                Console.WriteLine("Is Following: " & capturedWord.IsFollowing)
                Console.WriteLine("Context Words: " & String.Join(" ", capturedWord.ContextWords))
                Console.WriteLine()
            Next

            Console.ReadLine()
        End Sub
        ''' <summary>
        ''' Configures the entity patterns by adding them to the recognizer.
        ''' </summary>
        Public Sub ConfigureEntityPatterns()
            ' Define entity patterns
            Me.AddEntityPattern("Person", "John|Mary|David")
            Me.AddEntityPattern("Location", "store|office|park")
            ' Add more entity patterns as needed
        End Sub

        ''' <summary>
        ''' Gets the entity types associated with a given word.
        ''' </summary>
        ''' <param name="word">The word to get entity types for.</param>
        ''' <returns>A list of entity types associated with the word.</returns>
        Public Function GetEntityTypes(ByVal word As String) As List(Of String)
            Dim recognizedEntities As List(Of CapturedWord) = RuleBasedEntityRecognizer.RecognizeEntities(word)
            Return recognizedEntities.Select(Function(entity) entity.EntityType).ToList()
        End Function

        ''' <summary>
        ''' Captures words with their context based on a focus term and the number of preceding and following words to include.
        ''' </summary>
        ''' <param name="text">The input text.</param>
        ''' <param name="focusTerm">The focus term to capture.</param>
        ''' <param name="precedingWordsCount">The number of preceding words to capture.</param>
        ''' <param name="followingWordsCount">The number of following words to capture.</param>
        ''' <returns>A list of WordWithContext objects containing captured words and their context information.</returns>
        Public Function CaptureWordsWithEntityContext(ByVal text As String, ByVal focusTerm As String, ByVal precedingWordsCount As Integer, ByVal followingWordsCount As Integer) As List(Of CapturedWord)
            Dim words As List(Of String) = text.Split(" "c).ToList()
            Dim focusIndex As Integer = words.IndexOf(focusTerm)

            Dim capturedWordsWithEntityContext As New List(Of CapturedWord)()

            If focusIndex <> -1 Then
                Dim startIndex As Integer = Math.Max(0, focusIndex - precedingWordsCount)
                Dim endIndex As Integer = Math.Min(words.Count - 1, focusIndex + followingWordsCount)

                Dim contextWords As List(Of String) = words.GetRange(startIndex, endIndex - startIndex + 1)

                Dim prediction As EntityPositionPrediction = PredictEntityPosition(contextWords, focusTerm)

                For i As Integer = startIndex To endIndex
                    Dim word As String = words(i)

                    Dim entityTypes As List(Of String) = GetEntityTypes(word)

                    If entityTypes.Count = 0 AndAlso prediction <> EntityPositionPrediction.None Then
                        Dim isLowConfidenceEntity As Boolean = (prediction = EntityPositionPrediction.After AndAlso i > focusIndex) OrElse
                                                           (prediction = EntityPositionPrediction.Before AndAlso i < focusIndex)

                        If isLowConfidenceEntity Then
                            entityTypes.Add("Low Confidence Entity")
                        End If
                    End If

                    Dim wordWithContext As New CapturedWord() With {
                    .Word = word,
                    .IsEntity = entityTypes.Count > 0,
                    .EntityTypes = entityTypes,
                    .IsFocusTerm = (i = focusIndex),
                    .IsPreceding = (i < focusIndex),
                    .IsFollowing = (i > focusIndex),
                    .ContextWords = contextWords
                }

                    capturedWordsWithEntityContext.Add(wordWithContext)
                Next
            End If

            Return capturedWordsWithEntityContext
        End Function

        ''' <summary>
        ''' Predicts the position of an entity relative to the focus term within the context words.
        ''' </summary>
        ''' <param name="contextWords">The context words.</param>
        ''' <param name="focusTerm">The focus term.</param>
        ''' <returns>The predicted entity position.</returns>
        Public Function PredictEntityPosition(ByVal contextWords As List(Of String), ByVal focusTerm As String) As EntityPositionPrediction
            Dim termIndex As Integer = contextWords.IndexOf(focusTerm)

            If termIndex >= 0 Then
                If termIndex < contextWords.Count - 1 Then
                    Return EntityPositionPrediction.After
                ElseIf termIndex > 0 Then
                    Return EntityPositionPrediction.Before
                End If
            End If

            Return EntityPositionPrediction.None
        End Function

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RuleBasedEntityRecognizer"/> class.
        ''' </summary>
        Public Sub New()
            entityPatterns = New Dictionary(Of String, String)()
        End Sub

        ''' <summary>
        ''' Adds an entity pattern to the recognizer.
        ''' </summary>
        ''' <param name="entityType">The entity type.</param>
        ''' <param name="pattern">The regular expression pattern.</param>
        Public Sub AddEntityPattern(ByVal entityType As String, ByVal pattern As String)
            entityPatterns.Add(entityType, pattern)
        End Sub

        ''' <summary>
        ''' Recognizes entities in the given text.
        ''' </summary>
        ''' <param name="text">The text to recognize entities in.</param>
        ''' <returns>A list of captured words with entity information.</returns>
        Public Shared Function RecognizeEntities(ByVal text As String) As List(Of CapturedWord)
            Dim capturedEntities As New List(Of CapturedWord)()

            For Each entityType As String In entityPatterns.Keys
                Dim pattern As String = entityPatterns(entityType)
                Dim matches As MatchCollection = Regex.Matches(text, pattern)

                For Each match As Match In matches
                    capturedEntities.Add(New CapturedWord() With {
                    .Entity = match.Value,
                    .EntityType = entityType
                })
                Next
            Next

            Return capturedEntities
        End Function
    End Class
    Namespace Classifier


        Public Class LogicalDependencyClassifier
            Private Shared ReadOnly CauseAndEffectPattern As Regex = New Regex("(?i)(cause|effect|result in|lead to|because|due to|consequently)")
            Private Shared ReadOnly ComparisonPattern As Regex = New Regex("(?i)(compared to|greater than|less than|similar to|different from|between)")
            Private Shared ReadOnly ConditionPattern As Regex = New Regex("(?i)(if|unless|only if|when|provided that|in the case of)")
            Private Shared ReadOnly GeneralizationPattern As Regex = New Regex("(?i)(all|every|always|none|never|in general|could|would|maybe|Is a)")
            Private Shared ReadOnly TemporalSequencePattern As Regex = New Regex("(?i)(before|after|during|while|subsequently|previously|simultaneously|when|at the time of|next)")

            Public Shared CauseEntityList As String() = {"cause", "reason", "factor", "based on", "indicates", "lead to", "due to", "consequently", "because", "was provided"}
            Public Shared EffectEntityList As String() = {"effect", "result", "outcome", "was the consequence of", "end process of", "because of", "reason for"}
            Public Shared ComparableObject1EntityList As String() = {"first object", "object A"}
            Public Shared ComparableObject2EntityList As String() = {"second object", "object B"}
            Public Shared ConditionEntityList As String() = {"condition", "requirement", "prerequisite", "if", "when", "then", "but", "And", "Not", "Or", "less than", "greater than"}
            Public Shared GeneralizedObjectEntityList As String() = {"generalized object", "common element", "universal attribute"}
            Public Shared Event1EntityList As String() = {"first event", "event A"}
            Public Shared Event2EntityList As String() = {"second event", "event B"}

            Public Shared ReadOnly DependencyPatterns As Dictionary(Of String, List(Of Regex)) = New Dictionary(Of String, List(Of Regex)) From {
                {"Causal Dependency", New List(Of Regex) From {
                    New Regex(".*Is\s+the\s+cause\s+of\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*leads\s+to\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*causes\s+.*", RegexOptions.IgnoreCase),
                    CauseAndEffectPattern
                }},
                {"Comparison Dependency", New List(Of Regex) From {
                    ComparisonPattern
                }},
                {"Conditional Dependency", New List(Of Regex) From {
                    ConditionPattern
                }},
                {"Generalization Dependency", New List(Of Regex) From {
                    GeneralizationPattern
                }},
                {"Temporal Sequence Dependency", New List(Of Regex) From {
                    TemporalSequencePattern
                }},
                {"Premise", New List(Of Regex) From {
                    New Regex(".*infers\s+that\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+deduced\s+from\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*drawn\s+from\s+.*", RegexOptions.IgnoreCase),
                    New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*would\s+have\s+occurred\s+if\s+.*", RegexOptions.IgnoreCase),
                    New Regex("Based\s+on\s+statistics,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("According\s+to\s+the\s+survey,\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+similar\s+to\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+analogous\s+to\s+.*", RegexOptions.IgnoreCase),
                    New Regex("For\s+example,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("In\s+support\s+of\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+backed\s+by\s+.*", RegexOptions.IgnoreCase),
                    New Regex("In\s+general,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("Typically,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("Most\s+of\s+the\s+time,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*relies\s+on\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+the\s+cause\s+of\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*leads\s+to\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*causes\s+.*", RegexOptions.IgnoreCase),
                    New Regex("In\s+fact,\s+.*", RegexOptions.IgnoreCase),
                    New Regex("Indeed,\s+.*", RegexOptions.IgnoreCase),
                    New Regex(".*Is\s+a\s+fact\s+that\s+.*", RegexOptions.IgnoreCase),
                    CauseAndEffectPattern,
                    ComparisonPattern,
                    ConditionPattern,
                    GeneralizationPattern,
                    TemporalSequencePattern
                }}
            }

            Public Function DetectLogicalDependancyType(ByVal premise As String) As String
                For Each premisePattern In DependencyPatterns
                    If premisePattern.Value.Any(Function(indicatorRegex) indicatorRegex.IsMatch(premise)) Then
                        Return premisePattern.Key
                    End If
                Next

                Return "Unknown"
            End Function

            Public Shared Function ClassifyLogicalDependency(statement As String) As String
                If CauseAndEffectPattern.IsMatch(statement) Then
                    Return "Cause And Effect"
                ElseIf ComparisonPattern.IsMatch(statement) Then
                    Return "Comparison"
                ElseIf ConditionPattern.IsMatch(statement) Then
                    Return "Condition"
                ElseIf GeneralizationPattern.IsMatch(statement) Then
                    Return "Generalization"
                ElseIf TemporalSequencePattern.IsMatch(statement) Then
                    Return "Temporal Sequence"
                Else
                    Return "Unknown"
                End If
            End Function

        End Class

        Public Class SentenceClassifier
            Public Structure ClassificationRule
                Public Property Type As String
                Public Property Subtype As String
                Public Property Relationship As String
                Public Property Patterns As List(Of Regex)
            End Structure
            Public Structure CapturedType

                Public Property Sentence As String
                Public Property LogicalRelation_ As String
                Public Property SubType As String
            End Structure
            Public Structure ClassifiedSentence
                Public Classified As Boolean
                Public Type As String
                Public Entity As CapturedType
            End Structure

            Private ReadOnly ClassificationRules As List(Of ClassificationRule)
            Public Shared Function IsPremise(ByVal sentence As String) As Boolean
                ' List of indicator phrases for premises
                Dim indicatorPhrases As New List(Of String) From {"premise"}
                If PremiseDetector.DetectPremise(sentence) <> "" Then Return True
                For Each phrase As String In indicatorPhrases
                    ' Match the phrase at the beginning of the sentence
                    Dim match As Match = Regex.Match(sentence, "^\s*" + phrase + ":", RegexOptions.IgnoreCase)
                    If match.Success Then
                        Return True
                    End If
                Next
                If PremiseDetector.ClassifySentence(sentence) = "Unknown" Then
                    Return False
                Else
                    Return True
                End If
                Return False
            End Function
            Public Shared Function IsConclusion(ByVal sentence As String) As Boolean
                ' List of indicator phrases for hypotheses
                Dim indicatorPhrases As New List(Of String) From {"Conclusion"}
                If ConclusionDetector.DetectConclusion(sentence) <> "" Then Return True
                For Each phrase As String In indicatorPhrases
                    ' Match the phrase at the beginning of the sentence
                    Dim match As Match = Regex.Match(sentence, "^\s*" + phrase + ":", RegexOptions.IgnoreCase)
                    If match.Success Then
                        Return True
                    End If
                Next
                If ConclusionDetector.ClassifySentence(sentence) = "Unclassified" Then
                    Return False
                Else
                    Return True
                End If
                Return False
            End Function
            Public Shared Function IsHypothesis(ByVal sentence As String) As Boolean
                ' List of indicator phrases for hypotheses
                Dim indicatorPhrases As New List(Of String) From {"hypothesis"}
                If HypothesisDetector.DetectHypothesis(sentence) <> "" Then Return True
                For Each phrase As String In indicatorPhrases
                    ' Match the phrase at the beginning of the sentence
                    Dim match As Match = Regex.Match(sentence, "^\s*" + phrase + ":", RegexOptions.IgnoreCase)
                    If match.Success Then
                        Return True
                    End If
                Next
                If HypothesisDetector.ClassifySentence(sentence) = "Unclassified" Then
                    Return False
                Else
                    Return True
                End If
                Return False
            End Function
            Public Shared Function IsQuestion(ByVal sentence As String) As Boolean
                ' List of indicator phrases for hypotheses
                Dim indicatorPhrases As New List(Of String) From {"Question"}
                If QuestionDetector.ClassifySentence(sentence) <> "Unknown" Then Return True
                For Each phrase As String In indicatorPhrases
                    ' Match the phrase at the beginning of the sentence
                    Dim match As Match = Regex.Match(sentence, "^\s*" + phrase + ":", RegexOptions.IgnoreCase)
                    If match.Success Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Shared Function ClassifySentences(ByRef Sentences As List(Of String)) As List(Of ClassifiedSentence)
                Dim lst As New List(Of ClassifiedSentence)
                For Each item In Sentences
                    Dim classified As New ClassifiedSentence
                    classified.Classified = False
                    Dim Captured As New CapturedType
                    If IsPremise(item) = True Then
                        If PremiseDetector.DetectPremise(item) <> "" Then

                            classified.Type = "Premise"
                            classified.Entity = PremiseDetector.GetSentence(item)
                            classified.Classified = True
                            lst.Add(classified)
                        Else
                            'Captured = New CapturedType
                            'Captured.Sentence = item
                            'Captured.SubType = PremiseDetector.ExtractPremiseSubtype(item)
                            'Captured.LogicalRelation_ = PremiseDetector.ClassifySentence(item)
                            'classified.Entity = Captured
                            'classified.Type = "Premise"
                            'classified.Classified = True
                            'lst.Add(classified)
                        End If
                    Else
                    End If



                    If IsHypothesis(item) = True Then
                        If HypothesisDetector.DetectHypothesis(item) <> "" Then

                            classified.Type = "Hypotheses"
                            classified.Entity = HypothesisDetector.GetSentence(item)
                            classified.Classified = True
                            lst.Add(classified)
                        Else
                            'Captured = New CapturedType
                            'Captured.Sentence = item
                            'Captured.SubType = HypothesisDetector.ClassifyHypothesis(item)
                            'Captured.LogicalRelation_ = HypothesisDetector.ClassifySentence(item)
                            'classified.Entity = Captured
                            'classified.Type = "Hypotheses"
                            'classified.Classified = True
                            'lst.Add(classified)
                        End If
                    Else
                    End If


                    If IsConclusion(item) = True Then
                        If ConclusionDetector.DetectConclusion(item) <> "" Then

                            classified.Type = "Conclusion"
                            classified.Entity = ConclusionDetector.GetSentence(item)
                            classified.Classified = True
                            lst.Add(classified)
                        Else
                            'Captured = New CapturedType
                            'Captured.Sentence = item
                            'Captured.SubType = ConclusionDetector.ClassifyConclusion(item)
                            'Captured.LogicalRelation_ = ConclusionDetector.ClassifySentence(item)
                            'classified.Entity = Captured
                            'classified.Type = "Conclusion"
                            'classified.Classified = True
                            'lst.Add(classified)
                        End If
                    Else
                    End If

                    If IsQuestion(item) = True Then
                        If QuestionDetector.ClassifySentence(item) <> "Unclassified" Then

                            classified.Type = "Question"
                            classified.Entity = QuestionDetector.GetSentence(item)
                            classified.Classified = True
                            lst.Add(classified)
                        Else
                            'Captured = New CapturedType
                            'Captured.Sentence = item
                            'Captured.SubType = PremiseDetector.ExtractPremiseSubtype(item)
                            'Captured.LogicalRelation_ = PremiseDetector.ClassifySentence(item)
                            'classified.Entity = Captured
                            'classified.Type = "Question"
                            'classified.Classified = True
                            'lst.Add(classified)
                        End If
                    Else
                    End If

                    'Else
                    If classified.Classified = False Then

                        classified.Type = "Unknown Classification"
                        Captured = New CapturedType
                        Captured.Sentence = item
                        Captured.LogicalRelation_ = LogicalDependencyClassifier.ClassifyLogicalDependency(item)
                        Captured.SubType = LogicalDependencyClassifier.ClassifyLogicalDependency(item)
                        classified.Entity = Captured
                        lst.Add(classified)


                    End If


                Next

                Return lst.Distinct.ToList
            End Function

            Public Shared Sub Main()
                Dim documents As List(Of String) = iGetTrainingSentences()
                Console.WriteLine("Docs :" & documents.Count)
                Dim lst As List(Of ClassifiedSentence) = ClassifySentences(documents)
                Console.WriteLine("Premises")
                Console.WriteLine()
                Dim Count As Integer = 0
                For Each item As ClassifiedSentence In lst
                    If item.Type = "Premise" Then
                        Count += 1
                        Console.WriteLine(Count & ":")

                        Console.WriteLine($"Sentence: {item.Entity.Sentence}")
                        Console.WriteLine("Classification: " & item.Type)
                        Console.WriteLine("Sub Type: " & item.Entity.SubType)
                        Console.WriteLine("Logical Relation Type: " & item.Entity.LogicalRelation_)
                        Console.WriteLine()
                    Else
                    End If

                Next
                Console.WriteLine()
                Console.WriteLine()
                Console.WriteLine("Hypotheses")
                Console.WriteLine()
                Count = 0
                For Each item As ClassifiedSentence In lst
                    If item.Type = "Hypotheses" Then
                        Count += 1
                        Console.WriteLine(Count & ":")
                        Console.WriteLine($"Sentence: {item.Entity.Sentence}")
                        Console.WriteLine("Classification: " & item.Type)
                        Console.WriteLine("Sub Type: " & item.Entity.SubType)
                        Console.WriteLine("Logical Relation Type: " & item.Entity.LogicalRelation_)
                        Console.WriteLine()
                    Else
                    End If

                Next

                Console.WriteLine()
                Console.WriteLine()
                Console.WriteLine("Conclusions")
                Console.WriteLine()
                Count = 0
                For Each item As ClassifiedSentence In lst
                    If item.Type = "Conclusion" Then
                        Count += 1
                        Console.WriteLine(Count & ":")
                        Console.WriteLine($"Sentence: {item.Entity.Sentence}")
                        Console.WriteLine("Classification: " & item.Type)
                        Console.WriteLine("Sub Type: " & item.Entity.SubType)
                        Console.WriteLine("Logical Relation Type: " & item.Entity.LogicalRelation_)
                        Console.WriteLine()
                    Else
                    End If

                Next

                Console.WriteLine()
                Console.WriteLine()
                Console.WriteLine("Questions")
                Console.WriteLine()
                Count = 0
                For Each item As ClassifiedSentence In lst
                    If item.Type = "Question" Then
                        Count += 1
                        Console.WriteLine(Count & ":")
                        Console.WriteLine($"Sentence: {item.Entity.Sentence}")
                        Console.WriteLine("Classification: " & item.Type)
                        Console.WriteLine("Sub Type: " & item.Entity.SubType)
                        Console.WriteLine("Logical Relation Type: " & item.Entity.LogicalRelation_)
                        Console.WriteLine()
                    Else
                    End If

                Next





                Count = 0
                Console.WriteLine()
                Console.WriteLine()
                Console.WriteLine("Unclassified")
                Console.WriteLine()
                For Each item As ClassifiedSentence In lst
                    If item.Type = "Unknown Classification" Or item.Entity.SubType = "Unknown" Or item.Entity.LogicalRelation_ = "Unknown" Then
                        Count += 1
                        Console.WriteLine(Count & ":")
                        Console.WriteLine($"Sentence: {item.Entity.Sentence}")
                        Console.WriteLine("Classification: " & item.Type)
                        Console.WriteLine("Sub Type: " & item.Entity.SubType)
                        Console.WriteLine("Logical Relation Type: " & item.Entity.LogicalRelation_)
                        Console.WriteLine()
                    Else
                    End If

                Next
                Console.ReadLine()

            End Sub
            Public Sub New()

                ClassificationRules = New List(Of ClassificationRule)

            End Sub
            Public Shared Function InitializeClassificationRules() As List(Of ClassificationRule)



                ' Add your existing classification rules here
                Dim ClassificationRules As New List(Of ClassificationRule)






                ' Inductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "Inductive Reasoning",
            .Relationship = "Supports",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bBased on\b", RegexOptions.IgnoreCase),
                New Regex("^\bObserving\b", RegexOptions.IgnoreCase),
                New Regex("^\bEmpirical evidence suggests\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Inductive Reasoning",
            .Relationship = "Inferred From",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore, it can be inferred\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt is likely that\b", RegexOptions.IgnoreCase),
                New Regex("^\bGeneralizing from the evidence\b", RegexOptions.IgnoreCase)
            }
        })

                ' Deductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "Deductive Reasoning",
            .Relationship = "Supports",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bGiven that\b", RegexOptions.IgnoreCase),
                New Regex("^\bIf\b", RegexOptions.IgnoreCase),
                New Regex("^\bAssuming\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Deductive Reasoning",
            .Relationship = "Follows From",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt follows that\b", RegexOptions.IgnoreCase),
                New Regex("^\bSo\b", RegexOptions.IgnoreCase)
            }
        })

                ' Abductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Hypothesis",
            .Subtype = "Abductive Reasoning",
            .Relationship = "Supports",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bIt is possible that\b", RegexOptions.IgnoreCase),
                New Regex("^\bTo explain the observation\b", RegexOptions.IgnoreCase),
                New Regex("^\bSuggesting a likely explanation\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Abductive Reasoning",
            .Relationship = "Explains",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore, the best explanation is\b", RegexOptions.IgnoreCase),
                New Regex("^\bThe most plausible conclusion is\b", RegexOptions.IgnoreCase),
                New Regex("^\bThe evidence supports the hypothesis that\b", RegexOptions.IgnoreCase)
            }
        })








                ' Straw Man Argument Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "FallacyPremise ",
            .Subtype = "Straw Man Argument",
            .Relationship = "Incorrect truth",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bMisrepresenting\b", RegexOptions.IgnoreCase),
                New Regex("^\bExaggerating\b", RegexOptions.IgnoreCase),
                New Regex("^\bDistorting\b", RegexOptions.IgnoreCase)
            }
        })

                ' Fallacy Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Fallacy Premise",
            .Subtype = "deductive",
            .Relationship = "Circular argument",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bAd Hominem\b", RegexOptions.IgnoreCase),
                New Regex("^\bCircular Reasoning\b", RegexOptions.IgnoreCase),
                New Regex("^\bFalse Cause\b", RegexOptions.IgnoreCase)
            }
        })




                ' Inductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "Inductive Reasoning",
            .Relationship = "Inductive",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bBased on\b", RegexOptions.IgnoreCase),
                New Regex("^\bObserving\b", RegexOptions.IgnoreCase),
                New Regex("^\bEmpirical evidence suggests\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Inductive Reasoning",
            .Relationship = "Inductive",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore, it can be inferred\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt is likely that\b", RegexOptions.IgnoreCase),
                New Regex("^\bGeneralizing from the evidence\b", RegexOptions.IgnoreCase)
            }
        })

                ' Deductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "Deductive Reasoning",
            .Relationship = "Deductive Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bGiven that\b", RegexOptions.IgnoreCase),
                New Regex("^\bIf\b", RegexOptions.IgnoreCase),
                New Regex("^\bAssuming\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Deductive Reasoning",
            .Relationship = "Follow Up Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt follows that\b", RegexOptions.IgnoreCase),
                New Regex("^\bSo\b", RegexOptions.IgnoreCase)
            }
        })

                ' Abductive Reasoning Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Hypothesis",
            .Subtype = "Abductive Reasoning",
            .Relationship = "Possibility Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bIt is possible that\b", RegexOptions.IgnoreCase),
                New Regex("^\bTo explain the observation\b", RegexOptions.IgnoreCase),
                New Regex("^\bSuggesting a likely explanation\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "Abductive Reasoning",
            .Relationship = "Supporting Evidence Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore, the best explanation is\b", RegexOptions.IgnoreCase),
                New Regex("^\bThe most plausible conclusion is\b", RegexOptions.IgnoreCase),
                New Regex("^\bThe evidence supports the hypothesis that\b", RegexOptions.IgnoreCase)
            }
        })



                ' Question Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Question",
            .Subtype = "General",
            .Relationship = "General Question",
            .Patterns = New List(Of Regex)() From {
                New Regex("^(?:What|Who|Where|When|Why|How)\b", RegexOptions.IgnoreCase),
                New Regex("^Is\b", RegexOptions.IgnoreCase),
                New Regex("^\bCan\b", RegexOptions.IgnoreCase),
                New Regex("^\bAre\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Question",
            .Subtype = "Comparison",
            .Relationship = "Compare Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^(?:Which|What|Who)\b.*\b(is|are)\b.*\b(?:better|worse|superior|inferior|more|less)\b", RegexOptions.IgnoreCase),
                New Regex("^(?:How|In what way)\b.*\b(?:different|similar|alike)\b", RegexOptions.IgnoreCase),
                New Regex("^(?:Compare|Contrast)\b", RegexOptions.IgnoreCase)
            }
        })

                ' Answer Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Answer",
            .Subtype = "General",
            .Relationship = "Confirm/Deny",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bYes\b", RegexOptions.IgnoreCase),
                New Regex("^\bNo\b", RegexOptions.IgnoreCase),
                New Regex("^\bMaybe\b", RegexOptions.IgnoreCase),
                New Regex("^\bI don't know\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Answer",
            .Subtype = "Comparison",
            .Relationship = "Compare Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bA is\b.*\b(?:better|worse|superior|inferior|more|less)\b", RegexOptions.IgnoreCase),
                New Regex("^\bA is\b.*\b(?:different|similar|alike)\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt depends\b", RegexOptions.IgnoreCase),
                New Regex("^\bBoth\b", RegexOptions.IgnoreCase)
            }
        })

                ' Hypothesis Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Hypothesis",
            .Subtype = "General",
            .Relationship = "Hypothesize",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bIf\b", RegexOptions.IgnoreCase),
                New Regex("^\bAssuming\b", RegexOptions.IgnoreCase),
                New Regex("^\bSuppose\b", RegexOptions.IgnoreCase),
                New Regex("^\bHypothesize\b", RegexOptions.IgnoreCase)
            }
        })

                ' Conclusion Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "General",
            .Relationship = "Follow On Conclusion",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore\b", RegexOptions.IgnoreCase),
                New Regex("^\bThus\b", RegexOptions.IgnoreCase),
                New Regex("^\bHence\b", RegexOptions.IgnoreCase),
                New Regex("^\bConsequently\b", RegexOptions.IgnoreCase)
            }
        })

                ' Premise Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "General",
            .Relationship = "Reason",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bBecause\b", RegexOptions.IgnoreCase),
                New Regex("^\bSince\b", RegexOptions.IgnoreCase),
                New Regex("^\bGiven that\b", RegexOptions.IgnoreCase),
                New Regex("^\bConsidering\b", RegexOptions.IgnoreCase)
            }
        })
                ' Add new classification rules
                ClassificationRules.Add(New ClassificationRule With {
                .Type = "Dependency",
                .Subtype = "Cause and Effect",
                .Patterns = New List(Of Regex)() From {
                    New Regex("(?i)(cause|effect|result in|lead to|because|due to|consequently)")
                }
            })

                ClassificationRules.Add(New ClassificationRule With {
                .Type = "Dependency",
                .Subtype = "Comparison",
                .Patterns = New List(Of Regex)() From {
                    New Regex("(?i)(compared to|greater than|less than|similar to|different from)")
                }
            })

                ClassificationRules.Add(New ClassificationRule With {
                .Type = "Dependency",
                .Subtype = "Condition",
                .Patterns = New List(Of Regex)() From {
                    New Regex("(?i)(if|unless|only if|when|provided that|in the case of)")
                }
            })

                ClassificationRules.Add(New ClassificationRule With {
                .Type = "Dependency",
                .Subtype = "Generalization",
                .Patterns = New List(Of Regex)() From {
                    New Regex("(?i)(all|every|always|none|never|in general)")
                }
            })

                ClassificationRules.Add(New ClassificationRule With {
                .Type = "Dependency",
                .Subtype = "Temporal Sequence",
                .Patterns = New List(Of Regex)() From {
                    New Regex("(?i)(before|after|during|while|subsequently|previously|simultaneously)")
                }
            })
                ' Add new classification rules
                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Dependency",
                    .Subtype = "Control Flow",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(if the|as long as|when)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Dependency",
                    .Subtype = "Function Call",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to calculate|to display|to show|to invoke|to utilize|to get)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Code Task",
                    .Subtype = "Variable Assignment",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(store the|assign the|set the)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "System Task",
                    .Subtype = "File Manipulation",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to read|to write|to extract|to save|to open|to close)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Code Task",
                    .Subtype = "Exception Handling",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(wrap the|enclose the|employ a)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Code Task",
                    .Subtype = "Object-Oriented Programming",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to create|to access|to instantiate|to retrieve)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Tokenization",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to split|to tokenize|use a tokenizer)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Lemmatization",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to apply|to convert|use a lemmatizer)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Named Entity Recognition (NER)",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to apply|to detect|use a named entity recognizer)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Sentiment Analysis",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to perform|to analyze|use a sentiment classifier)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Text Classification",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to classify|to perform|use a text classifier)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Language Translation",
                    .Patterns = New List(Of Regex)() From {
                New Regex("(?i)(to translate|to perform|use a machine translation model)")
            }
        })

                ClassificationRules.Add(New ClassificationRule With {
            .Type = "Task",
            .Subtype = "Text Summarization",
            .Patterns = New List(Of Regex)() From {
                New Regex("(?i)(to generate|to perform|use a text summarizer)")
            }
        })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Word Embeddings",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to represent|to perform|use word embeddings)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Text Similarity",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to measure|to determine|use a text similarity metric)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Part-of-Speech Tagging",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to assign|to perform|use a part-of-speech tagger)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Dependency Parsing",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to analyze|to perform|perform dependency parsing)")
                    }
                })

                ClassificationRules.Add(New ClassificationRule With {
                    .Type = "Task",
                    .Subtype = "Topic Modeling",
                    .Patterns = New List(Of Regex)() From {
                        New Regex("(?i)(to identify|to perform|use topic modeling)")
                    }
                })
                Return ClassificationRules
            End Function

            Public Class HypothesisDetector

                Private Shared Function GetEntitys() As String()
                    Return {"Effect", "relationship", "influence", "impact", "difference", "association", "correlation", "effectiveness.", "Significant", "statistically significant", "noticeable", "measurable", "Increase", "decrease", "positive", "negative", "Difference between", "change in", "effect on", "effect of,", "relationship between."}
                End Function

                Private Shared HypothesisPatterns As Dictionary(Of String, String()) = GetInternalHypothesisPatterns()

                Private Shared HypothesisIndicators As String() = {"Hypothesis", "Assumption", "theory", "in theory", "in practice",
                "proposal", "proposes", "it can be proposed", "Supposition", "supposedly", "supposes", "conjecture", "connects",
                "concludes", "follows that", "in light of", "in reflection", "reflects", "statistical", "strong relationship", "correlation", "exactly"}


                Public Shared Function GetSentence(document As String) As CapturedType



                    Dim hypotheses As String = DetectHypothesis(document)


                    Dim classification As String = ClassifyHypothesis(hypotheses)
                    Dim logicalRelationship As String = ClassifySentence(hypotheses)

                    Dim hypothesisStorage As New CapturedType With {
                        .Sentence = hypotheses,
                        .LogicalRelation_ = logicalRelationship,
                        .SubType = classification
                    }




                    Return hypothesisStorage
                End Function


                Public Shared Function GetSentences(documents As List(Of String)) As List(Of CapturedType)
                    Dim storage As New List(Of CapturedType)

                    For Each document As String In documents
                        Dim hypotheses As List(Of String) = DetectHypotheses(document)

                        For Each hypothesis As String In hypotheses
                            Dim classification As String = ClassifyHypothesis(hypothesis)
                            Dim logicalRelationship As String = ClassifySentence(hypothesis)

                            Dim hypothesisStorage As New CapturedType With {
                        .Sentence = hypothesis,
                        .SubType = classification,
                        .LogicalRelation_ = logicalRelationship
                    }
                            storage.Add(hypothesisStorage)
                        Next
                    Next

                    Return storage.Distinct.ToList
                End Function
                Public Shared Function DetectHypotheses(document As String, HypothesisPatterns As Dictionary(Of String, String())) As List(Of String)
                    Dim hypotheses As New List(Of String)

                    Dim sentences As String() = document.Split("."c)
                    For Each sentence As String In sentences
                        sentence = sentence.Trim().ToLower

                        ' Check if the sentence contains any indicator terms
                        If sentence.ContainsAny(HypothesisIndicators) Then
                            hypotheses.Add(sentence)
                        End If
                        If sentence.ContainsAny(GetEntitys) Then
                            hypotheses.Add(sentence)
                        End If

                        For Each hypothesesPattern In HypothesisPatterns
                            For Each indicatorPhrase In hypothesesPattern.Value
                                Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                                Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                                Dim matches = regex.Matches(sentence)

                                For Each match As Match In matches
                                    hypotheses.Add(match.Value)
                                Next
                            Next
                        Next
                    Next

                    Return hypotheses
                End Function

                Public Shared Function DetectHypothesis(ByRef document As String) As String


                    Dim sentence = document.Trim().ToLower

                    ' Check if the sentence contains any indicator terms
                    If sentence.ContainsAny(GetInternalHypothesisIndicators.ToArray) Then
                        Return sentence
                    End If
                    If sentence.ToLower.ContainsAny(GetEntitys) Then
                        Return sentence
                    End If

                    For Each hypothesesPattern In GetInternalHypothesisPatterns()

                        For Each indicatorPhrase In hypothesesPattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(sentence.ToLower)

                            For Each match As Match In matches
                                Return sentence
                            Next
                        Next
                    Next

                    Return ""
                End Function


                Public Shared Function DetectHypotheses(document As String) As List(Of String)
                    Return DetectHypotheses(document, HypothesisPatterns)
                End Function

                Public Shared Function ClassifyHypothesis(hypothesis As String) As String
                    Dim lowercaseHypothesis As String = hypothesis.ToLower()

                    If lowercaseHypothesis.ContainsAny(GetCompositeHypothesisIndicators.ToArray) Then
                        Return "Composite Hypotheses"
                    ElseIf lowercaseHypothesis.ContainsAny(GetNonDirectionalHypothesisIndicators.ToArray) Then
                        Return "Non Directional Hypothesis"
                    ElseIf lowercaseHypothesis.ContainsAny(GetDirectionalHypothesisIndicators.ToArray) Then
                        Return "Directional Hypothesis"
                    ElseIf lowercaseHypothesis.ContainsAny(GetNullHypothesisIndicators.ToArray) Then
                        Return "Null Hypothesis"
                    ElseIf lowercaseHypothesis.ContainsAny(GetAlternativeHypothesisIndicators.ToArray) Then
                        Return "Alternative Hypothesis"
                    ElseIf lowercaseHypothesis.ContainsAny(GetGeneralHypothesisIndicators.ToArray) Then
                        Return "General Hypothesis"
                    ElseIf lowercaseHypothesis.ContainsAny(GetResearchHypothesisIndicators.ToArray) Then
                        Return "Research Hypothesis"
                    End If
                    For Each hypothesesPattern In GetInternalHypothesisPatterns()

                        For Each indicatorPhrase In hypothesesPattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(lowercaseHypothesis)

                            For Each match As Match In matches
                                Return hypothesesPattern.Key
                            Next
                        Next
                    Next

                    ' Check classification rules
                    For Each rule In InitializeClassificationRules()
                        For Each pattern In rule.Patterns
                            If pattern.IsMatch(lowercaseHypothesis) Then
                                Return rule.Subtype
                            End If
                        Next
                    Next

                    Return LogicalDependencyClassifier.ClassifyLogicalDependency(lowercaseHypothesis)


                    Return "Unclassified"
                End Function

                Public Shared Function ClassifySentence(ByVal sentence As String) As String
                    Dim lowercaseSentence As String = sentence.ToLower()
                    If ClassifyHypothesis(lowercaseSentence) = "Unclassified" Then
                        For Each hypothesesPattern In GetInternalHypothesisPatterns()

                            For Each indicatorPhrase In hypothesesPattern.Value
                                Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                                Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                                Dim matches = regex.Matches(lowercaseSentence)

                                For Each match As Match In matches
                                    Return hypothesesPattern.Key
                                Next
                            Next
                        Next

                        ' Check classification rules
                        For Each rule In InitializeClassificationRules()
                            For Each pattern In rule.Patterns
                                If pattern.IsMatch(lowercaseSentence) Then
                                    Return rule.Relationship
                                End If
                            Next
                        Next

                    Else
                        'detect logical relation
                        Return LogicalDependencyClassifier.ClassifyLogicalDependency(lowercaseSentence)


                    End If


                    ' If no match found, return unknown
                    Return PremiseDetector.ClassifySentence(lowercaseSentence)
                End Function
                Public Shared Sub Main()
                    Dim documents As List(Of String) = GenerateRandomTrainingData(GetInternalHypothesisIndicators)
                    Dim count As Integer = 0
                    For Each hypothesisStorage As CapturedType In HypothesisDetector.GetSentences(GenerateRandomTrainingData(GetInternalHypothesisIndicators))
                        count += 1
                        Console.WriteLine(count & ":")
                        Console.WriteLine($"Sentence: {hypothesisStorage.Sentence}")
                        Console.WriteLine("Hypothesis Classification: " & hypothesisStorage.SubType)
                        Console.WriteLine("Logical Relationship: " & hypothesisStorage.LogicalRelation_)
                        Console.WriteLine()
                    Next
                    Console.ReadLine()

                End Sub

                ''' <summary>
                ''' used to detect type of classification of hypothosis
                ''' </summary>
                ''' <returns></returns>
                Public Shared Function GetInternalHypothesisPatterns() As Dictionary(Of String, String())
                    Return New Dictionary(Of String, String()) From {
        {"Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:hypothesis|assumption|theory|proposal|supposition|conjecture|concludes|assumes|correlates)\"}},
        {"Research Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:significant|effects|has an effect|induces|strong correlation|statistical)\b"}},
        {"Null Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:no significant relationship|no relationship between|nothing|null|no effect)\b"}},
        {"Alternative Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:is a significant relationship|significant relationship between)\b"}},
        {"Directional Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:increase|decrease|loss|gain|position|correlation|above|below|before|after|precedes|preceding|following|follows|precludes)\b"}},
        {"Non-Directional Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:significant difference|no change|unchanged|unchangeable)\b"}},
        {"Diagnostic Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:diagnostic hypothesis|can identify|characteristic of|feature of)\b"}},
        {"Descriptive Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?:describes|it follows that|comprises of|comprises|builds towards)\b"}},
              {"Casual Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(causal hypothesis|causes|leads to|results in)\b"}},
               {"Explanatory Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?i)\b(?:explanatory hypothesis|explains|reason for|cause of)\b"}},
                {"Predictive Hypothesis", {"(?i)\b[A-Z][^.!?]*\b(?i)\b(prediction|forecast|projection|predicts|fore-casted:projects:projection)\b"}
    }}
                End Function

                Public Shared Function GetGeneralHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("assuming")
                    lst.Add("assuming")
                    lst.Add("theory")
                    lst.Add("proposed")
                    lst.Add("indicates")
                    lst.Add("conjecture")
                    lst.Add("correlates")


                    Return lst
                End Function
                Public Shared Function GetResearchHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant")
                    lst.Add("effects")
                    lst.Add("has an effect")
                    lst.Add("induces")
                    lst.Add("strong correlation")
                    lst.Add("statistically")
                    lst.Add("statistics show")
                    lst.Add("it can be said")
                    lst.Add("it has been shown")
                    lst.Add("been proved")

                    Return lst
                End Function
                Public Shared Function GetDirectionalHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("increase")
                    lst.Add("decrease")
                    lst.Add("loss")
                    lst.Add("gain")
                    lst.Add("position")
                    lst.Add("correlation")
                    lst.Add("above")
                    lst.Add("below")
                    lst.Add("before")
                    lst.Add("after")
                    lst.Add("precedes")
                    lst.Add("follows")
                    lst.Add("following")
                    lst.Add("gaining")
                    lst.Add("precursor")

                    Return lst
                End Function
                Public Shared Function GetInternalHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.AddRange(GetCompositeHypothesisIndicators)
                    lst.AddRange(GetNonDirectionalHypothesisIndicators)
                    lst.AddRange(GetAlternativeHypothesisIndicators)
                    lst.AddRange(GetDirectionalHypothesisIndicators)
                    lst.AddRange(GetNullHypothesisIndicators)
                    lst.AddRange(GetResearchHypothesisIndicators)
                    lst.AddRange(GetGeneralHypothesisIndicators)
                    Return lst
                End Function
                Private Shared Function GetAlternativeHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant relationship")
                    lst.Add("relationship between")
                    lst.Add("great significance")
                    lst.Add("signify")

                    Return lst
                End Function
                Private Shared Function GetNonDirectionalHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant difference")
                    lst.Add("no change")
                    lst.Add("unchangeable")
                    lst.Add("unchanged")

                    Return lst
                End Function
                Private Shared Function GetCompositeHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)

                    lst.Add("leads to")
                    lst.Add("consequence of")
                    lst.Add("it follows that")
                    lst.Add("comprises of")
                    lst.Add("comprises")
                    lst.Add("builds towards")
                    Return lst
                End Function

                Private Shared Function GetNullHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("no significant relationship")
                    lst.Add("no relationship between")
                    lst.Add("no significance")
                    lst.Add("does not signify")
                    lst.Add("no effect")
                    lst.Add("no changes")
                    Return lst
                End Function
            End Class

            Public Class PremiseDetector

                Public Shared Function ContainsAny(text As String, indicators As String()) As Boolean
                    For Each indicator As String In indicators
                        If text.Contains(indicator) Then
                            Return True
                        End If
                    Next

                    Return False
                End Function
                Public Shared Function ClassifySentence(ByVal sentence As String) As String
                    Dim lowercaseSentence As String = sentence.ToLower()
                    For Each premisePattern In GetInternalPremisePatterns()

                        For Each indicatorPhrase In premisePattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(lowercaseSentence)

                            For Each match As Match In matches
                                Return premisePattern.Key
                            Next
                        Next
                    Next
                    ' Check classification rules
                    For Each rule In InitializeClassificationRules()

                        For Each pattern In rule.Patterns
                            If pattern.IsMatch(lowercaseSentence) Then
                                Return rule.Subtype

                            End If
                        Next
                    Next




                    Return LogicalDependencyClassifier.ClassifyLogicalDependency(lowercaseSentence)


                    ' If no match found, return unknown
                    Return "Unknown"
                End Function

                Public Function ClassifyPremises(ByVal document As String) As List(Of String)
                    ' Add your premise detection logic here
                    ' Return a list of premises found in the document

                    Dim Premise As List(Of String) = DetectPremises(document)
                    ' Placeholder implementation
                    Return Premise
                End Function

                Private Shared PremisePatterns As Dictionary(Of String, String())
                Private Shared DependencyPatterns As Dictionary(Of String, List(Of Regex))
                Private Shared DeductiveDependencyPatterns As List(Of Regex)
                Private Shared InductiveDependencyPatterns As List(Of Regex)
                Private Shared ContrapositiveDependencyPatterns As List(Of Regex)
                Private Shared ConditionalDependencyPatterns As List(Of Regex)
                Private Shared CausalDependencyPatterns As List(Of Regex)
                Private Shared BiconditionalDependencyPatterns As List(Of Regex)
                Private Shared InferenceDependencyPatterns As List(Of Regex)
                Private Shared CounterfactualDependencyPatterns As List(Of Regex)
                Private Shared StatisticalDependencyPatterns As List(Of Regex)
                Private Shared AnalogicalDependencyPatterns As List(Of Regex)
                Public Shared ReadOnly DeductiveDependencyIndicators As String() = {"If", "Then"}
                Public Shared ReadOnly InductiveDependencyIndicators As String() = {"Every time", "Every instance"}
                Public Shared ReadOnly ContrapositiveDependencyIndicators As String() = {"If Not", "Then Not"}
                Public Shared ReadOnly ConditionalDependencyIndicators As String() = {"If", "When"}
                Public Shared ReadOnly CausalDependencyIndicators As String() = {"Because", "Due to"}
                Public Shared ReadOnly BiconditionalDependencyIndicators As String() = {"If And only if", "before", "after", "above", "below"}
                Public Shared ReadOnly InferenceDependencyIndicators As String() = {"Based on", "From"}
                Public Shared ReadOnly CounterfactualDependencyIndicators As String() = {"If Not", "Then Not"}
                Public Shared ReadOnly StatisticalDependencyIndicators As String() = {"Based on statistics", "According to the survey"}
                Public Shared ReadOnly AnalogicalDependencyIndicators As String() = {"Similar to", "Analogous to"}
                Public Shared ReadOnly SupportingPremiseIndicators As String() = {"For", "In support of", "Research has shown that", "Studies have demonstrated that", "Experts agree that", "Evidence suggests that", "Data indicates that", "Statistics show that", "In accordance with", "Based on the findings of", "According to the research"}
                Public Shared ReadOnly GeneralizationPremiseIndicators As String() = {"In general", "Typically", "In general", "Typically", "On average", "As a rule", "Commonly", "In most cases", "Generally speaking", "Universally", "As a general principle", "Across the board"}
                Public Shared ReadOnly ConditionalPremiseIndicators As String() = {"If", "When", "then", "Given that", "On the condition that", "Assuming that", "Provided that", "In the event that", "Whenever", "In case", "Under the circumstances that"}
                Public Shared ReadOnly AnalogicalPremiseIndicators As String() = {"Similar to", "Analogous to", "Similar to", "Just as", "Like", "Comparable to", "In the same way that", "Analogous to", "Corresponding to", "Resembling", "As if", "In a similar fashion"}
                Public Shared ReadOnly CausalPremiseIndicators As String() = {"Because", "Due to", "Because", "Since", "As a result of", "Due to", "Caused by", "Leads to", "Results in", "Owing to", "Contributes to", "Is responsible for"}
                Public Shared ReadOnly FactualPremiseIndicators As String() = {"In fact", "Indeed", "It Is a fact that", "It Is well-established that", "Historically, it has been proven that", "Scientifically speaking", "Empirical evidence confirms that", "Observations reveal that", "Documented sources state that", "In reality", "Undeniably"}

                Public Shared Function GetSentences(documents As List(Of String)) As List(Of CapturedType)
                    Dim storage As New List(Of CapturedType)
                    For Each document As String In documents
                        Dim premises As List(Of String) = PremiseDetector.DetectPremises(document, PremiseDetector.GetInternalPremisePatterns)

                        For Each premise As String In premises
                            Dim premiseSubtype As String = PremiseDetector.ExtractPremiseSubtype(premise)
                            If premiseSubtype = "Unknown" Then

                                premiseSubtype = ClassifySentence(premise)
                            End If

                            Dim classification As String = LogicalDependencyClassifier.ClassifyLogicalDependency(premise)

                            Dim premiseStorage As New CapturedType With {
                        .Sentence = premise,
                        .SubType = premiseSubtype,
                        .LogicalRelation_ = classification}
                            storage.Add(premiseStorage)
                        Next
                    Next
                    Return storage.Distinct.ToList
                End Function
                Public Shared Function GetSentence(document As String) As CapturedType


                    Dim premise As String = PremiseDetector.DetectPremise(document)


                    Dim premiseSubtype As String = ClassifySentence(premise)
                    If premiseSubtype = "Unknown" Then

                        premiseSubtype = PremiseDetector.ExtractPremiseSubtype(premise)
                    End If

                    Dim classification As String = LogicalDependencyClassifier.ClassifyLogicalDependency(premise)

                    Dim premiseStorage As New CapturedType With {
                        .Sentence = premise,
                        .SubType = premiseSubtype,
                        .LogicalRelation_ = classification}



                    Return premiseStorage
                End Function

                Public Sub New()
                    PremisePatterns = GetInternalPremisePatterns()
                    InitializeInternalDependancyPatterns()
                    DependencyPatterns = GetInternalDependacyPatterns()
                End Sub

                Public Shared Sub Main()
                    Dim detect As New PremiseDetector
                    Dim count As Integer = 0

                    For Each item In GenerateRandomTrainingData(HypothesisDetector.GetInternalHypothesisIndicators)
                        If DetectPremise(item) <> "" Then


                            Dim x = PremiseDetector.GetSentence(item)

                            count += 1
                            Console.WriteLine(count & ":")
                            Console.WriteLine($"Sentence {x.Sentence}")
                            Console.WriteLine($"Premise Subtype {x.SubType}")
                            Console.WriteLine("Classification " & x.LogicalRelation_)
                            Console.WriteLine()
                        End If

                    Next



                    Console.ReadLine()
                End Sub

                Public Shared Function GetInternalPremisePatterns() As Dictionary(Of String, String())
                    InitializeInternalDependancyPatterns()
                    Return New Dictionary(Of String, String()) From {
    {"Deductive Dependency", DeductiveDependencyIndicators},
    {"Inductive Dependency", InductiveDependencyIndicators},
    {"Contrapositive Dependency", ContrapositiveDependencyIndicators},
    {"Conditional Dependency", ConditionalDependencyIndicators},
    {"Causal Dependency", CausalDependencyIndicators},
    {"Biconditional Dependency", BiconditionalDependencyIndicators},
    {"Inference Dependency", InferenceDependencyIndicators},
    {"Counterfactual Dependency", CounterfactualDependencyIndicators},
    {"Statistical Dependency", StatisticalDependencyIndicators},
    {"Analogical Dependency", AnalogicalDependencyIndicators},
                {"Supporting Premise", SupportingPremiseIndicators},
                {"Generalization Premise", GeneralizationPremiseIndicators},
                {"Conditional Premise", ConditionalPremiseIndicators},
                {"Causal Premise", CausalPremiseIndicators},
                {"Factual Premise", FactualPremiseIndicators}, {"Analogical Premise", AnalogicalPremiseIndicators}}

                End Function

                Private Shared Sub InitializeInternalDependancyPatterns()
                    ' Initialize the patterns for each premise type
                    DeductiveDependencyPatterns = New List(Of Regex) From {
                New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                New Regex("Given\s+that\s+.*,\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*implies\s+that\s+.*", RegexOptions.IgnoreCase)
            }

                    InductiveDependencyPatterns = New List(Of Regex) From {
                New Regex(".*every\s+time\s+.*,\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+often\s+associated\s+with\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+usually\s+followed\s+by\s+.*", RegexOptions.IgnoreCase)
            }

                    ContrapositiveDependencyPatterns = New List(Of Regex) From {
                New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+Not\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase)
            }

                    ConditionalDependencyPatterns = New List(Of Regex) From {
                New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*depends\s+on\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+conditioned\s+by\s+.*", RegexOptions.IgnoreCase)
            }

                    CausalDependencyPatterns = New List(Of Regex) From {
                New Regex(".*Is\s+the\s+cause,\s+which\s+leads\s+to\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*results\s+in\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*causes\s+.*", RegexOptions.IgnoreCase)
            }

                    BiconditionalDependencyPatterns = New List(Of Regex) From {
                New Regex(".*if\s+And\s+only\s+if\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+equivalent\s+to\s+.*", RegexOptions.IgnoreCase)
            }

                    InferenceDependencyPatterns = New List(Of Regex) From {
                New Regex("Based\s+on\s+the\s+.*,\s+it\s+can\s+be\s+inferred\s+that\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*implies\s+that\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*leads\s+to\s+the\s+conclusion\s+that\s+.*", RegexOptions.IgnoreCase)
            }

                    CounterfactualDependencyPatterns = New List(Of Regex) From {
                New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*would\s+have\s+been\s+.*,\s+if\s+.*", RegexOptions.IgnoreCase)
            }

                    StatisticalDependencyPatterns = New List(Of Regex) From {
                New Regex("Based\s+on\s+a\s+survey\s+of\s+.*,\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*statistically\s+correlated\s+with\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+likely\s+if\s+.*", RegexOptions.IgnoreCase)
            }

                    AnalogicalDependencyPatterns = New List(Of Regex) From {
                New Regex(".*Is\s+similar\s+to\s+.*,\s+which\s+implies\s+that\s+.*", RegexOptions.IgnoreCase),
                New Regex(".*Is\s+analogous\s+to\s+.*,\s+indicating\s+that\s+.*", RegexOptions.IgnoreCase)
            }
                End Sub

                Private Shared Function GetInternalDependacyPatterns() As Dictionary(Of String, List(Of Regex))
                    Return New Dictionary(Of String, List(Of Regex)) From {
                    {"Deductive Dependency", New List(Of Regex) From {
                        New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                        New Regex("Given\s+that\s+.*,\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*implies\s+that\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Inductive Dependency", New List(Of Regex) From {
                        New Regex(".*every\s+time\s+.*,\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+often\s+associated\s+with\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+usually\s+followed\s+by\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Contrapositive Dependency", New List(Of Regex) From {
                        New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+Not\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Conditional Dependency", New List(Of Regex) From {
                        New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*depends\s+on\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+conditioned\s+by\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Causal Dependency", New List(Of Regex) From {
                        New Regex(".*Is\s+the\s+cause,\s+which\s+leads\s+to\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*results\s+in\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*causes\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Biconditional Dependency", New List(Of Regex) From {
                        New Regex(".*if\s+And\s+only\s+if\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+equivalent\s+to\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Inference Dependency", New List(Of Regex) From {
                        New Regex(".*infers\s+that\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+deduced\s+from\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*drawn\s+from\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Counterfactual Dependency", New List(Of Regex) From {
                        New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*would\s+have\s+occurred\s+if\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Statistical Dependency", New List(Of Regex) From {
                        New Regex("Based\s+on\s+statistics,\s+.*", RegexOptions.IgnoreCase),
                        New Regex("According\s+to\s+the\s+survey,\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Analogical Dependency", New List(Of Regex) From {
                        New Regex(".*Is\s+similar\s+to\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+analogous\s+to\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Supporting Premise", New List(Of Regex) From {
                        New Regex("For\s+example,\s+.*", RegexOptions.IgnoreCase),
                        New Regex("In\s+support\s+of\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+backed\s+by\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Generalization Premise", New List(Of Regex) From {
                        New Regex("In\s+general,\s+.*", RegexOptions.IgnoreCase),
                        New Regex("Typically,\s+.*", RegexOptions.IgnoreCase),
                        New Regex("Most\s+of\s+the\s+time,\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Conditional Premise", New List(Of Regex) From {
                        New Regex("If\s+.*,\s+then\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*relies\s+on\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Analogical Premise", New List(Of Regex) From {
                        New Regex(".*Is\s+similar\s+to\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+analogous\s+to\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Causal Premise", New List(Of Regex) From {
                        New Regex(".*Is\s+the\s+cause\s+of\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*leads\s+to\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*causes\s+.*", RegexOptions.IgnoreCase)
                    }},
                    {"Factual Premise", New List(Of Regex) From {
                        New Regex("In\s+fact,\s+.*", RegexOptions.IgnoreCase),
                        New Regex("Indeed,\s+.*", RegexOptions.IgnoreCase),
                        New Regex(".*Is\s+a\s+fact\s+that\s+.*", RegexOptions.IgnoreCase)
                    }}
                }
                End Function

                Public Function DetectPremises(ByVal document As String) As List(Of String)
                    Dim premises As New List(Of String)()

                    For Each premisePattern In PremisePatterns
                        For Each indicatorPhrase In premisePattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(document)

                            For Each match As Match In matches
                                premises.Add(match.Value)
                            Next
                        Next
                    Next

                    For Each premiseType As String In PremisePatterns.Keys
                        Dim indicators As String() = PremisePatterns(premiseType)

                        For Each indicator As String In indicators
                            Dim pattern As String = $"(?<=\b{indicator}\b).*?(?=[.]|$)"
                            Dim matches As MatchCollection = Regex.Matches(document, pattern, RegexOptions.IgnoreCase)

                            For Each match As Match In matches
                                premises.Add(match.Value.Trim())
                            Next
                        Next
                    Next

                    Return premises
                End Function
                Public Shared Function DetectPremise(ByVal document As String) As String
                    Dim premises As String = ""

                    For Each premisePattern In GetInternalPremisePatterns()

                        For Each indicatorPhrase In premisePattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(document)

                            For Each match As Match In matches
                                Return match.Value
                            Next
                        Next
                    Next


                    Return premises
                End Function

                Public Shared Function DetectPremises(ByVal document As String, ByRef PremisePatterns As Dictionary(Of String, String())) As List(Of String)
                    Dim premises As New List(Of String)()

                    For Each premisePattern In PremisePatterns
                        For Each indicatorPhrase In premisePattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(document)

                            For Each match As Match In matches
                                premises.Add(match.Value)
                            Next
                        Next
                    Next

                    For Each premiseType As String In PremisePatterns.Keys
                        Dim indicators As String() = PremisePatterns(premiseType)

                        For Each indicator As String In indicators
                            Dim pattern As String = $"(?<=\b{indicator}\b).*?(?=[.]|$)"
                            Dim matches As MatchCollection = Regex.Matches(document, pattern, RegexOptions.IgnoreCase)

                            For Each match As Match In matches
                                premises.Add(match.Value.Trim())
                            Next
                        Next
                    Next

                    Return premises
                End Function

                Public Shared Function ExtractPremiseSubtype(ByVal premise As String) As String
                    For Each premisePattern In GetInternalPremisePatterns()

                        If Not premisePattern.Value.Any(Function(indicatorPhrase) premise.Contains(indicatorPhrase)) Then
                            Continue For
                        End If
                        Return premisePattern.Key
                    Next

                    Return "Unknown"
                End Function

                Private Shared Function DetectDependencies(ByVal document As String) As List(Of String)
                    Dim dependencies As New List(Of String)()

                    For Each dependencyPattern In DependencyPatterns
                        For Each regex In dependencyPattern.Value
                            Dim matches = regex.Matches(document)

                            For Each match As Match In matches
                                dependencies.Add(match.Value)
                            Next
                        Next
                    Next

                    Return dependencies
                End Function

                Private Shared Function CheckPremiseSubtype(ByVal premise As String) As String
                    ' Extract the premise subtype from the premise string
                    If premise.Contains("Deductive Dependency") Then
                        Return "Deductive Dependency"
                    ElseIf premise.Contains("Inductive Dependency") Then
                        Return "Inductive Dependency"
                    ElseIf premise.Contains("Contrapositive Dependency") Then
                        Return "Contrapositive Dependency"
                    ElseIf premise.Contains("Conditional Dependency") Then
                        Return "Conditional Dependency"
                    ElseIf premise.Contains("Causal Dependency") Then
                        Return "Causal Dependency"
                    ElseIf premise.Contains("Biconditional Dependency") Then
                        Return "Biconditional Dependency"
                    ElseIf premise.Contains("Inference Dependency") Then
                        Return "Inference Dependency"
                    ElseIf premise.Contains("Counterfactual Dependency") Then
                        Return "Counterfactual Dependency"
                    ElseIf premise.Contains("Statistical Dependency") Then
                        Return "Statistical Dependency"
                    ElseIf premise.Contains("Analogical Dependency") Then
                        Return "Analogical Dependency"
                        ' Use regular expressions or string matching to
                        'extract the premise subtype based on indicator phrases
                    ElseIf premise.ContainsAny(DeductiveDependencyIndicators) Then
                        Return "Deductive Dependency"
                    ElseIf premise.ContainsAny(InductiveDependencyIndicators) Then
                        Return "Inductive Dependency"
                    ElseIf premise.ContainsAny(ContrapositiveDependencyIndicators) Then
                        Return "Contrapositive Dependency"
                    ElseIf premise.ContainsAny(ConditionalDependencyIndicators) Then
                        Return "Conditional Dependency"
                    ElseIf premise.ContainsAny(CausalDependencyIndicators) Then
                        Return "Causal Dependency"
                    ElseIf premise.ContainsAny(BiconditionalDependencyIndicators) Then
                        Return "Biconditional Dependency"
                    ElseIf premise.ContainsAny(InferenceDependencyIndicators) Then
                        Return "Inference Dependency"
                    ElseIf premise.ContainsAny(CounterfactualDependencyIndicators) Then
                        Return "Counterfactual Dependency"
                    ElseIf premise.ContainsAny(StatisticalDependencyIndicators) Then
                        Return "Statistical Dependency"
                    ElseIf premise.ContainsAny(AnalogicalDependencyIndicators) Then
                        Return "Analogical Dependency"
                    ElseIf premise.ContainsAny(SupportingPremiseIndicators) Then
                        Return "Supporting Premise"
                    ElseIf premise.ContainsAny(GeneralizationPremiseIndicators) Then
                        Return "Generalization Premise"
                    ElseIf premise.ContainsAny(ConditionalPremiseIndicators) Then
                        Return "Conditional Premise"
                    ElseIf premise.ContainsAny(AnalogicalPremiseIndicators) Then
                        Return "Analogical Premise"
                    ElseIf premise.ContainsAny(CausalPremiseIndicators) Then
                        Return "Causal Premise"
                    ElseIf premise.ContainsAny(FactualPremiseIndicators) Then
                        Return "Factual Premise"
                    Else
                        Return "Unknown"
                    End If
                End Function

                Public Shared Function ExtractDependencyType(ByVal dependency As String) As String
                    For Each dependencyPattern In GetInternalDependacyPatterns()

                        If dependencyPattern.Value.Any(Function(regex) regex.IsMatch(dependency)) Then
                            Return dependencyPattern.Key
                        End If
                    Next

                    Return "Unknown"
                End Function


            End Class

            Public Class ConclusionDetector

                Private Shared Function GetEntitys() As String()
                    Return {"hence", "thus", "therefore", "Consequently", "accordingly", "association with", "correlation with", "conclusion", "Significant result", "statistically results show", "Contrary to popular belief",
                "in light of", "in summary", "in future", "as described", "in lieu"}

                End Function

                Private Shared ConclusionPatterns As Dictionary(Of String, String()) = GetInternalConclusionPatterns()

                Private Shared ConclusionIndicators As String() = {"conclusion", "Assumption", "theory", "in theory", "in practice",
                "proposal", "proposes", "it can be proposed", "Supposition", "supposedly", "supposes", "conjecture", "connects",
                "concludes", "follows that", "in light of", "in reflection", "This disproves", "statistical", "discovered relationship", "correlation", "exactly"}


                Public Shared Function GetSentence(document As String) As CapturedType



                    Dim hypotheses As String = DetectConclusion(document)


                    Dim classification As String = ClassifyConclusion(hypotheses)
                    Dim logicalRelationship As String = ClassifySentence(hypotheses)

                    Dim hypothesisStorage As New CapturedType With {
                        .Sentence = hypotheses,
                        .LogicalRelation_ = logicalRelationship,
                        .SubType = classification
                    }




                    Return hypothesisStorage
                End Function


                Public Shared Function GetSentences(documents As List(Of String)) As List(Of CapturedType)
                    Dim storage As New List(Of CapturedType)
                    For Each document As String In documents
                        Dim hypotheses As List(Of String) = Detectconclusions(document)

                        For Each hypothesis As String In hypotheses
                            Dim classification As String = ClassifyConclusion(hypothesis)
                            Dim logicalRelationship As String = ClassifySentence(hypothesis)

                            Dim hypothesisStorage As New CapturedType With {
                        .Sentence = hypothesis,
                        .LogicalRelation_ = logicalRelationship,
                        .SubType = classification
                    }
                            storage.Add(hypothesisStorage)
                        Next
                    Next

                    Return storage.Distinct.ToList
                End Function
                Public Shared Function DetectConclusions(document As String, HypothesisPatterns As Dictionary(Of String, String())) As List(Of String)
                    Dim Conclusions As New List(Of String)

                    Dim sentences As String() = document.Split("."c)
                    For Each sentence As String In sentences
                        sentence = sentence.Trim().ToLower

                        ' Check if the sentence contains any indicator terms
                        If sentence.ContainsAny(ConclusionIndicators) Then
                            Conclusions.Add(sentence)
                        End If
                        If sentence.ContainsAny(GetEntitys) Then
                            Conclusions.Add(sentence)
                        End If

                        For Each conclusionPattern In ConclusionPatterns
                            For Each indicatorPhrase In conclusionPattern.Value
                                Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                                Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                                Dim matches = regex.Matches(sentence)

                                For Each match As Match In matches
                                    Conclusions.Add(match.Value)
                                Next
                            Next
                        Next
                    Next

                    Return Conclusions
                End Function

                Public Shared Function DetectConclusion(ByRef document As String) As String


                    Dim sentence = document.Trim().ToLower

                    ' Check if the sentence contains any indicator terms
                    If sentence.ContainsAny(GetInternalConclusionIndicators.ToArray) Then
                        Return sentence
                    End If
                    If sentence.ToLower.ContainsAny(GetEntitys) Then
                        Return sentence
                    End If

                    For Each hypothesesPattern In GetInternalConclusionPatterns()

                        For Each indicatorPhrase In hypothesesPattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(sentence.ToLower)

                            For Each match As Match In matches
                                Return sentence
                            Next
                        Next
                    Next

                    Return ""
                End Function


                Public Shared Function Detectconclusions(document As String) As List(Of String)
                    Return DetectConclusions(document, ConclusionPatterns)
                End Function

                Public Shared Function ClassifyConclusion(hypothesis As String) As String
                    Dim lowercaseHypothesis As String = hypothesis.ToLower()

                    For Each conclusionPattern In GetInternalConclusionPatterns()

                        For Each indicatorPhrase In conclusionPattern.Value
                            Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                            Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                            Dim matches = regex.Matches(lowercaseHypothesis)

                            For Each match As Match In matches
                                Return conclusionPattern.Key
                            Next
                        Next
                    Next

                    ' Check classification rules
                    For Each rule In InitializeClassificationRules()
                        For Each pattern In rule.Patterns
                            If pattern.IsMatch(lowercaseHypothesis) Then
                                Return rule.Subtype
                            End If
                        Next
                    Next

                    Return HypothesisDetector.ClassifySentence(lowercaseHypothesis)


                    Return "Unclassified"
                End Function

                Public Shared Function ClassifySentence(ByVal sentence As String) As String
                    Dim lowercaseSentence As String = sentence.ToLower()
                    If ClassifyConclusion(lowercaseSentence) = "Unclassified" Then
                        For Each hypothesesPattern In GetInternalConclusionPatterns()

                            For Each indicatorPhrase In hypothesesPattern.Value
                                Dim regexPattern = $"(\b{indicatorPhrase}\b).*?(\.|$)"
                                Dim regex As New Regex(regexPattern, RegexOptions.IgnoreCase)
                                Dim matches = regex.Matches(lowercaseSentence)

                                For Each match As Match In matches
                                    Return hypothesesPattern.Key
                                Next
                            Next
                        Next

                        ' Check classification rules
                        For Each rule In InitializeClassificationRules()
                            For Each pattern In rule.Patterns
                                If pattern.IsMatch(lowercaseSentence) Then
                                    Return rule.Relationship
                                End If
                            Next
                        Next

                    Else
                        'detect logical relation
                        Return LogicalDependencyClassifier.ClassifyLogicalDependency(lowercaseSentence)


                    End If


                    ' If no match found, return unknown
                    Return HypothesisDetector.ClassifySentence(lowercaseSentence)
                End Function
                Public Shared Sub Main()
                    Dim documents As List(Of String) = GenerateRandomTrainingData(GetInternalConclusionIndicators)
                    Dim count As Integer = 0
                    For Each hypothesisStorage As CapturedType In ConclusionDetector.GetSentences(GenerateRandomTrainingData(GetInternalConclusionIndicators))
                        count += 1
                        Console.WriteLine(count & ":")
                        Console.WriteLine($"Sentence: {hypothesisStorage.Sentence}")
                        Console.WriteLine("Hypothesis Classification: " & hypothesisStorage.SubType)
                        Console.WriteLine("Logical Relationship: " & hypothesisStorage.LogicalRelation_)
                        Console.WriteLine()
                    Next
                    Console.ReadLine()

                End Sub

                ''' <summary>
                ''' used to detect type of classification of hypothesis
                ''' </summary>
                ''' <returns></returns>
                Public Shared Function GetInternalConclusionPatterns() As Dictionary(Of String, String())
                    Return New Dictionary(Of String, String()) From {
        {"Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:hypothesis|assumption|theory|proposal|supposition|conjecture|concludes|assumes|correlates)\"}},
        {"Research Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:significant|effects|has an effect|induces|strong correlation|statistical)\b"}},
        {"Null Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:no significant relationship|no relationship between|nothing|null|no effect)\b"}},
        {"Alternative Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:is a significant relationship|significant relationship between)\b"}},
        {"Directional Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:increase|decrease|loss|gain|position|correlation|above|below|before|after|precedes|preceding|following|follows|precludes)\b"}},
        {"Non-Directional Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:significant difference|no change|unchanged|unchangeable)\b"}},
        {"Diagnostic Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:diagnostic hypothesis|can identify|characteristic of|feature of|factors entail)\b"}},
        {"Descriptive Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:describes|it follows that|comprises of|comprises|builds towards)\b"}},
             {"Recommendation Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?:recommends|it is suggested that|it is urged|it is advisable|Considering these factors|Based on these findings)\b"}},
                       {"Casual Conclusion", {"(?i)\b[A-Z][^.!?]*\b(causal hypothesis|causes|leads to|results in)\b"}},
                    {"Conditional Conclusion", {"(?i)\b[A-Z][^.!?]*\b(provided that|As a result|it leads to|results in|conditionally:based on:due to|because of|under the circumstances)\b"}},
               {"Explanatory Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?i)\b(?:explanatory hypothesis|explains|reason for|cause of)\b"}},
                {"Predictive Conclusion", {"(?i)\b[A-Z][^.!?]*\b(?i)\b(prediction|it is estimated that|based on projections|it is foreseen that|fore-casted|predictive models|projections show)\b"}
    }}
                End Function

                Public Shared Function GetGeneralConclusionIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("assuming")
                    lst.Add("assuming")
                    lst.Add("theory")
                    lst.Add("proposed")
                    lst.Add("indicates")
                    lst.Add("conjecture")
                    lst.Add("correlates")


                    Return lst
                End Function
                Public Shared Function GetResearchHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant")
                    lst.Add("effects")
                    lst.Add("has an effect")
                    lst.Add("induces")
                    lst.Add("strong correlation")
                    lst.Add("statistically")
                    lst.Add("statistics show")
                    lst.Add("it can be said")
                    lst.Add("it has been shown")
                    lst.Add("been proved")

                    Return lst
                End Function
                Public Shared Function GetDirectionalHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("increase")
                    lst.Add("decrease")
                    lst.Add("loss")
                    lst.Add("gain")
                    lst.Add("position")
                    lst.Add("correlation")
                    lst.Add("above")
                    lst.Add("below")
                    lst.Add("before")
                    lst.Add("after")
                    lst.Add("precedes")
                    lst.Add("follows")
                    lst.Add("following")
                    lst.Add("gaining")
                    lst.Add("precursor")

                    Return lst
                End Function
                Public Shared Function GetInternalConclusionIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.AddRange(GetCompositeHypothesisIndicators)
                    lst.AddRange(GetNonDirectionalHypothesisIndicators)
                    lst.AddRange(GetAlternativeHypothesisIndicators)
                    lst.AddRange(GetDirectionalHypothesisIndicators)
                    lst.AddRange(GetNullHypothesisIndicators)
                    lst.AddRange(GetResearchHypothesisIndicators)
                    lst.AddRange(GetGeneralConclusionIndicators)
                    Return lst
                End Function
                Private Shared Function GetAlternativeHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant relationship")
                    lst.Add("relationship between")
                    lst.Add("great significance")
                    lst.Add("signify")

                    Return lst
                End Function
                Private Shared Function GetNonDirectionalHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("significant difference")
                    lst.Add("no change")
                    lst.Add("unchangeable")
                    lst.Add("unchanged")

                    Return lst
                End Function
                Private Shared Function GetCompositeHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)

                    lst.Add("leads to")
                    lst.Add("consequence of")
                    lst.Add("it follows that")
                    lst.Add("comprises of")
                    lst.Add("comprises")
                    lst.Add("builds towards")
                    Return lst
                End Function

                Private Shared Function GetNullHypothesisIndicators() As List(Of String)
                    Dim lst As New List(Of String)
                    lst.Add("no significant relationship")
                    lst.Add("This contradicts")
                    lst.Add("no significance")
                    lst.Add("does not signify")
                    lst.Add("this disproves")
                    lst.Add("Contrary to popular belief")
                    lst.Add("this negates")
                    Return lst
                End Function
            End Class
            Public Class QuestionDetector

                Private ReadOnly ClassificationRules As List(Of ClassificationRule)

                Private Shared ReadOnly CauseEffectQuestionIndicators As String() = {"why does", "how does", "what causes"}
                Private Shared ReadOnly ComparativeQuestionIndicators As String() = {"which", "what is the difference", "how does"}
                Private Shared ReadOnly DependentQuestionIndicators As String() = {"if", "unless", "whether", "in case"}
                Private Shared ReadOnly DescriptiveQuestionIndicators As String() = {"describe", "explain", "tell me about"}
                Private Shared ReadOnly HypotheticalQuestionIndicators As String() = {"what if", "imagine", "suppose", "assume"}
                Private Shared ReadOnly IndependentQuestionIndicators As String() = {"what", "who", "where", "when", "why", "how"}
                Private Shared ReadOnly LocationalQuestionIndicators As String() = {"where is", "where was", "where are"}
                Private Shared ReadOnly SocialQuestionIndicators As String() = {"who is", "who was", "who were", "do you", "do they"}
                Private Shared ReadOnly TemporalQuestionIndicators As String() = {"when is", "when was", "when were", "when are", "what day", "what time"}
                Private Shared ReadOnly QuestionPattern As Regex = New Regex("^\s*(?:what|who|where|when|why|how|if|unless|whether|in case|which|what is the difference|how does|why does|describe|explain|tell me about|what if|imagine|suppose|assume)\b", RegexOptions.IgnoreCase)

                Public Shared Function ClassifySentence(sentence As String) As String
                    Dim lowercaseSentence As String = sentence.ToLower()

                    If IsIndependentQuestion(lowercaseSentence) Then
                        Return "Independent Question"
                    ElseIf IsDependentQuestion(lowercaseSentence) Then
                        Return "Dependent Question"
                    ElseIf IsComparativeQuestion(lowercaseSentence) Then
                        Return "Comparative Question"
                    ElseIf IsCauseEffectQuestion(lowercaseSentence) Then
                        Return "Cause-Effect Question"
                    ElseIf IsDescriptiveQuestion(lowercaseSentence) Then
                        Return "Descriptive Question"
                    ElseIf IsHypotheticalQuestion(lowercaseSentence) Then
                        Return "Hypothetical Question"
                    ElseIf IsTemporalQuestion(lowercaseSentence) Then
                        Return "Temporal Question"
                    ElseIf IsSocialQuestion(lowercaseSentence) Then
                        Return "Social Question"
                    ElseIf IsLocationalQuestion(lowercaseSentence) Then
                        Return "Locational Question"
                    Else
                        Return "Unclassified"
                    End If
                End Function
                Private Shared Function IsQuestionType(sentence As String, indicators As String()) As Boolean
                    Return StartsWithAny(sentence, indicators)
                End Function



                Public Shared Function GetSentence(sentence As String) As CapturedType
                    Dim lowercaseSentence As String = sentence.ToLower()
                    Dim newType As New CapturedType With {
            .Sentence = lowercaseSentence,
            .LogicalRelation_ = LogicalDependencyClassifier.ClassifyLogicalDependency(lowercaseSentence)
        }

                    If IsQuestionType(lowercaseSentence, IndependentQuestionIndicators) Then
                        newType.SubType = "Independent Question"
                    ElseIf IsQuestionType(lowercaseSentence, DependentQuestionIndicators) Then
                        newType.SubType = "Dependent Question"
                    ElseIf IsQuestionType(lowercaseSentence, ComparativeQuestionIndicators) Then
                        newType.SubType = "Comparative Question"
                    ElseIf IsQuestionType(lowercaseSentence, CauseEffectQuestionIndicators) Then
                        newType.SubType = "Cause-Effect Question"
                    ElseIf IsQuestionType(lowercaseSentence, DescriptiveQuestionIndicators) Then
                        newType.SubType = "Descriptive Question"
                    ElseIf IsQuestionType(lowercaseSentence, HypotheticalQuestionIndicators) Then
                        newType.SubType = "Hypothetical Question"
                    ElseIf IsQuestionType(lowercaseSentence, TemporalQuestionIndicators) Then
                        newType.SubType = "Temporal Question"
                    ElseIf IsQuestionType(lowercaseSentence, SocialQuestionIndicators) Then
                        newType.SubType = "Social Question"
                    ElseIf IsQuestionType(lowercaseSentence, LocationalQuestionIndicators) Then
                        newType.SubType = "Locational Question"
                    Else
                        newType.SubType = "Unclassified"
                    End If

                    Return newType
                End Function

                Private Shared Function IsLocationalQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(LocationalQuestionIndicators)
                End Function
                Private Shared Function IsSocialQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(SocialQuestionIndicators)
                End Function
                Private Shared Function IsTemporalQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(TemporalQuestionIndicators)
                End Function
                Private Shared Function IsCauseEffectQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(CauseEffectQuestionIndicators)
                End Function

                Private Shared Function IsComparativeQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(ComparativeQuestionIndicators)
                End Function

                Private Shared Function IsDependentQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(DependentQuestionIndicators)
                End Function

                Private Shared Function IsDescriptiveQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(DescriptiveQuestionIndicators)
                End Function

                Private Shared Function IsHypotheticalQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(HypotheticalQuestionIndicators)
                End Function

                Private Shared Function IsIndependentQuestion(sentence As String) As Boolean
                    Return sentence.StartsWithAny(IndependentQuestionIndicators)
                End Function

                Private Shared Function StartsWithAny(ByVal input As String, ByVal values As String()) As Boolean
                    For Each value As String In values
                        If input.StartsWith(value, StringComparison.OrdinalIgnoreCase) Then
                            Return True
                        End If
                    Next
                    Return False
                End Function

                Public Shared Sub Main()
                    ' Example usage
                    Dim sentences As String() = {
                    "What is the effect of smoking on health?",
                    "How does exercise affect weight loss?",
                    "If it rains, will the event be canceled?",
                    "Describe the process of photosynthesis.",
                    "What if I don't submit the assignment?",
                    "Who discovered penicillin?",
                    "Where is the nearest hospital?",
                    "When was the Declaration of Independence signed?",
                    "Why is the sky blue?",
                    "How does a computer work?"
                }

                    For Each sentence As String In sentences
                        Dim questionType = GetSentence(sentence)
                        Console.WriteLine("Sentence: " & questionType.Sentence)
                        Console.WriteLine("Question Type: " & questionType.SubType)
                        Console.WriteLine("Logical Relation: " & questionType.LogicalRelation_)
                        Console.WriteLine()
                    Next

                    Console.ReadLine()
                End Sub
            End Class

        End Class
        Public Class RuleBasedEntityRecognizer
            Private Shared entityPatterns As Dictionary(Of String, String)

            ''' <summary>
            ''' Represents a captured word and its associated information.
            ''' </summary>
            Public Structure CapturedWord
                ''' <summary>
                ''' The captured word.
                ''' </summary>
                Public Property Word As String
                ''' <summary>
                ''' The list of preceding words.
                ''' </summary>
                Public Property PrecedingWords As List(Of String)
                ''' <summary>
                ''' The list of following words.
                ''' </summary>
                Public Property FollowingWords As List(Of String)
                ''' <summary>
                ''' The person associated with the word.
                ''' </summary>
                Public Property Person As String
                ''' <summary>
                ''' The location associated with the word.
                ''' </summary>
                Public Property Location As String
                ''' <summary>
                ''' The recognized entity.
                ''' </summary>
                Public Property Entity As String
                ''' <summary>
                ''' Indicates whether the word is recognized as an entity.
                ''' </summary>
                Public Property IsEntity As Boolean
                ''' <summary>
                ''' The entity type of the word.
                ''' </summary>
                Public Property EntityType As String
                ''' <summary>
                ''' The list of entity types associated with the word.
                ''' </summary>
                Public Property EntityTypes As List(Of String)
                ''' <summary>
                ''' Indicates whether the word is the focus term.
                ''' </summary>
                Public Property IsFocusTerm As Boolean
                ''' <summary>
                ''' Indicates whether the word is a preceding word.
                ''' </summary>
                Public Property IsPreceding As Boolean
                ''' <summary>
                ''' Indicates whether the word is a following word.
                ''' </summary>
                Public Property IsFollowing As Boolean
                ''' <summary>
                ''' The context words.
                ''' </summary>
                Public Property ContextWords As List(Of String)

                ''' <summary>
                ''' Initializes a new instance of the <see cref="CapturedWord"/> structure.
                ''' </summary>
                ''' <param name="word">The captured word.</param>
                ''' <param name="precedingWords">The list of preceding words.</param>
                ''' <param name="followingWords">The list of following words.</param>
                ''' <param name="person">The person associated with the word.</param>
                ''' <param name="location">The location associated with the word.</param>
                Public Sub New(ByVal word As String, ByVal precedingWords As List(Of String), ByVal followingWords As List(Of String), ByVal person As String, ByVal location As String)
                    Me.Word = word
                    Me.PrecedingWords = precedingWords
                    Me.FollowingWords = followingWords
                    Me.Person = person
                    Me.Location = location
                End Sub
            End Structure
            Public Enum EntityPositionPrediction
                None
                Before
                After
            End Enum
            ''' <summary>
            ''' Performs a Sub-search within the given context words to recognize entities.
            ''' </summary>
            ''' <param name="contextWords">(applied After) The context words to search within.</param>
            ''' <param name="targetWord">The target word to recognize entities in.</param>
            ''' <returns>A list of captured words with entity information.</returns>
            Public Function PerformAfterSubSearch(ByVal contextWords As List(Of String), ByVal targetWord As String) As List(Of CapturedWord)
                Dim recognizedEntities As New List(Of CapturedWord)()
                Dim NewPat As String = targetWord
                For Each contextWord As String In contextWords

                    NewPat &= " " & contextWord
                    Dim entities As List(Of CapturedWord) = RecognizeEntities(contextWord & " " & targetWord)

                    If entities.Count > 0 Then
                        recognizedEntities.AddRange(entities)
                    End If
                Next

                Return recognizedEntities
            End Function
            ''' <summary>
            ''' Performs a subsearch within the given context words to recognize entities.
            ''' </summary>
            ''' <param name="contextWords">(Applied before) The context words to search within.</param>
            ''' <param name="targetWord">The target word to recognize entities in.</param>
            ''' <returns>A list of captured words with entity information.</returns>
            Public Function PerformBeforeSubSearch(ByVal contextWords As List(Of String), ByVal targetWord As String) As List(Of CapturedWord)
                Dim recognizedEntities As New List(Of CapturedWord)()
                Dim NewPat As String = targetWord
                For Each contextWord As String In contextWords

                    NewPat = contextWord & " " & NewPat
                    Dim entities As List(Of CapturedWord) = RecognizeEntities(contextWord & " " & targetWord)

                    If entities.Count > 0 Then
                        recognizedEntities.AddRange(entities)
                    End If
                Next

                Return recognizedEntities
            End Function

            Public Shared Sub Main()
                Dim recognizer As New RuleBasedEntityRecognizer()

                ' Configure entity patterns
                recognizer.ConfigureEntityPatterns()

                ' Example input text
                Dim inputText As String = "John went to the store and met Mary."

                ' Capture words with entity context
                Dim capturedWords As List(Of RuleBasedEntityRecognizer.CapturedWord) = recognizer.CaptureWordsWithEntityContext(inputText, "store", 2, 2)

                ' Display captured words and their entity information
                For Each capturedWord As RuleBasedEntityRecognizer.CapturedWord In capturedWords
                    Console.WriteLine("Word: " & capturedWord.Word)
                    Console.WriteLine("Is Entity: " & capturedWord.IsEntity)
                    Console.WriteLine("Entity Types: " & String.Join(", ", capturedWord.EntityTypes))
                    Console.WriteLine("Is Focus Term: " & capturedWord.IsFocusTerm)
                    Console.WriteLine("Is Preceding: " & capturedWord.IsPreceding)
                    Console.WriteLine("Is Following: " & capturedWord.IsFollowing)
                    Console.WriteLine("Context Words: " & String.Join(" ", capturedWord.ContextWords))
                    Console.WriteLine()
                Next

                Console.ReadLine()
            End Sub
            ''' <summary>
            ''' Configures the entity patterns by adding them to the recognizer.
            ''' </summary>
            Public Sub ConfigureEntityPatterns()
                ' Define entity patterns
                Me.AddEntityPattern("Person", "John|Mary|David")
                Me.AddEntityPattern("Location", "store|office|park")
                ' Add more entity patterns as needed
            End Sub

            ''' <summary>
            ''' Gets the entity types associated with a given word.
            ''' </summary>
            ''' <param name="word">The word to get entity types for.</param>
            ''' <returns>A list of entity types associated with the word.</returns>
            Public Function GetEntityTypes(ByVal word As String) As List(Of String)
                Dim recognizedEntities As List(Of CapturedWord) = RuleBasedEntityRecognizer.RecognizeEntities(word)
                Return recognizedEntities.Select(Function(entity) entity.EntityType).ToList()
            End Function

            ''' <summary>
            ''' Captures words with their context based on a focus term and the number of preceding and following words to include.
            ''' </summary>
            ''' <param name="text">The input text.</param>
            ''' <param name="focusTerm">The focus term to capture.</param>
            ''' <param name="precedingWordsCount">The number of preceding words to capture.</param>
            ''' <param name="followingWordsCount">The number of following words to capture.</param>
            ''' <returns>A list of WordWithContext objects containing captured words and their context information.</returns>
            Public Function CaptureWordsWithEntityContext(ByVal text As String, ByVal focusTerm As String, ByVal precedingWordsCount As Integer, ByVal followingWordsCount As Integer) As List(Of CapturedWord)
                Dim words As List(Of String) = text.Split(" "c).ToList()
                Dim focusIndex As Integer = words.IndexOf(focusTerm)

                Dim capturedWordsWithEntityContext As New List(Of CapturedWord)()

                If focusIndex <> -1 Then
                    Dim startIndex As Integer = Math.Max(0, focusIndex - precedingWordsCount)
                    Dim endIndex As Integer = Math.Min(words.Count - 1, focusIndex + followingWordsCount)

                    Dim contextWords As List(Of String) = words.GetRange(startIndex, endIndex - startIndex + 1)

                    Dim prediction As EntityPositionPrediction = PredictEntityPosition(contextWords, focusTerm)

                    For i As Integer = startIndex To endIndex
                        Dim word As String = words(i)

                        Dim entityTypes As List(Of String) = GetEntityTypes(word)

                        If entityTypes.Count = 0 AndAlso prediction <> EntityPositionPrediction.None Then
                            Dim isLowConfidenceEntity As Boolean = (prediction = EntityPositionPrediction.After AndAlso i > focusIndex) OrElse
                                                           (prediction = EntityPositionPrediction.Before AndAlso i < focusIndex)

                            If isLowConfidenceEntity Then
                                entityTypes.Add("Low Confidence Entity")
                            End If
                        End If

                        Dim wordWithContext As New CapturedWord() With {
                    .Word = word,
                    .IsEntity = entityTypes.Count > 0,
                    .EntityTypes = entityTypes,
                    .IsFocusTerm = (i = focusIndex),
                    .IsPreceding = (i < focusIndex),
                    .IsFollowing = (i > focusIndex),
                    .ContextWords = contextWords
                }

                        capturedWordsWithEntityContext.Add(wordWithContext)
                    Next
                End If

                Return capturedWordsWithEntityContext
            End Function

            ''' <summary>
            ''' Predicts the position of an entity relative to the focus term within the context words.
            ''' </summary>
            ''' <param name="contextWords">The context words.</param>
            ''' <param name="focusTerm">The focus term.</param>
            ''' <returns>The predicted entity position.</returns>
            Public Function PredictEntityPosition(ByVal contextWords As List(Of String), ByVal focusTerm As String) As EntityPositionPrediction
                Dim termIndex As Integer = contextWords.IndexOf(focusTerm)

                If termIndex >= 0 Then
                    If termIndex < contextWords.Count - 1 Then
                        Return EntityPositionPrediction.After
                    ElseIf termIndex > 0 Then
                        Return EntityPositionPrediction.Before
                    End If
                End If

                Return EntityPositionPrediction.None
            End Function

            ''' <summary>
            ''' Initializes a new instance of the <see cref="RuleBasedEntityRecognizer"/> class.
            ''' </summary>
            Public Sub New()
                entityPatterns = New Dictionary(Of String, String)()
            End Sub

            ''' <summary>
            ''' Adds an entity pattern to the recognizer.
            ''' </summary>
            ''' <param name="entityType">The entity type.</param>
            ''' <param name="pattern">The regular expression pattern.</param>
            Public Sub AddEntityPattern(ByVal entityType As String, ByVal pattern As String)
                entityPatterns.Add(entityType, pattern)
            End Sub

            ''' <summary>
            ''' Recognizes entities in the given text.
            ''' </summary>
            ''' <param name="text">The text to recognize entities in.</param>
            ''' <returns>A list of captured words with entity information.</returns>
            Public Shared Function RecognizeEntities(ByVal text As String) As List(Of CapturedWord)
                Dim capturedEntities As New List(Of CapturedWord)()

                For Each entityType As String In entityPatterns.Keys
                    Dim pattern As String = entityPatterns(entityType)
                    Dim matches As MatchCollection = Regex.Matches(text, pattern)

                    For Each match As Match In matches
                        capturedEntities.Add(New CapturedWord() With {
                    .Entity = match.Value,
                    .EntityType = entityType
                })
                    Next
                Next

                Return capturedEntities
            End Function
        End Class
        Public Module Helper
            <Extension()>
            Public Function StartsWithAny(ByVal input As String, ByVal values As String()) As Boolean
                For Each value As String In values
                    If input.StartsWith(value, StringComparison.OrdinalIgnoreCase) Then
                        Return True
                    End If
                Next
                Return False
            End Function


            Enum ConclusionTypes
                Affirmative_Conclusion
                Conditional_Conclusion
                Negative_Conclusion
                Recommendation_Conclusion
                Prediction_Conclusion
            End Enum

            <Extension()>
            Public Function ContainsAny(text As String, indicators As String()) As Boolean
                For Each indicator As String In indicators
                    If text.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

        End Module

        Module TrainingData
            Public Function GenerateRandomTrainingData(ByRef Seeds As List(Of String)) As List(Of String)
                Dim trainingData As New List(Of String)()
                Dim random As New Random()

                ' List of entities
                Dim entities As New List(Of String)() From {
        "John", "Mary", "Susan", "The cat", "The book",
        "The president", "car", "book", "chair", "laptop",
        "Alice", "Bob", "The dog", "The car", "home", "office", "park", "store",
        "where is", "What is a", "where is "
    }
                entities.AddRange(Seeds)
                ' List of pronouns
                Dim pronouns As New List(Of String)() From {
        "he", "she", "it", "they", "his", "her", "its", "their"
    }

                ' Generate 100 random training examples
                For i = 1 To 100
                    Dim sb As New StringBuilder()

                    ' Generate a sentence with co-reference
                    Dim numEntities = random.Next(2, 4) ' Randomly select 2 to 3 entities in a sentence
                    Dim entityIndices = Enumerable.Range(0, entities.Count).OrderBy(Function(x) random.Next()).Take(numEntities)
                    Dim pronounIndex = random.Next(numEntities)

                    For j = 0 To numEntities - 1
                        Dim entity = entities(entityIndices(j))
                        sb.Append(entity)

                        If j = pronounIndex Then
                            sb.Append(" ").Append(pronouns(random.Next(pronouns.Count)))
                        End If

                        sb.Append(" ")
                    Next

                    trainingData.Add(sb.ToString().Trim())
                Next

                Return trainingData
            End Function


            Public Function iGetTrainingSentences() As List(Of String)
                Dim documents As New List(Of String)()
                documents.AddRange(IgetTrainingQuestionSentences)
                ' Generate random documents related to NLP, machine learning, and data science
                documents.Add("If a sentence contains a noun and a verb, then it is grammatically correct.")
                documents.Add("In NLP, language models are often used to generate text based on input prompts.")
                documents.Add("Every time a new document is added to the dataset, the machine learning model is retrained.")
                documents.Add("If the temperature exceeds 30 degrees Celsius, the ice cream will melt.")
                documents.Add("Similar to image classification, text classification involves assigning labels to textual data.")
                documents.Add("Because of the high correlation between study time and test scores, it can be concluded that studying more leads to better performance.")
                documents.Add("In fact, the sun is a star.")
                documents.Add("Based on statistical analysis, there is a strong correlation between feature X and the target variable.")
                documents.Add("If the input data is preprocessed properly, the accuracy of the machine learning model improves.")
                documents.Add("The use of deep learning algorithms has led to significant advancements in natural language processing.")
                documents.Add("If a hypothesis is rejected based on the p-value, then the null hypothesis is accepted.")
                documents.Add("Analogous to image classification, text classification involves assigning labels to textual data.")
                documents.Add("By analyzing large volumes of data, data scientists can uncover valuable insights and patterns.")
                documents.Add("If all mammals are warm-blooded, and dogs are mammals, then dogs are warm-blooded.")
                documents.Add("Every time it rains, the streets get wet. It will rain today.")
                documents.Add("If it's raining, then the ground is wet. The ground is not wet.")
                documents.Add("If you study hard, you will get good grades. You studied hard.")
                documents.Add("Eating a healthy diet improves overall health. If you eat a healthy diet, your energy levels will increase.")
                documents.Add("An animal is a mammal if and only if it gives birth to live young. The platypus gives birth to live young.")
                documents.Add("All roses are flowers. The red object is a rose.")
                documents.Add("If it were not raining, the ground would not be wet. The ground is wet.")
                documents.Add("In a survey of 1,000 participants, 80% preferred tea over coffee. If a random person is chosen from the population, they are likely to prefer tea over coffee.")
                documents.Add("In the past, when the price of oil increased, the stock market declined. If the price of oil increases, the stock market will decline.")
                documents.Add("For a hypothesis to be accepted, it must be supported by evidence.")
                documents.Add("In general, cats are independent animals.")
                documents.Add("If the temperature exceeds 30 degrees Celsius, the ice cream will melt.")
                documents.Add("Similar to image classification, text classification involves assigning labels to textual data.")
                documents.Add("Because of the high correlation between study time and test scores, it can be concluded that studying more leads to better performance.")
                documents.Add("In fact, the sun is a star.")
                Return documents
            End Function
            Public Function IgetTrainingQuestionSentences() As List(Of String)
                Dim trainingSentences As New List(Of String)

                ' Independent Question
                trainingSentences.Add("What is the capital of France?")
                trainingSentences.Add("Who won the World Cup in 2018?")
                trainingSentences.Add("Where can I find a good restaurant?")
                trainingSentences.Add("When is the next train arriving?")
                trainingSentences.Add("Why do birds fly?")
                trainingSentences.Add("How do I bake a cake?")

                ' Dependent Question
                trainingSentences.Add("If it rains, will the picnic be canceled?")
                trainingSentences.Add("Unless you study, you won't pass the exam.")
                trainingSentences.Add("Whether it's sunny or cloudy, we'll go to the beach.")
                trainingSentences.Add("In case of an emergency, call 911.")

                ' Comparative Question
                trainingSentences.Add("Which one is better, chocolate or vanilla?")
                trainingSentences.Add("What is the difference between a cat and a dog?")
                trainingSentences.Add("How does the price of gasoline compare to last year?")

                ' Cause-Effect Question
                trainingSentences.Add("Why does exercise improve cardiovascular health?")
                trainingSentences.Add("How does stress affect sleep quality?")
                trainingSentences.Add("What causes earthquakes?")

                ' Descriptive Question
                trainingSentences.Add("Describe the main features of a smartphone.")
                trainingSentences.Add("Explain the process of photosynthesis.")
                trainingSentences.Add("Tell me about the history of the Roman Empire.")

                ' Hypothetical Question
                trainingSentences.Add("What if humans could fly?")
                trainingSentences.Add("Imagine a world without internet.")
                trainingSentences.Add("Suppose you won the lottery, what would you do?")
                trainingSentences.Add("Assume you have unlimited resources, how would you solve world hunger?")

                Return trainingSentences
            End Function

        End Module

        Public Class LogicalArgumentClassifier
            Private Shared ClassificationRules As List(Of ClassificationRule)

            Public Shared Function ClassifyText(ByVal text As String) As List(Of ClassificationRule)
                Dim matchedRules As New List(Of ClassificationRule)()

                For Each rule As ClassificationRule In ClassificationRules
                    For Each pattern As Regex In rule.Patterns
                        If pattern.IsMatch(text) Then
                            matchedRules.Add(rule)
                            Exit For
                        End If
                    Next
                Next

                Return matchedRules
            End Function

            Public Shared Sub Main()
                ' Define the classification rules
                ClassificationRules = New List(Of ClassificationRule)()

                ' Question Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Question",
            .Subtype = "General",
            .Relationship = "General Question",
            .Patterns = New List(Of Regex)() From {
                New Regex("^(?:What|Who|Where|When|Why|How)\b", RegexOptions.IgnoreCase),
                New Regex("^Is\b", RegexOptions.IgnoreCase),
                New Regex("^\bCan\b", RegexOptions.IgnoreCase),
                New Regex("^\bAre\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Question",
            .Subtype = "Comparison",
            .Relationship = "Compare Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^(?:Which|What|Who)\b.*\b(is|are)\b.*\b(?:better|worse|superior|inferior|more|less)\b", RegexOptions.IgnoreCase),
                New Regex("^(?:How|In what way)\b.*\b(?:different|similar|alike)\b", RegexOptions.IgnoreCase),
                New Regex("^(?:Compare|Contrast)\b", RegexOptions.IgnoreCase)
            }
        })

                ' Answer Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Answer",
            .Subtype = "General",
            .Relationship = "Confirm/Deny",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bYes\b", RegexOptions.IgnoreCase),
                New Regex("^\bNo\b", RegexOptions.IgnoreCase),
                New Regex("^\bMaybe\b", RegexOptions.IgnoreCase),
                New Regex("^\bI don't know\b", RegexOptions.IgnoreCase)
            }
        })

                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Answer",
            .Subtype = "Comparison",
            .Relationship = "Compare Premise",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bA is\b.*\b(?:better|worse|superior|inferior|more|less)\b", RegexOptions.IgnoreCase),
                New Regex("^\bA is\b.*\b(?:different|similar|alike)\b", RegexOptions.IgnoreCase),
                New Regex("^\bIt depends\b", RegexOptions.IgnoreCase),
                New Regex("^\bBoth\b", RegexOptions.IgnoreCase)
            }
        })

                ' Hypothesis Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Hypothesis",
            .Subtype = "General",
            .Relationship = "Hypothesize",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bIf\b", RegexOptions.IgnoreCase),
                New Regex("^\bAssuming\b", RegexOptions.IgnoreCase),
                New Regex("^\bSuppose\b", RegexOptions.IgnoreCase),
                New Regex("^\bHypothesize\b", RegexOptions.IgnoreCase)
            }
        })

                ' Conclusion Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Conclusion",
            .Subtype = "General",
            .Relationship = "Follow On Conclusion",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bTherefore\b", RegexOptions.IgnoreCase),
                New Regex("^\bThus\b", RegexOptions.IgnoreCase),
                New Regex("^\bHence\b", RegexOptions.IgnoreCase),
                New Regex("^\bConsequently\b", RegexOptions.IgnoreCase)
            }
        })

                ' Premise Rules
                ClassificationRules.Add(New ClassificationRule() With {
            .Type = "Premise",
            .Subtype = "General",
            .Relationship = "Reason",
            .Patterns = New List(Of Regex)() From {
                New Regex("^\bBecause\b", RegexOptions.IgnoreCase),
                New Regex("^\bSince\b", RegexOptions.IgnoreCase),
                New Regex("^\bGiven that\b", RegexOptions.IgnoreCase),
                New Regex("^\bConsidering\b", RegexOptions.IgnoreCase)
            }
        })

                ' Test the function
                Dim text As String = "What is the hypothesis for this experiment?"
                Dim matchedRules As List(Of ClassificationRule) = ClassifyText(text)

                Console.WriteLine("Matched Rules:")
                For Each rule As ClassificationRule In matchedRules
                    Console.WriteLine("Type: " & rule.Type)
                    Console.WriteLine("Subtype: " & rule.Subtype)
                    Console.WriteLine("Relationship: " & rule.Relationship)
                    Console.WriteLine()
                Next

                Console.ReadLine()
            End Sub
        End Class

        Public Class ContextAnalyzer
            Public Class StatementGroup
                ''' <summary>
                ''' Gets or sets the type of the statement group (e.g., Premise, Conclusion, Hypothesis).
                ''' </summary>
                Public Property Type As String

                ''' <summary>
                ''' Gets or sets the list of sentences in the statement group.
                ''' </summary>
                Public Property Sentences As List(Of String)

                ''' <summary>
                ''' Initializes a new instance of the StatementGroup class.
                ''' </summary>
                Public Sub New()
                    Sentences = New List(Of String)()
                End Sub
            End Class
            Public Function GroupStatements(ByVal sentences As List(Of String)) As List(Of StatementGroup)
                Dim statementGroups As New List(Of StatementGroup)()
                Dim previousSentenceType As String = ""

                ' Placeholder for sentence classification logic
                Dim sentenceTypes = ClassifySentences(sentences)

                For Each item In sentenceTypes
                    Dim sentenceType = item.Type

                    ' Perform context analysis based on the previous sentence type and current sentence
                    ' Apply rules or patterns to group the sentences accordingly

                    ' Example rule: If the current sentence is a premise and the previous sentence is a conclusion, group them together
                    If sentenceType = "Premise" AndAlso previousSentenceType = "Conclusion" Then
                        ' Check if the group exists, otherwise create a new group
                        Dim premiseGroup = statementGroups.FirstOrDefault(Function(g) g.Type = "Premise")
                        If premiseGroup Is Nothing Then
                            premiseGroup = New StatementGroup() With {.Type = "Premise"}
                            statementGroups.Add(premiseGroup)
                        End If

                        ' Add the current premise sentence to the existing group
                        premiseGroup.Sentences.Add(item.Entity.Sentence)
                    End If

                    ' Example rule: If the current sentence is a premise and the previous sentence is a Hypothesis, group them together
                    If sentenceType = "Premise" AndAlso previousSentenceType = "Hypothesis" Then
                        ' Check if the group exists, otherwise create a new group
                        Dim premiseGroup = statementGroups.FirstOrDefault(Function(g) g.Type = "Premise")
                        If premiseGroup Is Nothing Then
                            premiseGroup = New StatementGroup() With {.Type = "Premise"}
                            statementGroups.Add(premiseGroup)
                        End If

                        ' Add the current premise sentence to the existing group
                        premiseGroup.Sentences.Add(item.Entity.Sentence)
                    End If

                    ' Add more rules or patterns to group other sentence types

                    ' Update the previous sentence type for the next iteration
                    previousSentenceType = sentenceType
                Next

                ' Return the grouped statements
                Return statementGroups
            End Function

            Public Shared Sub main()




                Dim sentences As New List(Of String)()
                sentences.Add("This is a premise sentence.")
                sentences.Add("Therefore, the conclusion follows.")
                sentences.Add("Based on the hypothesis, we can conclude that...")
                sentences.Add("In conclusion, the experiment results support the theory.")
                sentences.Add("This is a premise sentence.")
                sentences.Add("Therefore, the conclusion follows.")

                sentences.Add("Based on the hypothesis, we can conclude that...")
                sentences.Add("In conclusion, the experiment results support the theory.")
                sentences.Add("The question is whether...")
                sentences.Add("The answer to this question is...")
                sentences.Add("Please follow the instructions carefully.")
                sentences.Add("The task requires you to...")

                sentences.AddRange(iGetTrainingSentences)
                Dim analyzer As New ContextAnalyzer()
                Dim statementGroups As List(Of StatementGroup) = analyzer.GroupStatements(sentences)
                ' statementGroups.AddRange(analyzer.GetContextStatements(sentences))
                For Each group As StatementGroup In statementGroups
                    Console.WriteLine("Statement Type: " & group.Type)
                    Console.WriteLine("Sentences:")
                    For Each sentence As String In group.Sentences
                        Console.WriteLine("- " & sentence)
                    Next
                    Console.WriteLine()
                Next
                Console.ReadLine()
            End Sub
            ''' <summary>
            ''' Attempts to find context Sentences for discovered premise of conclusions or hypotheses etc
            ''' </summary>
            ''' <param name="sentences"></param>
            ''' <returns>Type EG Premise / Partner Sentences  conclusions / hypotheses</returns>
            Public Function GetContextStatements(ByVal sentences As List(Of String)) As Dictionary(Of String, List(Of String))
                Dim statementGroups As New Dictionary(Of String, List(Of String))()
                Dim previousSentenceType As String = ""


                Dim sentenceTypes = SentenceClassifier.ClassifySentences(sentences)
                For Each item In sentenceTypes
                    Dim SentenceType = item.Type
                    ' Perform context analysis based on the previous sentence type and current sentence
                    ' Apply rules or patterns to group the sentences accordingly

                    ' Example rule: If the current sentence is a premise and the previous sentence is a conclusion, group them together
                    If SentenceType = "Premise" AndAlso previousSentenceType = "Conclusion" Then
                        ' Check if the group exists, otherwise create a new group
                        If Not statementGroups.ContainsKey(previousSentenceType) Then
                            statementGroups(previousSentenceType) = New List(Of String)()
                        End If

                        ' Add the current premise sentence to the existing group
                        statementGroups(previousSentenceType).Add(item.Entity.Sentence)
                    End If

                    ' Example rule: If the current sentence is a Conclusion and the previous sentence is a Hypotheses, group them together
                    If SentenceType = "Conclusion" AndAlso previousSentenceType = "Hypotheses" Then
                        ' Check if the group exists, otherwise create a new group
                        If Not statementGroups.ContainsKey(previousSentenceType) Then
                            statementGroups(previousSentenceType) = New List(Of String)()
                        End If

                        ' Add the current premise sentence to the existing group
                        statementGroups(previousSentenceType).Add(item.Entity.Sentence)
                    End If

                    ' Example rule: If the current sentence is a premise and the previous sentence is a Hypotheses, group them together
                    If SentenceType = "Premise" AndAlso previousSentenceType = "Hypotheses" Then
                        ' Check if the group exists, otherwise create a new group
                        If Not statementGroups.ContainsKey(previousSentenceType) Then
                            statementGroups(previousSentenceType) = New List(Of String)()
                        End If

                        ' Add the current premise sentence to the existing group
                        statementGroups(previousSentenceType).Add(item.Entity.Sentence)
                    End If
                    ' Add more rules or patterns to group other sentence types

                    ' Update the previous sentence type for the next iteration
                    previousSentenceType = SentenceType
                Next

                ' Return the grouped statements
                Return statementGroups
            End Function
        End Class
    End Namespace
End Namespace