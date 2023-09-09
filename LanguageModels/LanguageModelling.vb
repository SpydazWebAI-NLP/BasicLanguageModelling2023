Imports System.Text.RegularExpressions
Imports LanguageModelling.LanguageModels

Namespace Common_NLP_Tasks

    Namespace LanguageModelling


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

            Private Function CalculateIDF(documents As List(Of List(Of String))) As Dictionary(Of String, Double)
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

            Private Function CalculateTermFrequency(sentence As List(Of String)) As Dictionary(Of String, Double)
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
        Public Class SimilarityCalculator

            Public Shared Function CalculateCosineSimilarity(sentences1 As List(Of String), sentences2 As List(Of String)) As Double
                Dim vectorizer As New SentenceVectorizer()
                Dim vector1 = vectorizer.Vectorize(sentences1)
                Dim vector2 = vectorizer.Vectorize(sentences2)

                Return SimilarityCalculator.CalculateCosineSimilarity(vector1, vector2)
            End Function

            Public Shared Function CalculateCosineSimilarity(vector1 As List(Of Double), vector2 As List(Of Double)) As Double
                If vector1.Count <> vector2.Count Then
                    Throw New ArgumentException("Vector dimensions do not match.")
                End If

                Dim dotProduct As Double = 0
                Dim magnitude1 As Double = 0
                Dim magnitude2 As Double = 0

                For i As Integer = 0 To vector1.Count - 1
                    dotProduct += vector1(i) * vector2(i)
                    magnitude1 += Math.Pow(vector1(i), 2)
                    magnitude2 += Math.Pow(vector2(i), 2)
                Next

                magnitude1 = Math.Sqrt(magnitude1)
                magnitude2 = Math.Sqrt(magnitude2)

                Return dotProduct / (magnitude1 * magnitude2)
            End Function

            Public Shared Function CalculateJaccardSimilarity(sentences1 As List(Of String), sentences2 As List(Of String)) As Double
                Dim set1 As New HashSet(Of String)(sentences1)
                Dim set2 As New HashSet(Of String)(sentences2)

                Return SimilarityCalculator.CalculateJaccardSimilarity(set1, set2)
            End Function

            Public Shared Function CalculateJaccardSimilarity(set1 As HashSet(Of String), set2 As HashSet(Of String)) As Double
                Dim intersectionCount As Integer = set1.Intersect(set2).Count()
                Dim unionCount As Integer = set1.Union(set2).Count()

                Return CDbl(intersectionCount) / CDbl(unionCount)
            End Function

        End Class
        ''' <summary>
        ''' The removal of commonly used words which are only used to create a sentence such as,
        ''' the, on, in of, but
        ''' </summary>
        Public Class RemoveStopWords

            Public StopWords As New List(Of String)

            Public StopWordsArab() As String = {"،", "آض", "آمينَ", "آه",
                "آهاً", "آي", "أ", "أب", "أجل", "أجمع", "أخ", "أخذ", "أصبح", "أضحى", "أقبل",
                "أقل", "أكثر", "ألا", "أم", "أما", "أمامك", "أمامكَ", "أمسى", "أمّا", "أن", "أنا", "أنت",
                "أنتم", "أنتما", "أنتن", "أنتِ", "أنشأ", "أنّى", "أو", "أوشك", "أولئك", "أولئكم", "أولاء",
                "أولالك", "أوّهْ", "أي", "أيا", "أين", "أينما", "أيّ", "أَنَّ", "أََيُّ", "أُفٍّ", "إذ", "إذا", "إذاً",
                "إذما", "إذن", "إلى", "إليكم", "إليكما", "إليكنّ", "إليكَ", "إلَيْكَ", "إلّا", "إمّا", "إن", "إنّما",
                "إي", "إياك", "إياكم", "إياكما", "إياكن", "إيانا", "إياه", "إياها", "إياهم", "إياهما", "إياهن",
                "إياي", "إيهٍ", "إِنَّ", "ا", "ابتدأ", "اثر", "اجل", "احد", "اخرى", "اخلولق", "اذا", "اربعة", "ارتدّ",
                "استحال", "اطار", "اعادة", "اعلنت", "اف", "اكثر", "اكد", "الألاء", "الألى", "الا", "الاخيرة", "الان", "الاول",
                "الاولى", "التى", "التي", "الثاني", "الثانية", "الذاتي", "الذى", "الذي", "الذين", "السابق", "الف", "اللائي",
                "اللاتي", "اللتان", "اللتيا", "اللتين", "اللذان", "اللذين", "اللواتي", "الماضي", "المقبل", "الوقت", "الى",
                "اليوم", "اما", "امام", "امس", "ان", "انبرى", "انقلب", "انه", "انها", "او", "اول", "اي", "ايار", "ايام",
                "ايضا", "ب", "بات", "باسم", "بان", "بخٍ", "برس", "بسبب", "بسّ", "بشكل", "بضع", "بطآن", "بعد", "بعض", "بك",
                "بكم", "بكما", "بكن", "بل", "بلى", "بما", "بماذا", "بمن", "بن", "بنا", "به", "بها", "بي", "بيد", "بين",
                "بَسْ", "بَلْهَ", "بِئْسَ", "تانِ", "تانِك", "تبدّل", "تجاه", "تحوّل", "تلقاء", "تلك", "تلكم", "تلكما", "تم", "تينك",
                "تَيْنِ", "تِه", "تِي", "ثلاثة", "ثم", "ثمّ", "ثمّة", "ثُمَّ", "جعل", "جلل", "جميع", "جير", "حار", "حاشا", "حاليا",
                "حاي", "حتى", "حرى", "حسب", "حم", "حوالى", "حول", "حيث", "حيثما", "حين", "حيَّ", "حَبَّذَا", "حَتَّى", "حَذارِ", "خلا",
                "خلال", "دون", "دونك", "ذا", "ذات", "ذاك", "ذانك", "ذانِ", "ذلك", "ذلكم", "ذلكما", "ذلكن", "ذو", "ذوا", "ذواتا", "ذواتي", "ذيت", "ذينك", "ذَيْنِ", "ذِه", "ذِي", "راح", "رجع", "رويدك", "ريث", "رُبَّ", "زيارة", "سبحان", "سرعان", "سنة", "سنوات", "سوف", "سوى", "سَاءَ", "سَاءَمَا", "شبه", "شخصا", "شرع", "شَتَّانَ", "صار", "صباح", "صفر", "صهٍ", "صهْ", "ضد", "ضمن", "طاق", "طالما", "طفق", "طَق", "ظلّ", "عاد", "عام", "عاما", "عامة", "عدا", "عدة", "عدد", "عدم", "عسى", "عشر", "عشرة", "علق", "على", "عليك", "عليه", "عليها", "علًّ", "عن", "عند", "عندما", "عوض", "عين", "عَدَسْ", "عَمَّا", "غدا", "غير", "ـ", "ف", "فان", "فلان", "فو", "فى", "في", "فيم", "فيما", "فيه", "فيها", "قال", "قام", "قبل", "قد", "قطّ", "قلما", "قوة", "كأنّما", "كأين", "كأيّ", "كأيّن", "كاد", "كان", "كانت", "كذا", "كذلك", "كرب", "كل", "كلا", "كلاهما", "كلتا", "كلم", "كليكما", "كليهما", "كلّما", "كلَّا", "كم", "كما", "كي", "كيت", "كيف", "كيفما", "كَأَنَّ", "كِخ", "لئن", "لا", "لات", "لاسيما", "لدن", "لدى", "لعمر", "لقاء", "لك", "لكم", "لكما", "لكن", "لكنَّما", "لكي", "لكيلا", "للامم", "لم", "لما", "لمّا", "لن", "لنا", "له", "لها", "لو", "لوكالة", "لولا", "لوما", "لي", "لَسْتَ", "لَسْتُ", "لَسْتُم", "لَسْتُمَا", "لَسْتُنَّ", "لَسْتِ", "لَسْنَ", "لَعَلَّ", "لَكِنَّ", "لَيْتَ", "لَيْسَ", "لَيْسَا", "لَيْسَتَا", "لَيْسَتْ", "لَيْسُوا", "لَِسْنَا", "ما", "ماانفك", "مابرح", "مادام", "ماذا", "مازال", "مافتئ", "مايو", "متى", "مثل", "مذ", "مساء", "مع", "معاذ", "مقابل", "مكانكم", "مكانكما", "مكانكنّ", "مكانَك", "مليار", "مليون", "مما", "ممن", "من", "منذ", "منها", "مه", "مهما", "مَنْ", "مِن", "نحن", "نحو", "نعم", "نفس", "نفسه", "نهاية", "نَخْ", "نِعِمّا", "نِعْمَ", "ها", "هاؤم", "هاكَ", "هاهنا", "هبّ", "هذا", "هذه", "هكذا", "هل", "هلمَّ", "هلّا", "هم", "هما", "هن", "هنا", "هناك", "هنالك", "هو", "هي", "هيا", "هيت", "هيّا", "هَؤلاء", "هَاتانِ", "هَاتَيْنِ", "هَاتِه", "هَاتِي", "هَجْ", "هَذا", "هَذانِ", "هَذَيْنِ", "هَذِه", "هَذِي", "هَيْهَاتَ", "و", "و6", "وا", "واحد", "واضاف", "واضافت", "واكد", "وان", "واهاً", "واوضح", "وراءَك", "وفي", "وقال", "وقالت", "وقد", "وقف", "وكان", "وكانت", "ولا", "ولم",
                "ومن", "وهو", "وهي", "ويكأنّ", "وَيْ", "وُشْكَانََ", "يكون", "يمكن", "يوم", "ّأيّان"}

            Public StopWordsDutch() As String = {"aan", "achte", "achter", "af", "al", "alle", "alleen", "alles", "als", "ander", "anders", "beetje",
        "behalve", "beide", "beiden", "ben", "beneden", "bent", "bij", "bijna", "bijv", "blijkbaar", "blijken", "boven", "bv",
        "daar", "daardoor", "daarin", "daarna", "daarom", "daaruit", "dan", "dat", "de", "deden", "deed", "derde", "derhalve", "dertig",
        "deze", "dhr", "die", "dit", "doe", "doen", "doet", "door", "drie", "duizend", "echter", "een", "eens", "eerst", "eerste", "eigen",
        "eigenlijk", "elk", "elke", "en", "enige", "er", "erg", "ergens", "etc", "etcetera", "even", "geen", "genoeg", "geweest", "haar",
        "haarzelf", "had", "hadden", "heb", "hebben", "hebt", "hedden", "heeft", "heel", "hem", "hemzelf", "hen", "het", "hetzelfde",
        "hier", "hierin", "hierna", "hierom", "hij", "hijzelf", "hoe", "honderd", "hun", "ieder", "iedere", "iedereen", "iemand", "iets",
        "ik", "in", "inderdaad", "intussen", "is", "ja", "je", "jij", "jijzelf", "jou", "jouw", "jullie", "kan", "kon", "konden", "kun",
        "kunnen", "kunt", "laatst", "later", "lijken", "lijkt", "maak", "maakt", "maakte", "maakten", "maar", "mag", "maken", "me", "meer",
        "meest", "meestal", "men", "met", "mevr", "mij", "mijn", "minder", "miss", "misschien", "missen", "mits", "mocht", "mochten",
        "moest", "moesten", "moet", "moeten", "mogen", "mr", "mrs", "mw", "na", "naar", "nam", "namelijk", "nee", "neem", "negen",
        "nemen", "nergens", "niemand", "niet", "niets", "niks", "noch", "nochtans", "nog", "nooit", "nu", "nv", "of", "om", "omdat",
        "ondanks", "onder", "ondertussen", "ons", "onze", "onzeker", "ooit", "ook", "op", "over", "overal", "overige", "paar", "per",
        "recent", "redelijk", "samen", "sinds", "steeds", "te", "tegen", "tegenover", "thans", "tien", "tiende", "tijdens", "tja", "toch",
        "toe", "tot", "totdat", "tussen", "twee", "tweede", "u", "uit", "uw", "vaak", "van", "vanaf", "veel", "veertig", "verder",
        "verscheidene", "verschillende", "via", "vier", "vierde", "vijf", "vijfde", "vijftig", "volgend", "volgens", "voor", "voordat",
        "voorts", "waar", "waarom", "waarschijnlijk", "wanneer", "waren", "was", "wat", "we", "wederom", "weer", "weinig", "wel", "welk",
        "welke", "werd", "werden", "werder", "whatever", "wie", "wij", "wijzelf", "wil", "wilden", "willen", "word", "worden", "wordt", "zal",
        "ze", "zei", "zeker", "zelf", "zelfde", "zes", "zeven", "zich", "zij", "zijn", "zijzelf", "zo", "zoals", "zodat", "zou", "zouden",
        "zulk", "zullen"}

            Public StopWordsENG() As String = {"a", "as", "able", "about", "above", "according", "accordingly", "across", "actually", "after", "afterwards", "again", "against", "aint",
                        "all", "allow", "allows", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "an", "and", "another", "any",
        "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "arent", "around",
        "as", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "b", "be", "became", "because", "become", "becomes", "becoming",
        "been", "before", "beforehand", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief",
        "but", "by", "c", "cmon", "cs", "came", "can", "cant", "cannot", "cant", "cause", "causes", "certain", "certainly", "changes", "clearly", "co", "com",
        "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldnt",
        "course", "currently", "d", "definitely", "described", "despite", "did", "didnt", "different", "do", "does", "doesnt", "doing", "dont", "done", "down",
        "downwards", "during", "e", "each", "edu", "eg", "eight", "either", "else", "elsewhere", "enough", "entirely", "especially", "et", "etc", "even", "ever",
        "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "f", "far", "few", "fifth", "first", "five", "followed",
        "following", "follows", "for", "former", "formerly", "forth", "four", "from", "further", "furthermore", "g", "get", "gets", "getting", "given", "gives",
        "go", "goes", "going", "gone", "got", "gotten", "greetings", "h", "had", "hadnt", "happens", "hardly", "has", "hasnt", "have", "havent", "having", "he",
        "hes", "hello", "help", "hence", "her", "here", "heres", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "hi", "him", "himself", "his",
        "hither", "hopefully", "how", "howbeit", "however", "i", "id", "ill", "im", "ive", "ie", "if", "ignored", "immediate", "in", "inasmuch", "inc", "indeed",
        "indicate", "indicated", "indicates", "inner", "insofar", "instead", "into", "inward", "is", "isnt", "it", "itd", "itll", "its", "its", "itself", "j",
        "just", "k", "keep", "keeps", "kept", "know", "known", "knows", "l", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "lets",
        "like", "liked", "likely", "little", "look", "looking", "looks", "ltd", "m", "mainly", "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might",
        "more", "moreover", "most", "mostly", "much", "must", "my", "myself", "n", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needs", "neither",
        "never", "nevertheless", "new", "next", "nine", "no", "nobody", "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "o",
        "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto", "or", "other", "others", "otherwise", "ought", "our",
        "ours", "ourselves", "out", "outside", "over", "overall", "own", "p", "particular", "particularly", "per", "perhaps", "placed", "please", "plus", "possible",
        "presumably", "probably", "provides", "q", "que", "quite", "qv", "r", "rather", "rd", "re", "really", "reasonably", "regarding", "regardless", "regards",
        "relatively", "respectively", "right", "s", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming",
        "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "she", "should", "shouldnt", "since", "six", "so",
        "some", "somebody", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying",
        "still", "sub", "such", "sup", "sure", "t", "ts", "take", "taken", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "thats", "thats", "the",
        "their", "theirs", "them", "themselves", "then", "thence", "there", "theres", "thereafter", "thereby", "therefore", "therein", "theres", "thereupon",
        "these", "they", "theyd", "theyll", "theyre", "theyve", "think", "third", "this", "thorough", "thoroughly", "those", "though", "three", "through",
        "throughout", "thru", "thus", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "twice", "two", "u", "un",
        "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us", "use", "used", "useful", "uses", "using", "usually", "uucp", "v",
        "value", "various", "very", "via", "viz", "vs", "w", "want", "wants", "was", "wasnt", "way", "we", "wed", "well", "were", "weve", "welcome", "well",
        "went", "were", "werent", "what", "whats", "whatever", "when", "whence", "whenever", "where", "wheres", "whereafter", "whereas", "whereby", "wherein",
        "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whos", "whoever", "whole", "whom", "whose", "why", "will", "willing", "wish",
        "with", "within", "without", "wont", "wonder", "would", "wouldnt", "x", "y", "yes", "yet", "you", "youd", "youll", "youre", "youve", "your", "yours",
        "yourself", "yourselves", "youll", "z", "zero"}

            Public StopWordsFrench() As String = {"a", "abord", "absolument", "afin", "ah", "ai", "aie", "ailleurs", "ainsi", "ait", "allaient", "allo", "allons",
        "allô", "alors", "anterieur", "anterieure", "anterieures", "apres", "après", "as", "assez", "attendu", "au", "aucun", "aucune",
        "aujourd", "aujourd'hui", "aupres", "auquel", "aura", "auraient", "aurait", "auront", "aussi", "autre", "autrefois", "autrement",
        "autres", "autrui", "aux", "auxquelles", "auxquels", "avaient", "avais", "avait", "avant", "avec", "avoir", "avons", "ayant", "b",
        "bah", "bas", "basee", "bat", "beau", "beaucoup", "bien", "bigre", "boum", "bravo", "brrr", "c", "car", "ce", "ceci", "cela", "celle",
        "celle-ci", "celle-là", "celles", "celles-ci", "celles-là", "celui", "celui-ci", "celui-là", "cent", "cependant", "certain",
        "certaine", "certaines", "certains", "certes", "ces", "cet", "cette", "ceux", "ceux-ci", "ceux-là", "chacun", "chacune", "chaque",
        "cher", "chers", "chez", "chiche", "chut", "chère", "chères", "ci", "cinq", "cinquantaine", "cinquante", "cinquantième", "cinquième",
        "clac", "clic", "combien", "comme", "comment", "comparable", "comparables", "compris", "concernant", "contre", "couic", "crac", "d",
        "da", "dans", "de", "debout", "dedans", "dehors", "deja", "delà", "depuis", "dernier", "derniere", "derriere", "derrière", "des",
        "desormais", "desquelles", "desquels", "dessous", "dessus", "deux", "deuxième", "deuxièmement", "devant", "devers", "devra",
        "different", "differentes", "differents", "différent", "différente", "différentes", "différents", "dire", "directe", "directement",
        "dit", "dite", "dits", "divers", "diverse", "diverses", "dix", "dix-huit", "dix-neuf", "dix-sept", "dixième", "doit", "doivent", "donc",
        "dont", "douze", "douzième", "dring", "du", "duquel", "durant", "dès", "désormais", "e", "effet", "egale", "egalement", "egales", "eh",
        "elle", "elle-même", "elles", "elles-mêmes", "en", "encore", "enfin", "entre", "envers", "environ", "es", "est", "et", "etant", "etc",
        "etre", "eu", "euh", "eux", "eux-mêmes", "exactement", "excepté", "extenso", "exterieur", "f", "fais", "faisaient", "faisant", "fait",
        "façon", "feront", "fi", "flac", "floc", "font", "g", "gens", "h", "ha", "hein", "hem", "hep", "hi", "ho", "holà", "hop", "hormis", "hors",
        "hou", "houp", "hue", "hui", "huit", "huitième", "hum", "hurrah", "hé", "hélas", "i", "il", "ils", "importe", "j", "je", "jusqu", "jusque",
        "juste", "k", "l", "la", "laisser", "laquelle", "las", "le", "lequel", "les", "lesquelles", "lesquels", "leur", "leurs", "longtemps",
        "lors", "lorsque", "lui", "lui-meme", "lui-même", "là", "lès", "m", "ma", "maint", "maintenant", "mais", "malgre", "malgré", "maximale",
        "me", "meme", "memes", "merci", "mes", "mien", "mienne", "miennes", "miens", "mille", "mince", "minimale", "moi", "moi-meme", "moi-même",
        "moindres", "moins", "mon", "moyennant", "multiple", "multiples", "même", "mêmes", "n", "na", "naturel", "naturelle", "naturelles", "ne",
        "neanmoins", "necessaire", "necessairement", "neuf", "neuvième", "ni", "nombreuses", "nombreux", "non", "nos", "notamment", "notre",
        "nous", "nous-mêmes", "nouveau", "nul", "néanmoins", "nôtre", "nôtres", "o", "oh", "ohé", "ollé", "olé", "on", "ont", "onze", "onzième",
        "ore", "ou", "ouf", "ouias", "oust", "ouste", "outre", "ouvert", "ouverte", "ouverts", "o|", "où", "p", "paf", "pan", "par", "parce",
        "parfois", "parle", "parlent", "parler", "parmi", "parseme", "partant", "particulier", "particulière", "particulièrement", "pas",
        "passé", "pendant", "pense", "permet", "personne", "peu", "peut", "peuvent", "peux", "pff", "pfft", "pfut", "pif", "pire", "plein",
        "plouf", "plus", "plusieurs", "plutôt", "possessif", "possessifs", "possible", "possibles", "pouah", "pour", "pourquoi", "pourrais",
        "pourrait", "pouvait", "prealable", "precisement", "premier", "première", "premièrement", "pres", "probable", "probante",
        "procedant", "proche", "près", "psitt", "pu", "puis", "puisque", "pur", "pure", "q", "qu", "quand", "quant", "quant-à-soi", "quanta",
        "quarante", "quatorze", "quatre", "quatre-vingt", "quatrième", "quatrièmement", "que", "quel", "quelconque", "quelle", "quelles",
        "quelqu'un", "quelque", "quelques", "quels", "qui", "quiconque", "quinze", "quoi", "quoique", "r", "rare", "rarement", "rares",
        "relative", "relativement", "remarquable", "rend", "rendre", "restant", "reste", "restent", "restrictif", "retour", "revoici",
        "revoilà", "rien", "s", "sa", "sacrebleu", "sait", "sans", "sapristi", "sauf", "se", "sein", "seize", "selon", "semblable", "semblaient",
        "semble", "semblent", "sent", "sept", "septième", "sera", "seraient", "serait", "seront", "ses", "seul", "seule", "seulement", "si",
        "sien", "sienne", "siennes", "siens", "sinon", "six", "sixième", "soi", "soi-même", "soit", "soixante", "son", "sont", "sous", "souvent",
        "specifique", "specifiques", "speculatif", "stop", "strictement", "subtiles", "suffisant", "suffisante", "suffit", "suis", "suit",
        "suivant", "suivante", "suivantes", "suivants", "suivre", "superpose", "sur", "surtout", "t", "ta", "tac", "tant", "tardive", "te",
        "tel", "telle", "tellement", "telles", "tels", "tenant", "tend", "tenir", "tente", "tes", "tic", "tien", "tienne", "tiennes", "tiens",
        "toc", "toi", "toi-même", "ton", "touchant", "toujours", "tous", "tout", "toute", "toutefois", "toutes", "treize", "trente", "tres",
        "trois", "troisième", "troisièmement", "trop", "très", "tsoin", "tsouin", "tu", "té", "u", "un", "une", "unes", "uniformement", "unique",
        "uniques", "uns", "v", "va", "vais", "vas", "vers", "via", "vif", "vifs", "vingt", "vivat", "vive", "vives", "vlan", "voici", "voilà",
        "vont", "vos", "votre", "vous", "vous-mêmes", "vu", "vé", "vôtre", "vôtres", "w", "x", "y", "z", "zut", "à", "â", "ça", "ès", "étaient",
        "étais", "était", "étant", "été", "être", "ô"}

            Public StopWordsItalian() As String = {"IE", "a", "abbastanza", "abbia", "abbiamo", "abbiano", "abbiate", "accidenti", "ad", "adesso", "affinche", "agl", "agli",
                "ahime", "ahimè", "ai", "al", "alcuna", "alcuni", "alcuno", "all", "alla", "alle", "allo", "allora", "altri", "altrimenti", "altro",
        "altrove", "altrui", "anche", "ancora", "anni", "anno", "ansa", "anticipo", "assai", "attesa", "attraverso", "avanti", "avemmo",
        "avendo", "avente", "aver", "avere", "averlo", "avesse", "avessero", "avessi", "avessimo", "aveste", "avesti", "avete", "aveva",
        "avevamo", "avevano", "avevate", "avevi", "avevo", "avrai", "avranno", "avrebbe", "avrebbero", "avrei", "avremmo", "avremo",
        "avreste", "avresti", "avrete", "avrà", "avrò", "avuta", "avute", "avuti", "avuto", "basta", "bene", "benissimo", "berlusconi",
        "brava", "bravo", "c", "casa", "caso", "cento", "certa", "certe", "certi", "certo", "che", "chi", "chicchessia", "chiunque", "ci",
        "ciascuna", "ciascuno", "cima", "cio", "cioe", "cioè", "circa", "citta", "città", "ciò", "co", "codesta", "codesti", "codesto",
        "cogli", "coi", "col", "colei", "coll", "coloro", "colui", "come", "cominci", "comunque", "con", "concernente", "conciliarsi",
        "conclusione", "consiglio", "contro", "cortesia", "cos", "cosa", "cosi", "così", "cui", "d", "da", "dagl", "dagli", "dai", "dal",
        "dall", "dalla", "dalle", "dallo", "dappertutto", "davanti", "degl", "degli", "dei", "del", "dell", "della", "delle", "dello",
        "dentro", "detto", "deve", "di", "dice", "dietro", "dire", "dirimpetto", "diventa", "diventare", "diventato", "dopo", "dov", "dove",
        "dovra", "dovrà", "dovunque", "due", "dunque", "durante", "e", "ebbe", "ebbero", "ebbi", "ecc", "ecco", "ed", "effettivamente", "egli",
        "ella", "entrambi", "eppure", "era", "erano", "eravamo", "eravate", "eri", "ero", "esempio", "esse", "essendo", "esser", "essere",
        "essi", "ex", "fa", "faccia", "facciamo", "facciano", "facciate", "faccio", "facemmo", "facendo", "facesse", "facessero", "facessi",
        "facessimo", "faceste", "facesti", "faceva", "facevamo", "facevano", "facevate", "facevi", "facevo", "fai", "fanno", "farai",
        "faranno", "fare", "farebbe", "farebbero", "farei", "faremmo", "faremo", "fareste", "faresti", "farete", "farà", "farò", "fatto",
        "favore", "fece", "fecero", "feci", "fin", "finalmente", "finche", "fine", "fino", "forse", "forza", "fosse", "fossero", "fossi",
        "fossimo", "foste", "fosti", "fra", "frattempo", "fu", "fui", "fummo", "fuori", "furono", "futuro", "generale", "gia", "giacche",
        "giorni", "giorno", "già", "gli", "gliela", "gliele", "glieli", "glielo", "gliene", "governo", "grande", "grazie", "gruppo", "ha",
        "haha", "hai", "hanno", "ho", "i", "ieri", "il", "improvviso", "in", "inc", "infatti", "inoltre", "insieme", "intanto", "intorno",
        "invece", "io", "l", "la", "lasciato", "lato", "lavoro", "le", "lei", "li", "lo", "lontano", "loro", "lui", "lungo", "luogo", "là",
        "ma", "macche", "magari", "maggior", "mai", "male", "malgrado", "malissimo", "mancanza", "marche", "me", "medesimo", "mediante",
        "meglio", "meno", "mentre", "mesi", "mezzo", "mi", "mia", "mie", "miei", "mila", "miliardi", "milioni", "minimi", "ministro",
        "mio", "modo", "molti", "moltissimo", "molto", "momento", "mondo", "mosto", "nazionale", "ne", "negl", "negli", "nei", "nel",
        "nell", "nella", "nelle", "nello", "nemmeno", "neppure", "nessun", "nessuna", "nessuno", "niente", "no", "noi", "non", "nondimeno",
        "nonostante", "nonsia", "nostra", "nostre", "nostri", "nostro", "novanta", "nove", "nulla", "nuovo", "o", "od", "oggi", "ogni",
        "ognuna", "ognuno", "oltre", "oppure", "ora", "ore", "osi", "ossia", "ottanta", "otto", "paese", "parecchi", "parecchie",
        "parecchio", "parte", "partendo", "peccato", "peggio", "per", "perche", "perchè", "perché", "percio", "perciò", "perfino", "pero",
        "persino", "persone", "però", "piedi", "pieno", "piglia", "piu", "piuttosto", "più", "po", "pochissimo", "poco", "poi", "poiche",
        "possa", "possedere", "posteriore", "posto", "potrebbe", "preferibilmente", "presa", "press", "prima", "primo", "principalmente",
        "probabilmente", "proprio", "puo", "pure", "purtroppo", "può", "qualche", "qualcosa", "qualcuna", "qualcuno", "quale", "quali",
        "qualunque", "quando", "quanta", "quante", "quanti", "quanto", "quantunque", "quasi", "quattro", "quel", "quella", "quelle",
        "quelli", "quello", "quest", "questa", "queste", "questi", "questo", "qui", "quindi", "realmente", "recente", "recentemente",
        "registrazione", "relativo", "riecco", "salvo", "sara", "sarai", "saranno", "sarebbe", "sarebbero", "sarei", "saremmo", "saremo",
        "sareste", "saresti", "sarete", "sarà", "sarò", "scola", "scopo", "scorso", "se", "secondo", "seguente", "seguito", "sei", "sembra",
        "sembrare", "sembrato", "sembri", "sempre", "senza", "sette", "si", "sia", "siamo", "siano", "siate", "siete", "sig", "solito",
        "solo", "soltanto", "sono", "sopra", "sotto", "spesso", "srl", "sta", "stai", "stando", "stanno", "starai", "staranno", "starebbe",
        "starebbero", "starei", "staremmo", "staremo", "stareste", "staresti", "starete", "starà", "starò", "stata", "state", "stati",
        "stato", "stava", "stavamo", "stavano", "stavate", "stavi", "stavo", "stemmo", "stessa", "stesse", "stessero", "stessi", "stessimo",
        "stesso", "steste", "stesti", "stette", "stettero", "stetti", "stia", "stiamo", "stiano", "stiate", "sto", "su", "sua", "subito",
        "successivamente", "successivo", "sue", "sugl", "sugli", "sui", "sul", "sull", "sulla", "sulle", "sullo", "suo", "suoi", "tale",
        "tali", "talvolta", "tanto", "te", "tempo", "ti", "titolo", "torino", "tra", "tranne", "tre", "trenta", "troppo", "trovato", "tu",
        "tua", "tue", "tuo", "tuoi", "tutta", "tuttavia", "tutte", "tutti", "tutto", "uguali", "ulteriore", "ultimo", "un", "una", "uno",
        "uomo", "va", "vale", "vari", "varia", "varie", "vario", "verso", "vi", "via", "vicino", "visto", "vita", "voi", "volta", "volte",
        "vostra", "vostre", "vostri", "vostro", "è"}

            Public StopWordsSpanish() As String = {"a", "actualmente", "acuerdo", "adelante", "ademas", "además", "adrede", "afirmó", "agregó", "ahi", "ahora",
        "ahí", "al", "algo", "alguna", "algunas", "alguno", "algunos", "algún", "alli", "allí", "alrededor", "ambos", "ampleamos",
        "antano", "antaño", "ante", "anterior", "antes", "apenas", "aproximadamente", "aquel", "aquella", "aquellas", "aquello",
        "aquellos", "aqui", "aquél", "aquélla", "aquéllas", "aquéllos", "aquí", "arriba", "arribaabajo", "aseguró", "asi", "así",
        "atras", "aun", "aunque", "ayer", "añadió", "aún", "b", "bajo", "bastante", "bien", "breve", "buen", "buena", "buenas", "bueno",
        "buenos", "c", "cada", "casi", "cerca", "cierta", "ciertas", "cierto", "ciertos", "cinco", "claro", "comentó", "como", "con",
        "conmigo", "conocer", "conseguimos", "conseguir", "considera", "consideró", "consigo", "consigue", "consiguen", "consigues",
        "contigo", "contra", "cosas", "creo", "cual", "cuales", "cualquier", "cuando", "cuanta", "cuantas", "cuanto", "cuantos", "cuatro",
        "cuenta", "cuál", "cuáles", "cuándo", "cuánta", "cuántas", "cuánto", "cuántos", "cómo", "d", "da", "dado", "dan", "dar", "de",
        "debajo", "debe", "deben", "debido", "decir", "dejó", "del", "delante", "demasiado", "demás", "dentro", "deprisa", "desde",
        "despacio", "despues", "después", "detras", "detrás", "dia", "dias", "dice", "dicen", "dicho", "dieron", "diferente", "diferentes",
        "dijeron", "dijo", "dio", "donde", "dos", "durante", "día", "días", "dónde", "e", "ejemplo", "el", "ella", "ellas", "ello", "ellos",
        "embargo", "empleais", "emplean", "emplear", "empleas", "empleo", "en", "encima", "encuentra", "enfrente", "enseguida", "entonces",
        "entre", "era", "eramos", "eran", "eras", "eres", "es", "esa", "esas", "ese", "eso", "esos", "esta", "estaba", "estaban", "estado",
        "estados", "estais", "estamos", "estan", "estar", "estará", "estas", "este", "esto", "estos", "estoy", "estuvo", "está", "están", "ex",
        "excepto", "existe", "existen", "explicó", "expresó", "f", "fin", "final", "fue", "fuera", "fueron", "fui", "fuimos", "g", "general",
        "gran", "grandes", "gueno", "h", "ha", "haber", "habia", "habla", "hablan", "habrá", "había", "habían", "hace", "haceis", "hacemos",
        "hacen", "hacer", "hacerlo", "haces", "hacia", "haciendo", "hago", "han", "hasta", "hay", "haya", "he", "hecho", "hemos", "hicieron",
        "hizo", "horas", "hoy", "hubo", "i", "igual", "incluso", "indicó", "informo", "informó", "intenta", "intentais", "intentamos", "intentan",
        "intentar", "intentas", "intento", "ir", "j", "junto", "k", "l", "la", "lado", "largo", "las", "le", "lejos", "les", "llegó", "lleva",
        "llevar", "lo", "los", "luego", "lugar", "m", "mal", "manera", "manifestó", "mas", "mayor", "me", "mediante", "medio", "mejor", "mencionó",
        "menos", "menudo", "mi", "mia", "mias", "mientras", "mio", "mios", "mis", "misma", "mismas", "mismo", "mismos", "modo", "momento", "mucha",
        "muchas", "mucho", "muchos", "muy", "más", "mí", "mía", "mías", "mío", "míos", "n", "nada", "nadie", "ni", "ninguna", "ningunas", "ninguno",
        "ningunos", "ningún", "no", "nos", "nosotras", "nosotros", "nuestra", "nuestras", "nuestro", "nuestros", "nueva", "nuevas", "nuevo",
        "nuevos", "nunca", "o", "ocho", "os", "otra", "otras", "otro", "otros", "p", "pais", "para", "parece", "parte", "partir", "pasada",
        "pasado", "paìs", "peor", "pero", "pesar", "poca", "pocas", "poco", "pocos", "podeis", "podemos", "poder", "podria", "podriais",
        "podriamos", "podrian", "podrias", "podrá", "podrán", "podría", "podrían", "poner", "por", "porque", "posible", "primer", "primera",
        "primero", "primeros", "principalmente", "pronto", "propia", "propias", "propio", "propios", "proximo", "próximo", "próximos", "pudo",
        "pueda", "puede", "pueden", "puedo", "pues", "q", "qeu", "que", "quedó", "queremos", "quien", "quienes", "quiere", "quiza", "quizas",
        "quizá", "quizás", "quién", "quiénes", "qué", "r", "raras", "realizado", "realizar", "realizó", "repente", "respecto", "s", "sabe",
        "sabeis", "sabemos", "saben", "saber", "sabes", "salvo", "se", "sea", "sean", "segun", "segunda", "segundo", "según", "seis", "ser",
        "sera", "será", "serán", "sería", "señaló", "si", "sido", "siempre", "siendo", "siete", "sigue", "siguiente", "sin", "sino", "sobre",
        "sois", "sola", "solamente", "solas", "solo", "solos", "somos", "son", "soy", "soyos", "su", "supuesto", "sus", "suya", "suyas", "suyo",
        "sé", "sí", "sólo", "t", "tal", "tambien", "también", "tampoco", "tan", "tanto", "tarde", "te", "temprano", "tendrá", "tendrán", "teneis",
        "tenemos", "tener", "tenga", "tengo", "tenido", "tenía", "tercera", "ti", "tiempo", "tiene", "tienen", "toda", "todas", "todavia",
        "todavía", "todo", "todos", "total", "trabaja", "trabajais", "trabajamos", "trabajan", "trabajar", "trabajas", "trabajo", "tras",
        "trata", "través", "tres", "tu", "tus", "tuvo", "tuya", "tuyas", "tuyo", "tuyos", "tú", "u", "ultimo", "un", "una", "unas", "uno", "unos",
        "usa", "usais", "usamos", "usan", "usar", "usas", "uso", "usted", "ustedes", "v", "va", "vais", "valor", "vamos", "van", "varias", "varios",
        "vaya", "veces", "ver", "verdad", "verdadera", "verdadero", "vez", "vosotras", "vosotros", "voy", "vuestra", "vuestras", "vuestro",
        "vuestros", "w", "x", "y", "ya", "yo", "z", "él", "ésa", "ésas", "ése", "ésos", "ésta", "éstas", "éste", "éstos", "última", "últimas",
        "último", "últimos"}

            ''' <summary>
            ''' Removes StopWords from sentence
            ''' ARAB/ENG/DUTCH/FRENCH/SPANISH/ITALIAN
            ''' Hopefully leaving just relevant words in the user sentence
            ''' Currently Under Revision (takes too many words)
            ''' </summary>
            ''' <param name="Userinput"></param>
            ''' <returns></returns>
            Public Function RemoveStopWords(ByRef Userinput As String) As String
                ' Userinput = LCase(Userinput).Replace("the", "r")
                For Each item In StopWordsENG
                    Userinput = LCase(Userinput).Replace(item, "")
                Next
                For Each item In StopWordsArab
                    Userinput = Userinput.Replace(item, "")
                Next
                For Each item In StopWordsDutch
                    Userinput = Userinput.Replace(item, "")
                Next
                For Each item In StopWordsFrench
                    Userinput = Userinput.Replace(item, "")
                Next
                For Each item In StopWordsItalian
                    Userinput = Userinput.Replace(item, "")
                Next
                For Each item In StopWordsSpanish
                    Userinput = Userinput.Replace(item, "")
                Next
                Return Userinput
            End Function

            ''' <summary>
            ''' Removes Stop words given a list of stop words
            ''' </summary>
            ''' <param name="Userinput">user input</param>
            ''' <param name="Lst">stop word list</param>
            ''' <returns></returns>
            Public Function RemoveStopWords(ByRef Userinput As String, ByRef Lst As List(Of String)) As String
                For Each item In Lst
                    Userinput = LCase(Userinput).Replace(item, "")
                Next
                Return Userinput
            End Function

        End Class
        Public Class Word2VecModel
            Private embeddingMatrix As Double(,)
            Private embeddingSize As Integer
            Private indexToWord As Dictionary(Of Integer, String)
            Private learningRate As Double
            Private negativeSamples As Integer
            Private vocabulary As HashSet(Of String)
            Private weights As Double()
            Private windowSize As Integer
            Private wordToIndex As Dictionary(Of String, Integer)

            Public Sub New(embeddingSize As Integer, learningRate As Double, windowSize As Integer, negativeSamples As Integer)
                Me.embeddingSize = embeddingSize
                Me.learningRate = learningRate
                Me.windowSize = windowSize
                Me.negativeSamples = negativeSamples

                vocabulary = New HashSet(Of String)()
                wordToIndex = New Dictionary(Of String, Integer)()
                indexToWord = New Dictionary(Of Integer, String)()
                weights = New Double(vocabulary.Count - 1) {}
            End Sub

            Public Enum TrainingMethod
                CBOW
                SkipGram
            End Enum
            Private Shared Function RemoveStopwords(text As String, stopwords As List(Of String)) As String
                Dim cleanedText As String = ""
                Dim words As String() = text.Split()

                For Each word As String In words
                    If Not stopwords.Contains(word.ToLower()) Then
                        cleanedText += word + " "
                    End If
                Next

                Return cleanedText.Trim()
            End Function

            ' Helper function to get context words within the window
            Function GetContextWords(ByVal sentence As String(), ByVal targetIndex As Integer) As List(Of String)
                Dim contextWords As New List(Of String)

                For i = Math.Max(0, targetIndex - windowSize) To Math.Min(sentence.Length - 1, targetIndex + windowSize)
                    If i <> targetIndex Then
                        contextWords.Add(sentence(i))
                    End If
                Next

                Return contextWords
            End Function

            Public Shared Sub Main()
                Dim stopwords As List(Of String) = New List(Of String) From {"this", "own", "to", "is", "a", "with", "on", "is", "at", "they", "and", "the", "are", "for"}


                ' Create an instance of the Word2VecModel
                Dim model As New Word2VecModel(embeddingSize:=512, learningRate:=0.01, windowSize:=3, negativeSamples:=8)
                ' Define the sample article
                Dim article As New List(Of String)
                article.Add("Dog breeds and cat breeds are popular choices for pet owners. Dogs are known for their loyalty, companionship, and diverse characteristics. There are various dog breeds, such as Labrador Retrievers, German Shepherds, Golden Retrievers, and Bulldogs. Each breed has its unique traits and temperaments. Labrador Retrievers are friendly and energetic, while German Shepherds are intelligent and protective. Golden Retrievers are gentle and great with families, while Bulldogs are sturdy and have a distinct appearance.")
                article.Add("On the other hand, cat breeds also have their own charm. Cats are independent, agile, and make great companions. Some popular cat breeds include Maine Coons, Siamese cats, Persians, and Bengals. Maine Coons are large and known for their friendly nature. Siamese cats are vocal and have striking blue eyes. Persians are long-haired and have a calm demeanor, while Bengals have a wild appearance with their spotted coat patterns.")
                article.Add("Both dogs and cats bring joy and love to their owners. Whether you prefer dogs or cats, there's a breed out there for everyone's preferences and lifestyles.")
                Dim Cleaned As New List(Of String)
                For Each item In article
                    item = RemoveStopwords(item, stopwords)
                    Cleaned.Add(item)
                Next
                article = Cleaned
                ' Define the sample articles for cats and dogs
                Dim catArticles As New List(Of String)
                catArticles.Add("Maine Coons are one of the largest domestic cat breeds. They have a gentle and friendly nature.")
                catArticles.Add("Siamese cats are known for their striking blue eyes and vocal behavior.")
                catArticles.Add("Persian cats have long-haired coats and a calm demeanor.")
                catArticles.Add("Bengal cats have a wild appearance with their spotted coat patterns.")

                Dim dogArticles As New List(Of String)
                dogArticles.Add("Labrador Retrievers are friendly and energetic dogs.")
                dogArticles.Add("German Shepherd dogs are intelligent and protective.")
                dogArticles.Add("Golden Retrievers are gentle and great with families.")
                dogArticles.Add("Bulldogs have a sturdy build and a distinct appearance.")
                dogArticles.Add("dogs have a sturdy build and a distinct appearance.")


                Cleaned = New List(Of String)
                For Each item In dogArticles
                    item = RemoveStopwords(item, stopwords)
                    Cleaned.Add(item)
                Next
                dogArticles = Cleaned

                Cleaned = New List(Of String)
                For Each item In catArticles
                    item = RemoveStopwords(item, stopwords)
                    Cleaned.Add(item)
                Next
                catArticles = Cleaned
                ' Train the model with the articles
                article.AddRange(dogArticles)
                article.AddRange(catArticles)

                For i = 1 To 100
                    ' Train the model with cat articles
                    model.Train(article)
                Next


                For i = 1 To 100
                    ' Train the model with cat articles
                    model.Train(article, TrainingMethod.CBOW)
                Next


                ' Get the most similar words to "dog" / "cat"
                Dim similarDogWords As List(Of String) = model.GetMostSimilarWords("dogs", topK:=5)
                Dim similarCatWords As List(Of String) = model.GetMostSimilarWords("cats", topK:=5)

                ' Display the output
                Console.WriteLine("Most similar words to 'dog':")
                For Each word As String In similarDogWords
                    Console.WriteLine(word)
                Next
                Console.WriteLine()
                Console.WriteLine("Most similar words to 'cat':")
                For Each word As String In similarCatWords
                    Console.WriteLine(word)
                Next

                Console.WriteLine()

                For i = 1 To 100
                    ' Train the model with  articles
                    model.Train(article, TrainingMethod.SkipGram)
                Next

                ' Get the most similar words to "dog"
                similarDogWords = model.GetMostSimilarWords("dogs", topK:=8)
                ' Get the most similar words to "cat"
                similarCatWords = model.GetMostSimilarWords("cats", topK:=8)

                ' Display the output
                Console.WriteLine("Most similar words to 'dog' using Skip-gram with negative sampling:")
                For Each word As String In similarDogWords
                    Console.WriteLine(word)
                Next
                Console.WriteLine()
                Console.WriteLine("Most similar words to 'cat' using Skip-gram with negative sampling:")
                For Each word As String In similarCatWords
                    Console.WriteLine(word)
                Next
                Console.WriteLine()
                ' Wait for user input to exit
                Console.ReadLine()
            End Sub

            Public Function GetSimilarWords(word As String, topK As Integer) As List(Of String)
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
                Dim similarWords As New List(Of String)()

                Dim count As Integer = 0
                For Each pair In orderedSimilarities
                    similarWords.Add(pair.Key)
                    count += 1
                    If count >= topK Then
                        Exit For
                    End If
                Next

                Return similarWords
            End Function

            Public Function GetEmbedding(word As String) As Double()
                If wordToIndex.ContainsKey(word) Then
                    Dim wordIndex As Integer = wordToIndex(word)
                    Dim vector(embeddingSize - 1) As Double
                    For i As Integer = 0 To embeddingSize - 1
                        vector(i) = embeddingMatrix(wordIndex, i)
                    Next
                    Return vector
                Else
                    Return Nothing
                End If
            End Function

            Public Function GetMostSimilarWords(word As String, topK As Integer) As List(Of String)
                If wordToIndex.ContainsKey(word) Then
                    Dim wordIndex As Integer = wordToIndex(word)
                    Dim wordVector As Double() = GetEmbedding(word)
                    Dim similarities As New List(Of Tuple(Of String, Double))()

                    For i As Integer = 0 To vocabulary.Count - 1
                        If i <> wordIndex Then
                            Dim currentWord As String = indexToWord(i)
                            Dim currentVector As Double() = GetEmbedding(currentWord)
                            Dim similarity As Double = CalculateSimilarity(wordVector, currentVector)
                            similarities.Add(New Tuple(Of String, Double)(currentWord, similarity))
                        End If
                    Next

                    similarities.Sort(Function(x, y) y.Item2.CompareTo(x.Item2))

                    Dim similarWords As New List(Of String)()
                    For i As Integer = 0 To topK - 1
                        similarWords.Add(similarities(i).Item1)
                    Next

                    Return similarWords
                Else
                    Return Nothing
                End If
            End Function

            Public Sub Train(corpus As List(Of String))
                BuildVocabulary(corpus)
                InitializeEmbeddings()
                Dim trainingData As List(Of List(Of Integer)) = GenerateTrainingData(corpus)
                For epoch = 1 To 10
                    For Each sentenceIndices As List(Of Integer) In trainingData
                        For i As Integer = windowSize To sentenceIndices.Count - windowSize - 1
                            Dim targetIndex As Integer = sentenceIndices(i)
                            Dim contextIndices As List(Of Integer) = GetContextIndices(sentenceIndices, i)

                            Dim contextVectors As List(Of Double()) = GetContextVectors(contextIndices)
                            Dim targetVector As Double() = GetTargetVector(targetIndex)

                            UpdateTargetVector(contextVectors, targetVector)
                        Next
                    Next
                Next
            End Sub

            Public Sub Train(corpus As List(Of String), method As TrainingMethod)
                BuildVocabulary(corpus)
                InitializeEmbeddings()
                InitializeWeights()
                Dim trainingData As List(Of List(Of Integer)) = GenerateTrainingData(corpus)
                For Epoch = 1 To 10
                    For Each sentenceIndices As List(Of Integer) In trainingData
                        For i As Integer = windowSize To sentenceIndices.Count - windowSize - 1
                            Dim targetIndex As Integer = sentenceIndices(i)
                            Dim contextIndices As List(Of Integer) = GetContextIndices(sentenceIndices, i)

                            If method = TrainingMethod.CBOW Then
                                TrainCBOW(contextIndices, targetIndex)
                            ElseIf method = TrainingMethod.SkipGram Then
                                TrainSkipGram(targetIndex, contextIndices)
                            End If
                        Next
                    Next
                Next
            End Sub
            Private Sub BuildVocabulary(corpus As List(Of String))
                Dim index As Integer = 0
                For Each sentence As String In corpus
                    Dim cleanedText As String = Regex.Replace(sentence, "[^\w\s]", "").ToLower()
                    Dim tokens As String() = cleanedText.Split()
                    For Each token As String In tokens
                        If Not wordToIndex.ContainsKey(token) Then
                            vocabulary.Add(token)
                            wordToIndex.Add(token, index)
                            indexToWord.Add(index, token)
                            index += 1
                        End If
                    Next
                Next
            End Sub

            Private Function CalculateSimilarity(vector1 As Double(), vector2 As Double()) As Double
                Dim dotProduct As Double = 0.0
                Dim magnitude1 As Double = 0.0
                Dim magnitude2 As Double = 0.0

                For i As Integer = 0 To embeddingSize - 1
                    dotProduct += vector1(i) * vector2(i)
                    magnitude1 += vector1(i) * vector1(i)
                    magnitude2 += vector2(i) * vector2(i)
                Next

                magnitude1 = Math.Sqrt(magnitude1)
                magnitude2 = Math.Sqrt(magnitude2)

                Return dotProduct / (magnitude1 * magnitude2)
            End Function

            Private Function GenerateTrainingData(corpus As List(Of String)) As List(Of List(Of Integer))
                Dim trainingData As New List(Of List(Of Integer))

                For Each sentence As String In corpus
                    Dim cleanedText As String = Regex.Replace(sentence, "[^\w\s]", "").ToLower()
                    Dim tokens As String() = cleanedText.Split()
                    Dim sentenceIndices As New List(Of Integer)()

                    For Each token As String In tokens
                        sentenceIndices.Add(wordToIndex(token))
                    Next

                    trainingData.Add(sentenceIndices)
                Next

                Return trainingData
            End Function

            Private Function GetContextIndices(sentenceIndices As List(Of Integer), targetIndex As Integer) As List(Of Integer)
                Dim contextIndices As New List(Of Integer)

                Dim startIndex As Integer = Math.Max(0, targetIndex - windowSize)
                Dim endIndex As Integer = Math.Min(sentenceIndices.Count - 1, targetIndex + windowSize)

                For i As Integer = startIndex To endIndex
                    If i <> targetIndex Then
                        contextIndices.Add(sentenceIndices(i))
                    End If
                Next

                Return contextIndices
            End Function

            Private Function GetContextVector(contextIndex As Integer) As Double()
                Dim vector(embeddingSize - 1) As Double
                For i As Integer = 0 To embeddingSize - 1
                    vector(i) = embeddingMatrix(contextIndex, i)
                Next
                Return vector
            End Function

            Private Function GetContextVectors(contextIndices As List(Of Integer)) As List(Of Double())
                Dim contextVectors As New List(Of Double())

                For Each contextIndex As Integer In contextIndices
                    Dim vector(embeddingSize - 1) As Double
                    For i As Integer = 0 To embeddingSize - 1
                        vector(i) = embeddingMatrix(contextIndex, i)
                    Next
                    contextVectors.Add(vector)
                Next

                Return contextVectors
            End Function

            Private Function GetTargetVector(targetIndex As Integer) As Double()
                Dim vector(embeddingSize - 1) As Double
                For i As Integer = 0 To embeddingSize - 1
                    vector(i) = embeddingMatrix(targetIndex, i)
                Next
                Return vector
            End Function

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

            Private Sub InitializeWeights()
                For i As Integer = 0 To weights.Length - 1
                    weights(i) = 1.0
                Next
            End Sub

            Private Function PredictCBOW(contextVectors As List(Of Double())) As Double()
                Dim contextSum As Double() = Enumerable.Repeat(0.0, embeddingSize).ToArray()

                For Each contextVector As Double() In contextVectors
                    For i As Integer = 0 To embeddingSize - 1
                        contextSum(i) += contextVector(i)
                    Next
                Next

                For i As Integer = 0 To embeddingSize - 1
                    contextSum(i) /= contextVectors.Count
                Next

                Return contextSum
            End Function

            Private Function PredictSkipGram(contextVector As Double()) As Double()
                Return contextVector
            End Function

            Private Sub TrainCBOW(contextIndices As List(Of Integer), targetIndex As Integer)
                Dim contextVectors As New List(Of Double())

                For Each contextIndex As Integer In contextIndices
                    Dim vector(embeddingSize - 1) As Double
                    For i As Integer = 1 To embeddingSize - 1
                        vector(i) = embeddingMatrix(contextIndex, i)
                    Next
                    contextVectors.Add(vector)
                Next

                Dim targetVector As Double() = GetTargetVector(targetIndex)
                Dim predictedVector As Double() = PredictCBOW(contextVectors)

                UpdateTargetVector(predictedVector, targetVector, contextVectors)
            End Sub

            Private Sub TrainSkipGram(targetIndex As Integer, contextIndices As List(Of Integer))
                Dim targetVector As Double() = GetTargetVector(targetIndex)

                For Each contextIndex As Integer In contextIndices
                    Dim contextVector As Double() = GetContextVector(contextIndex)

                    Dim predictedVector As Double() = PredictSkipGram(contextVector)

                    UpdateContextVector(predictedVector, contextVector, targetVector)
                Next
            End Sub

            Private Sub UpdateContextVector(predictedVector As Double(), contextVector As Double(), targetVector As Double())
                Dim errorGradient As Double() = Enumerable.Repeat(0.0, embeddingSize).ToArray()

                For i As Integer = 0 To embeddingSize - 1
                    errorGradient(i) = predictedVector(i) - targetVector(i)
                Next

                If contextVector.Length = embeddingSize Then
                    For i As Integer = 0 To embeddingSize - 1
                        Dim contextIndex As Integer = CInt(contextVector(i))
                        If contextIndex >= 0 AndAlso contextIndex < embeddingMatrix.GetLength(0) Then
                            embeddingMatrix(contextIndex, i) -= errorGradient(i) * learningRate
                        End If
                    Next
                End If
            End Sub

            Private Sub UpdateTargetVector(contextVectors As List(Of Double()), targetVector As Double())
                Dim contextSum As Double() = Enumerable.Repeat(0.0, embeddingSize).ToArray()

                For Each contextVector As Double() In contextVectors
                    For i As Integer = 0 To embeddingSize - 1
                        contextSum(i) += contextVector(i)
                    Next
                Next

                For i As Integer = 0 To embeddingSize - 1
                    targetVector(i) += (contextSum(i) / contextVectors.Count) * learningRate
                Next
            End Sub

            Private Sub UpdateTargetVector(predictedVector As Double(), targetVector As Double(), contextVectors As List(Of Double()))
                Dim errorGradient As Double() = Enumerable.Repeat(0.0, embeddingSize).ToArray()

                For i As Integer = 0 To embeddingSize - 1
                    errorGradient(i) = predictedVector(i) - targetVector(i)
                Next

                For Each contextVector As Double() In contextVectors
                    If contextVector.Length <> embeddingSize Then
                        Continue For ' Skip invalid context vectors
                    End If

                    For i As Integer = 0 To embeddingSize - 1
                        Dim contextIndex As Integer = CInt(contextVector(i))
                        If contextIndex >= 0 AndAlso contextIndex < embeddingMatrix.GetLength(0) Then
                            embeddingMatrix(contextIndex, i) -= errorGradient(i) * learningRate
                        End If
                    Next
                Next
            End Sub

        End Class
        Public Module TextProcessingTasks

            <Runtime.CompilerServices.Extension()>
            Public Function PerformTasks(ByRef Txt As String, ByRef Tasks As List(Of TextPreProcessingTasks)) As String

                For Each tsk In Tasks
                    Select Case tsk

                        Case TextPreProcessingTasks.Space_Punctuation

                            Txt = SpacePunctuation(Txt).Replace("  ", " ")
                        Case TextPreProcessingTasks.To_Upper
                            Txt = Txt.ToUpper.Replace("  ", " ")
                        Case TextPreProcessingTasks.To_Lower
                            Txt = Txt.ToLower.Replace("  ", " ")
                        Case TextPreProcessingTasks.Lemmatize_Text
                        Case TextPreProcessingTasks.Tokenize_Characters
                            Txt = TokenizeChars(Txt)
                            Dim Words() As String = Txt.Split(",")
                            Txt &= vbNewLine & "Total Tokens in doc  -" & Words.Count - 1 & ":" & vbNewLine
                        Case TextPreProcessingTasks.Remove_Stop_Words
                            TextExtensions.RemoveStopWords(Txt)
                        Case TextPreProcessingTasks.Tokenize_Words
                            Txt = TokenizeWords(Txt)
                            Dim Words() As String = Txt.Split(",")
                            Txt &= vbNewLine & "Total Tokens in doc  -" & Words.Count - 1 & ":" & vbNewLine
                        Case TextPreProcessingTasks.Tokenize_Sentences
                            Txt = TokenizeSentences(Txt)
                            Dim Words() As String = Txt.Split(",")
                            Txt &= vbNewLine & "Total Tokens in doc  -" & Words.Count - 2 & ":" & vbNewLine
                        Case TextPreProcessingTasks.Remove_Symbols
                            Txt = RemoveSymbols(Txt).Replace("  ", " ")
                        Case TextPreProcessingTasks.Remove_Brackets
                            Txt = RemoveBrackets(Txt).Replace("  ", " ")
                        Case TextPreProcessingTasks.Remove_Maths_Symbols
                            Txt = RemoveMathsSymbols(Txt).Replace("  ", " ")
                        Case TextPreProcessingTasks.Remove_Punctuation
                            Txt = RemovePunctuation(Txt).Replace("  ", " ")
                        Case TextPreProcessingTasks.AlphaNumeric_Only
                            Txt = AlphaNumericOnly(Txt).Replace("  ", " ")
                    End Select
                Next

                Return Txt
            End Function

            Public Enum TextPreProcessingTasks
                Space_Punctuation
                To_Upper
                To_Lower
                Lemmatize_Text
                Tokenize_Characters
                Remove_Stop_Words
                Tokenize_Words
                Tokenize_Sentences
                Remove_Symbols
                Remove_Brackets
                Remove_Maths_Symbols
                Remove_Punctuation
                AlphaNumeric_Only
            End Enum

        End Module
        Public Module Modelling

            ''' <summary>
            ''' creates a list of words with thier positional encoding (cosine simularity)
            ''' </summary>
            ''' <param name="DocText">document</param>
            ''' <returns>tokens with positional encoding</returns>
            <Runtime.CompilerServices.Extension()>
            Public Function PositionalEncoder(ByRef DocText As String) As List(Of WordVector)
                Dim sequence As String = "The quick brown fox jumps over the lazy dog."
                Dim words As String() = DocText.Split(" ")

                ' Create a list to store the positional encoding for each word
                Dim encoding As New List(Of List(Of Double))

                ' Calculate the positional encoding for each word in the sequence
                For i As Integer = 0 To words.Length - 1
                    ' Create a list to store the encoding vector for this word
                    Dim encodingVector As New List(Of Double)

                    ' Calculate the encoding vector for each dimension (8 dimensions for positional encoding)
                    For j As Integer = 0 To 7
                        Dim exponent As Double = j / 2

                        ' Calculate the sine or cosine value based on whether j is even or odd
                        If j Mod 2 = 0 Then
                            encodingVector.Add(Math.Sin(i / (10000 ^ exponent)))
                        Else
                            encodingVector.Add(Math.Cos(i / (10000 ^ exponent)))
                        End If
                    Next

                    ' Add the encoding vector for this word to the list of encodings
                    encoding.Add(encodingVector)
                Next
                Dim WordVects As New List(Of WordVector)
                ' Print the positional encoding for each word in the sequence
                For i As Integer = 0 To words.Length - 1
                    Dim NVect As New WordVector
                    NVect.Token = words(i)
                    For Each item In encoding(i)
                        NVect.PositionalEncodingVector.Add(item)
                    Next

                    WordVects.Add(NVect)
                Next
                Return WordVects
            End Function

            <Runtime.CompilerServices.Extension()>
            Public Function Top_N_Words(ByRef corpus As String, ByRef Count As Integer)
                Dim words As String() = corpus.Split(" ")
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
                Dim sortedDict = (From entry In wordCount Order By entry.Value Descending Select entry).Take(Count)
                Dim LSt As New List(Of String)
                ' Print the top ten words and their frequency
                For Each item In sortedDict
                    LSt.Add(item.Key)

                Next
                Return LSt
            End Function

            ''' <summary>
            ''' calculates the probability of a word in a corpus of documents
            ''' </summary>
            ''' <param name="Token">to be found</param>
            ''' <param name="corpus">collection of documents</param>
            <Runtime.CompilerServices.Extension()>
            Public Sub ProbablityOfWordInCorpus(ByRef Token As String, ByRef corpus As List(Of String))
                Dim word As String = Token
                Dim count As Integer = 0
                Dim totalWords As Integer = 0

                ' Count the number of times the word appears in the corpus
                For Each sentence As String In corpus
                    Dim words() As String = sentence.Split(" ")
                    For Each w As String In words
                        If w.Equals(word) Then
                            count += 1
                        End If
                        totalWords += 1
                    Next
                Next

                ' Calculate the probability of the word in the corpus
                Dim probability As Double = count / totalWords

                Console.WriteLine("The probability of the word '" & word & "' in the corpus is " & probability)
            End Sub

            <Runtime.CompilerServices.Extension()>
            Public Function TokenizeChars(ByRef Txt As String) As String

                Dim NewTxt As String = ""
                For Each chr As Char In Txt

                    NewTxt &= chr.ToString & ","
                Next

                Return NewTxt
            End Function

            <Runtime.CompilerServices.Extension()>
            Public Function TokenizeSentences(ByRef txt As String) As String
                Dim NewTxt As String = ""
                Dim Words() As String = txt.Split(".")
                For Each item In Words
                    NewTxt &= item & ","
                Next
                Return NewTxt
            End Function

            <Runtime.CompilerServices.Extension()>
            Public Function TokenizeWords(ByRef txt As String) As String
                Dim NewTxt As String = ""
                Dim Words() As String = txt.Split(" ")
                For Each item In Words
                    NewTxt &= item & ","
                Next
                Return NewTxt
            End Function

            ''' <summary>
            ''' Creates a vocabulary from the string presented
            ''' a dictionary of words from the text. containing word frequencys and sequence embeddings
            ''' this can be used to create word embeddings for the string
            ''' </summary>
            ''' <param name="InputString"></param>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Public Function CreateVocabulary(ByVal InputString As String) As List(Of WordVector)
                Return WordVector.CreateVocabulary(InputString.ToLower)
            End Function

            ''' <summary>
            ''' Creates embeddings by generating an internal vocabulary from the text provided
            ''' </summary>
            ''' <param name="InputString">document</param>
            ''' <returns>list of word vectors containing embeddings</returns>
            Public Function CreateWordEmbeddings(ByVal InputString As String) As List(Of WordVector)
                Return InputString.CreateWordEmbeddings(InputString.CreateVocabulary)
            End Function

            ''' <summary>
            ''' Creates a list of word-embeddings for string using a provided vocabulary
            ''' </summary>
            ''' <param name="InputString">document</param>
            ''' <param name="Vocabulary">Pretrained vocabulary</param>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Public Function CreateWordEmbeddings(ByVal InputString As String, ByRef Vocabulary As List(Of WordVector)) As List(Of WordVector)
                Return WordVector.EncodeWordsToVectors(InputString, Vocabulary)
            End Function

            <Runtime.CompilerServices.Extension()>
            Public Function OneHotEncoding(ByRef EncodedList As List(Of WordVector), KeyWords As List(Of String)) As List(Of WordVector)
                Return WordVector.OneShotEncoding(EncodedList, KeyWords)
            End Function

            ''' <summary>
            ''' looks up sequence encoding in vocabulary - used to encode a Document
            ''' </summary>
            ''' <param name="EncodedWordlist"></param>
            ''' <param name="Token"></param>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Function LookUpSeqEncoding(ByRef EncodedWordlist As List(Of WordVector), ByRef Token As String) As Integer
                Return (WordVector.LookUpSeqEncoding(EncodedWordlist, Token))
            End Function

            ''' <summary>
            ''' used for decoding token by sequence encoding
            ''' </summary>
            ''' <param name="EncodedWordlist"></param>
            ''' <param name="EncodingValue"></param>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Public Function LookUpBySeqEncoding(ByRef EncodedWordlist As List(Of WordVector), ByRef EncodingValue As Integer) As String
                Return (WordVector.LookUpSeqEncoding(EncodedWordlist, EncodingValue))
            End Function

        End Module
        Public Module CommonDataTypes

            ''' <summary>
            ''' Generate a random number based on the upper and lower bounds of the array,
            ''' then use that to return the item.
            ''' </summary>
            ''' <typeparam name="t"></typeparam>
            ''' <param name="theArray"></param>
            ''' <returns></returns>
            <Runtime.CompilerServices.Extension()>
            Public Function FetchRandomItem(Of t)(ByRef theArray() As t) As t

                Dim randNumberGenerator As New Random
                Randomize()
                Dim index As Integer = randNumberGenerator.Next(theArray.GetLowerBound(0),
                                                    theArray.GetUpperBound(0) + 1)

                Return theArray(index)

            End Function

            Public ReadOnly AlphaBet() As String = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
                    "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
                    "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", " "}

            Public ReadOnly EncapuslationPunctuationEnd() As String = {"}", "]", ">", ")"}
            Public ReadOnly CodePunctuation() As String = {"\", "#", "@", "^"}
            Public ReadOnly EncapuslationPunctuationStart() As String = {"{", "[", "<", "("}

            Public ReadOnly GramaticalPunctuation() As String = {"?", "!", ":", ";", ",", "_", "&"}

            Public ReadOnly MathPunctuation() As String = {"+", "-", "=", "/", "*", "%", "PLUS", "ADD", "MINUS", "SUBTRACT", "DIVIDE", "DIFFERENCE", "TIMES", "MULTIPLY", "PERCENT", "EQUALS"}

            Public ReadOnly MoneyPunctuation() As String = {"£", "$"}

            Public ReadOnly Number() As String = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
    "30", "40", "50", "60", "70", "80", "90", "00", "000", "0000", "00000", "000000", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen",
    "nineteen", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety", "hundred", "thousand", "million", "Billion"}

            Public ReadOnly Numerical() As String = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"}

            Public ReadOnly Symbols() As String = {"£", "$", "^", "@", "#", "~", "\"}

            Public StopWords As New List(Of String)

            Public ReadOnly StopWordsArab() As String = {"،", "آض", "آمينَ", "آه",
                    "آهاً", "آي", "أ", "أب", "أجل", "أجمع", "أخ", "أخذ", "أصبح", "أضحى", "أقبل",
                    "أقل", "أكثر", "ألا", "أم", "أما", "أمامك", "أمامكَ", "أمسى", "أمّا", "أن", "أنا", "أنت",
                    "أنتم", "أنتما", "أنتن", "أنتِ", "أنشأ", "أنّى", "أو", "أوشك", "أولئك", "أولئكم", "أولاء",
                    "أولالك", "أوّهْ", "أي", "أيا", "أين", "أينما", "أيّ", "أَنَّ", "أََيُّ", "أُفٍّ", "إذ", "إذا", "إذاً",
                    "إذما", "إذن", "إلى", "إليكم", "إليكما", "إليكنّ", "إليكَ", "إلَيْكَ", "إلّا", "إمّا", "إن", "إنّما",
                    "إي", "إياك", "إياكم", "إياكما", "إياكن", "إيانا", "إياه", "إياها", "إياهم", "إياهما", "إياهن",
                    "إياي", "إيهٍ", "إِنَّ", "ا", "ابتدأ", "اثر", "اجل", "احد", "اخرى", "اخلولق", "اذا", "اربعة", "ارتدّ",
                    "استحال", "اطار", "اعادة", "اعلنت", "اف", "اكثر", "اكد", "الألاء", "الألى", "الا", "الاخيرة", "الان", "الاول",
                    "الاولى", "التى", "التي", "الثاني", "الثانية", "الذاتي", "الذى", "الذي", "الذين", "السابق", "الف", "اللائي",
                    "اللاتي", "اللتان", "اللتيا", "اللتين", "اللذان", "اللذين", "اللواتي", "الماضي", "المقبل", "الوقت", "الى",
                    "اليوم", "اما", "امام", "امس", "ان", "انبرى", "انقلب", "انه", "انها", "او", "اول", "اي", "ايار", "ايام",
                    "ايضا", "ب", "بات", "باسم", "بان", "بخٍ", "برس", "بسبب", "بسّ", "بشكل", "بضع", "بطآن", "بعد", "بعض", "بك",
                    "بكم", "بكما", "بكن", "بل", "بلى", "بما", "بماذا", "بمن", "بن", "بنا", "به", "بها", "بي", "بيد", "بين",
                    "بَسْ", "بَلْهَ", "بِئْسَ", "تانِ", "تانِك", "تبدّل", "تجاه", "تحوّل", "تلقاء", "تلك", "تلكم", "تلكما", "تم", "تينك",
                    "تَيْنِ", "تِه", "تِي", "ثلاثة", "ثم", "ثمّ", "ثمّة", "ثُمَّ", "جعل", "جلل", "جميع", "جير", "حار", "حاشا", "حاليا",
                    "حاي", "حتى", "حرى", "حسب", "حم", "حوالى", "حول", "حيث", "حيثما", "حين", "حيَّ", "حَبَّذَا", "حَتَّى", "حَذارِ", "خلا",
                    "خلال", "دون", "دونك", "ذا", "ذات", "ذاك", "ذانك", "ذانِ", "ذلك", "ذلكم", "ذلكما", "ذلكن", "ذو", "ذوا", "ذواتا", "ذواتي", "ذيت", "ذينك", "ذَيْنِ", "ذِه", "ذِي", "راح", "رجع", "رويدك", "ريث", "رُبَّ", "زيارة", "سبحان", "سرعان", "سنة", "سنوات", "سوف", "سوى", "سَاءَ", "سَاءَمَا", "شبه", "شخصا", "شرع", "شَتَّانَ", "صار", "صباح", "صفر", "صهٍ", "صهْ", "ضد", "ضمن", "طاق", "طالما", "طفق", "طَق", "ظلّ", "عاد", "عام", "عاما", "عامة", "عدا", "عدة", "عدد", "عدم", "عسى", "عشر", "عشرة", "علق", "على", "عليك", "عليه", "عليها", "علًّ", "عن", "عند", "عندما", "عوض", "عين", "عَدَسْ", "عَمَّا", "غدا", "غير", "ـ", "ف", "فان", "فلان", "فو", "فى", "في", "فيم", "فيما", "فيه", "فيها", "قال", "قام", "قبل", "قد", "قطّ", "قلما", "قوة", "كأنّما", "كأين", "كأيّ", "كأيّن", "كاد", "كان", "كانت", "كذا", "كذلك", "كرب", "كل", "كلا", "كلاهما", "كلتا", "كلم", "كليكما", "كليهما", "كلّما", "كلَّا", "كم", "كما", "كي", "كيت", "كيف", "كيفما", "كَأَنَّ", "كِخ", "لئن", "لا", "لات", "لاسيما", "لدن", "لدى", "لعمر", "لقاء", "لك", "لكم", "لكما", "لكن", "لكنَّما", "لكي", "لكيلا", "للامم", "لم", "لما", "لمّا", "لن", "لنا", "له", "لها", "لو", "لوكالة", "لولا", "لوما", "لي", "لَسْتَ", "لَسْتُ", "لَسْتُم", "لَسْتُمَا", "لَسْتُنَّ", "لَسْتِ", "لَسْنَ", "لَعَلَّ", "لَكِنَّ", "لَيْتَ", "لَيْسَ", "لَيْسَا", "لَيْسَتَا", "لَيْسَتْ", "لَيْسُوا", "لَِسْنَا", "ما", "ماانفك", "مابرح", "مادام", "ماذا", "مازال", "مافتئ", "مايو", "متى", "مثل", "مذ", "مساء", "مع", "معاذ", "مقابل", "مكانكم", "مكانكما", "مكانكنّ", "مكانَك", "مليار", "مليون", "مما", "ممن", "من", "منذ", "منها", "مه", "مهما", "مَنْ", "مِن", "نحن", "نحو", "نعم", "نفس", "نفسه", "نهاية", "نَخْ", "نِعِمّا", "نِعْمَ", "ها", "هاؤم", "هاكَ", "هاهنا", "هبّ", "هذا", "هذه", "هكذا", "هل", "هلمَّ", "هلّا", "هم", "هما", "هن", "هنا", "هناك", "هنالك", "هو", "هي", "هيا", "هيت", "هيّا", "هَؤلاء", "هَاتانِ", "هَاتَيْنِ", "هَاتِه", "هَاتِي", "هَجْ", "هَذا", "هَذانِ", "هَذَيْنِ", "هَذِه", "هَذِي", "هَيْهَاتَ", "و", "و6", "وا", "واحد", "واضاف", "واضافت", "واكد", "وان", "واهاً", "واوضح", "وراءَك", "وفي", "وقال", "وقالت", "وقد", "وقف", "وكان", "وكانت", "ولا", "ولم",
                    "ومن", "وهو", "وهي", "ويكأنّ", "وَيْ", "وُشْكَانََ", "يكون", "يمكن", "يوم", "ّأيّان"}

            Public ReadOnly StopWordsDutch() As String = {"aan", "achte", "achter", "af", "al", "alle", "alleen", "alles", "als", "ander", "anders", "beetje",
            "behalve", "beide", "beiden", "ben", "beneden", "bent", "bij", "bijna", "bijv", "blijkbaar", "blijken", "boven", "bv",
            "daar", "daardoor", "daarin", "daarna", "daarom", "daaruit", "dan", "dat", "de", "deden", "deed", "derde", "derhalve", "dertig",
            "deze", "dhr", "die", "dit", "doe", "doen", "doet", "door", "drie", "duizend", "echter", "een", "eens", "eerst", "eerste", "eigen",
            "eigenlijk", "elk", "elke", "en", "enige", "er", "erg", "ergens", "etc", "etcetera", "even", "geen", "genoeg", "geweest", "haar",
            "haarzelf", "had", "hadden", "heb", "hebben", "hebt", "hedden", "heeft", "heel", "hem", "hemzelf", "hen", "het", "hetzelfde",
            "hier", "hierin", "hierna", "hierom", "hij", "hijzelf", "hoe", "honderd", "hun", "ieder", "iedere", "iedereen", "iemand", "iets",
            "ik", "in", "inderdaad", "intussen", "is", "ja", "je", "jij", "jijzelf", "jou", "jouw", "jullie", "kan", "kon", "konden", "kun",
            "kunnen", "kunt", "laatst", "later", "lijken", "lijkt", "maak", "maakt", "maakte", "maakten", "maar", "mag", "maken", "me", "meer",
            "meest", "meestal", "men", "met", "mevr", "mij", "mijn", "minder", "miss", "misschien", "missen", "mits", "mocht", "mochten",
            "moest", "moesten", "moet", "moeten", "mogen", "mr", "mrs", "mw", "na", "naar", "nam", "namelijk", "nee", "neem", "negen",
            "nemen", "nergens", "niemand", "niet", "niets", "niks", "noch", "nochtans", "nog", "nooit", "nu", "nv", "of", "om", "omdat",
            "ondanks", "onder", "ondertussen", "ons", "onze", "onzeker", "ooit", "ook", "op", "over", "overal", "overige", "paar", "per",
            "recent", "redelijk", "samen", "sinds", "steeds", "te", "tegen", "tegenover", "thans", "tien", "tiende", "tijdens", "tja", "toch",
            "toe", "tot", "totdat", "tussen", "twee", "tweede", "u", "uit", "uw", "vaak", "van", "vanaf", "veel", "veertig", "verder",
            "verscheidene", "verschillende", "via", "vier", "vierde", "vijf", "vijfde", "vijftig", "volgend", "volgens", "voor", "voordat",
            "voorts", "waar", "waarom", "waarschijnlijk", "wanneer", "waren", "was", "wat", "we", "wederom", "weer", "weinig", "wel", "welk",
            "welke", "werd", "werden", "werder", "whatever", "wie", "wij", "wijzelf", "wil", "wilden", "willen", "word", "worden", "wordt", "zal",
            "ze", "zei", "zeker", "zelf", "zelfde", "zes", "zeven", "zich", "zij", "zijn", "zijzelf", "zo", "zoals", "zodat", "zou", "zouden",
            "zulk", "zullen"}

            Public ReadOnly StopWordsENG() As String = {"a", "as", "able", "about", "above", "according", "accordingly", "across", "actually", "after", "afterwards", "again", "against", "aint",
            "all", "allow", "allows", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "an", "and", "another", "any",
            "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "arent", "around",
            "as", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "b", "be", "became", "because", "become", "becomes", "becoming",
            "been", "before", "beforehand", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief",
            "but", "by", "c", "cmon", "cs", "came", "can", "cant", "cannot", "cant", "cause", "causes", "certain", "certainly", "changes", "clearly", "co", "com",
            "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldnt",
            "course", "currently", "d", "definitely", "described", "despite", "did", "didnt", "different", "do", "does", "doesnt", "doing", "dont", "done", "down",
            "downwards", "during", "e", "each", "edu", "eg", "eight", "either", "else", "elsewhere", "enough", "entirely", "especially", "et", "etc", "even", "ever",
            "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "f", "far", "few", "fifth", "first", "five", "followed",
            "following", "follows", "for", "former", "formerly", "forth", "four", "from", "further", "furthermore", "g", "get", "gets", "getting", "given", "gives",
            "go", "goes", "going", "gone", "got", "gotten", "greetings", "h", "had", "hadnt", "happens", "hardly", "has", "hasnt", "have", "havent", "having", "he",
            "hes", "hello", "help", "hence", "her", "here", "heres", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "hi", "him", "himself", "his",
            "hither", "hopefully", "how", "howbeit", "however", "i", "id", "ill", "im", "ive", "ie", "if", "ignored", "immediate", "in", "inasmuch", "inc", "indeed",
            "indicate", "indicated", "indicates", "inner", "insofar", "instead", "into", "inward", "is", "isnt", "it", "itd", "itll", "its", "its", "itself", "j",
            "just", "k", "keep", "keeps", "kept", "know", "known", "knows", "l", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "lets",
            "like", "liked", "likely", "little", "look", "looking", "looks", "ltd", "m", "mainly", "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might",
            "more", "moreover", "most", "mostly", "much", "must", "my", "myself", "n", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needs", "neither",
            "never", "nevertheless", "new", "next", "nine", "no", "nobody", "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "o",
            "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto", "or", "other", "others", "otherwise", "ought", "our",
            "ours", "ourselves", "out", "outside", "over", "overall", "own", "p", "particular", "particularly", "per", "perhaps", "placed", "please", "plus", "possible",
            "presumably", "probably", "provides", "q", "que", "quite", "qv", "r", "rather", "rd", "re", "really", "reasonably", "regarding", "regardless", "regards",
            "relatively", "respectively", "right", "s", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming",
            "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "she", "should", "shouldnt", "since", "six", "so",
            "some", "somebody", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying",
            "still", "sub", "such", "sup", "sure", "t", "ts", "take", "taken", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "thats", "thats", "the",
            "their", "theirs", "them", "themselves", "then", "thence", "there", "theres", "thereafter", "thereby", "therefore", "therein", "theres", "thereupon",
            "these", "they", "theyd", "theyll", "theyre", "theyve", "think", "third", "this", "thorough", "thoroughly", "those", "though", "three", "through",
            "throughout", "thru", "thus", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "twice", "two", "u", "un",
            "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us", "use", "used", "useful", "uses", "using", "usually", "uucp", "v",
            "value", "various", "very", "via", "viz", "vs", "w", "want", "wants", "was", "wasnt", "way", "we", "wed", "well", "were", "weve", "welcome", "well",
            "went", "were", "werent", "what", "whats", "whatever", "when", "whence", "whenever", "where", "wheres", "whereafter", "whereas", "whereby", "wherein",
            "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whos", "whoever", "whole", "whom", "whose", "why", "will", "willing", "wish",
            "with", "within", "without", "wont", "wonder", "would", "wouldnt", "x", "y", "yes", "yet", "you", "youd", "youll", "youre", "youve", "your", "yours",
            "yourself", "yourselves", "youll", "z", "zero"}

            Public ReadOnly StopWordsFrench() As String = {"a", "abord", "absolument", "afin", "ah", "ai", "aie", "ailleurs", "ainsi", "ait", "allaient", "allo", "allons",
            "allô", "alors", "anterieur", "anterieure", "anterieures", "apres", "après", "as", "assez", "attendu", "au", "aucun", "aucune",
            "aujourd", "aujourd'hui", "aupres", "auquel", "aura", "auraient", "aurait", "auront", "aussi", "autre", "autrefois", "autrement",
            "autres", "autrui", "aux", "auxquelles", "auxquels", "avaient", "avais", "avait", "avant", "avec", "avoir", "avons", "ayant", "b",
            "bah", "bas", "basee", "bat", "beau", "beaucoup", "bien", "bigre", "boum", "bravo", "brrr", "c", "car", "ce", "ceci", "cela", "celle",
            "celle-ci", "celle-là", "celles", "celles-ci", "celles-là", "celui", "celui-ci", "celui-là", "cent", "cependant", "certain",
            "certaine", "certaines", "certains", "certes", "ces", "cet", "cette", "ceux", "ceux-ci", "ceux-là", "chacun", "chacune", "chaque",
            "cher", "chers", "chez", "chiche", "chut", "chère", "chères", "ci", "cinq", "cinquantaine", "cinquante", "cinquantième", "cinquième",
            "clac", "clic", "combien", "comme", "comment", "comparable", "comparables", "compris", "concernant", "contre", "couic", "crac", "d",
            "da", "dans", "de", "debout", "dedans", "dehors", "deja", "delà", "depuis", "dernier", "derniere", "derriere", "derrière", "des",
            "desormais", "desquelles", "desquels", "dessous", "dessus", "deux", "deuxième", "deuxièmement", "devant", "devers", "devra",
            "different", "differentes", "differents", "différent", "différente", "différentes", "différents", "dire", "directe", "directement",
            "dit", "dite", "dits", "divers", "diverse", "diverses", "dix", "dix-huit", "dix-neuf", "dix-sept", "dixième", "doit", "doivent", "donc",
            "dont", "douze", "douzième", "dring", "du", "duquel", "durant", "dès", "désormais", "e", "effet", "egale", "egalement", "egales", "eh",
            "elle", "elle-même", "elles", "elles-mêmes", "en", "encore", "enfin", "entre", "envers", "environ", "es", "est", "et", "etant", "etc",
            "etre", "eu", "euh", "eux", "eux-mêmes", "exactement", "excepté", "extenso", "exterieur", "f", "fais", "faisaient", "faisant", "fait",
            "façon", "feront", "fi", "flac", "floc", "font", "g", "gens", "h", "ha", "hein", "hem", "hep", "hi", "ho", "holà", "hop", "hormis", "hors",
            "hou", "houp", "hue", "hui", "huit", "huitième", "hum", "hurrah", "hé", "hélas", "i", "il", "ils", "importe", "j", "je", "jusqu", "jusque",
            "juste", "k", "l", "la", "laisser", "laquelle", "las", "le", "lequel", "les", "lesquelles", "lesquels", "leur", "leurs", "longtemps",
            "lors", "lorsque", "lui", "lui-meme", "lui-même", "là", "lès", "m", "ma", "maint", "maintenant", "mais", "malgre", "malgré", "maximale",
            "me", "meme", "memes", "merci", "mes", "mien", "mienne", "miennes", "miens", "mille", "mince", "minimale", "moi", "moi-meme", "moi-même",
            "moindres", "moins", "mon", "moyennant", "multiple", "multiples", "même", "mêmes", "n", "na", "naturel", "naturelle", "naturelles", "ne",
            "neanmoins", "necessaire", "necessairement", "neuf", "neuvième", "ni", "nombreuses", "nombreux", "non", "nos", "notamment", "notre",
            "nous", "nous-mêmes", "nouveau", "nul", "néanmoins", "nôtre", "nôtres", "o", "oh", "ohé", "ollé", "olé", "on", "ont", "onze", "onzième",
            "ore", "ou", "ouf", "ouias", "oust", "ouste", "outre", "ouvert", "ouverte", "ouverts", "o|", "où", "p", "paf", "pan", "par", "parce",
            "parfois", "parle", "parlent", "parler", "parmi", "parseme", "partant", "particulier", "particulière", "particulièrement", "pas",
            "passé", "pendant", "pense", "permet", "personne", "peu", "peut", "peuvent", "peux", "pff", "pfft", "pfut", "pif", "pire", "plein",
            "plouf", "plus", "plusieurs", "plutôt", "possessif", "possessifs", "possible", "possibles", "pouah", "pour", "pourquoi", "pourrais",
            "pourrait", "pouvait", "prealable", "precisement", "premier", "première", "premièrement", "pres", "probable", "probante",
            "procedant", "proche", "près", "psitt", "pu", "puis", "puisque", "pur", "pure", "q", "qu", "quand", "quant", "quant-à-soi", "quanta",
            "quarante", "quatorze", "quatre", "quatre-vingt", "quatrième", "quatrièmement", "que", "quel", "quelconque", "quelle", "quelles",
            "quelqu'un", "quelque", "quelques", "quels", "qui", "quiconque", "quinze", "quoi", "quoique", "r", "rare", "rarement", "rares",
            "relative", "relativement", "remarquable", "rend", "rendre", "restant", "reste", "restent", "restrictif", "retour", "revoici",
            "revoilà", "rien", "s", "sa", "sacrebleu", "sait", "sans", "sapristi", "sauf", "se", "sein", "seize", "selon", "semblable", "semblaient",
            "semble", "semblent", "sent", "sept", "septième", "sera", "seraient", "serait", "seront", "ses", "seul", "seule", "seulement", "si",
            "sien", "sienne", "siennes", "siens", "sinon", "six", "sixième", "soi", "soi-même", "soit", "soixante", "son", "sont", "sous", "souvent",
            "specifique", "specifiques", "speculatif", "stop", "strictement", "subtiles", "suffisant", "suffisante", "suffit", "suis", "suit",
            "suivant", "suivante", "suivantes", "suivants", "suivre", "superpose", "sur", "surtout", "t", "ta", "tac", "tant", "tardive", "te",
            "tel", "telle", "tellement", "telles", "tels", "tenant", "tend", "tenir", "tente", "tes", "tic", "tien", "tienne", "tiennes", "tiens",
            "toc", "toi", "toi-même", "ton", "touchant", "toujours", "tous", "tout", "toute", "toutefois", "toutes", "treize", "trente", "tres",
            "trois", "troisième", "troisièmement", "trop", "très", "tsoin", "tsouin", "tu", "té", "u", "un", "une", "unes", "uniformement", "unique",
            "uniques", "uns", "v", "va", "vais", "vas", "vers", "via", "vif", "vifs", "vingt", "vivat", "vive", "vives", "vlan", "voici", "voilà",
            "vont", "vos", "votre", "vous", "vous-mêmes", "vu", "vé", "vôtre", "vôtres", "w", "x", "y", "z", "zut", "à", "â", "ça", "ès", "étaient",
            "étais", "était", "étant", "été", "être", "ô"}

            Public ReadOnly StopWordsItalian() As String = {"IE", "a", "abbastanza", "abbia", "abbiamo", "abbiano", "abbiate", "accidenti", "ad", "adesso", "affinche", "agl", "agli",
            "ahime", "ahimè", "ai", "al", "alcuna", "alcuni", "alcuno", "all", "alla", "alle", "allo", "allora", "altri", "altrimenti", "altro",
            "altrove", "altrui", "anche", "ancora", "anni", "anno", "ansa", "anticipo", "assai", "attesa", "attraverso", "avanti", "avemmo",
            "avendo", "avente", "aver", "avere", "averlo", "avesse", "avessero", "avessi", "avessimo", "aveste", "avesti", "avete", "aveva",
            "avevamo", "avevano", "avevate", "avevi", "avevo", "avrai", "avranno", "avrebbe", "avrebbero", "avrei", "avremmo", "avremo",
            "avreste", "avresti", "avrete", "avrà", "avrò", "avuta", "avute", "avuti", "avuto", "basta", "bene", "benissimo", "berlusconi",
            "brava", "bravo", "c", "casa", "caso", "cento", "certa", "certe", "certi", "certo", "che", "chi", "chicchessia", "chiunque", "ci",
            "ciascuna", "ciascuno", "cima", "cio", "cioe", "cioè", "circa", "citta", "città", "ciò", "co", "codesta", "codesti", "codesto",
            "cogli", "coi", "col", "colei", "coll", "coloro", "colui", "come", "cominci", "comunque", "con", "concernente", "conciliarsi",
            "conclusione", "consiglio", "contro", "cortesia", "cos", "cosa", "cosi", "così", "cui", "d", "da", "dagl", "dagli", "dai", "dal",
            "dall", "dalla", "dalle", "dallo", "dappertutto", "davanti", "degl", "degli", "dei", "del", "dell", "della", "delle", "dello",
            "dentro", "detto", "deve", "di", "dice", "dietro", "dire", "dirimpetto", "diventa", "diventare", "diventato", "dopo", "dov", "dove",
            "dovra", "dovrà", "dovunque", "due", "dunque", "durante", "e", "ebbe", "ebbero", "ebbi", "ecc", "ecco", "ed", "effettivamente", "egli",
            "ella", "entrambi", "eppure", "era", "erano", "eravamo", "eravate", "eri", "ero", "esempio", "esse", "essendo", "esser", "essere",
            "essi", "ex", "fa", "faccia", "facciamo", "facciano", "facciate", "faccio", "facemmo", "facendo", "facesse", "facessero", "facessi",
            "facessimo", "faceste", "facesti", "faceva", "facevamo", "facevano", "facevate", "facevi", "facevo", "fai", "fanno", "farai",
            "faranno", "fare", "farebbe", "farebbero", "farei", "faremmo", "faremo", "fareste", "faresti", "farete", "farà", "farò", "fatto",
            "favore", "fece", "fecero", "feci", "fin", "finalmente", "finche", "fine", "fino", "forse", "forza", "fosse", "fossero", "fossi",
            "fossimo", "foste", "fosti", "fra", "frattempo", "fu", "fui", "fummo", "fuori", "furono", "futuro", "generale", "gia", "giacche",
            "giorni", "giorno", "già", "gli", "gliela", "gliele", "glieli", "glielo", "gliene", "governo", "grande", "grazie", "gruppo", "ha",
            "haha", "hai", "hanno", "ho", "i", "ieri", "il", "improvviso", "in", "inc", "infatti", "inoltre", "insieme", "intanto", "intorno",
            "invece", "io", "l", "la", "lasciato", "lato", "lavoro", "le", "lei", "li", "lo", "lontano", "loro", "lui", "lungo", "luogo", "là",
            "ma", "macche", "magari", "maggior", "mai", "male", "malgrado", "malissimo", "mancanza", "marche", "me", "medesimo", "mediante",
            "meglio", "meno", "mentre", "mesi", "mezzo", "mi", "mia", "mie", "miei", "mila", "miliardi", "milioni", "minimi", "ministro",
            "mio", "modo", "molti", "moltissimo", "molto", "momento", "mondo", "mosto", "nazionale", "ne", "negl", "negli", "nei", "nel",
            "nell", "nella", "nelle", "nello", "nemmeno", "neppure", "nessun", "nessuna", "nessuno", "niente", "no", "noi", "non", "nondimeno",
            "nonostante", "nonsia", "nostra", "nostre", "nostri", "nostro", "novanta", "nove", "nulla", "nuovo", "o", "od", "oggi", "ogni",
            "ognuna", "ognuno", "oltre", "oppure", "ora", "ore", "osi", "ossia", "ottanta", "otto", "paese", "parecchi", "parecchie",
            "parecchio", "parte", "partendo", "peccato", "peggio", "per", "perche", "perchè", "perché", "percio", "perciò", "perfino", "pero",
            "persino", "persone", "però", "piedi", "pieno", "piglia", "piu", "piuttosto", "più", "po", "pochissimo", "poco", "poi", "poiche",
            "possa", "possedere", "posteriore", "posto", "potrebbe", "preferibilmente", "presa", "press", "prima", "primo", "principalmente",
            "probabilmente", "proprio", "puo", "pure", "purtroppo", "può", "qualche", "qualcosa", "qualcuna", "qualcuno", "quale", "quali",
            "qualunque", "quando", "quanta", "quante", "quanti", "quanto", "quantunque", "quasi", "quattro", "quel", "quella", "quelle",
            "quelli", "quello", "quest", "questa", "queste", "questi", "questo", "qui", "quindi", "realmente", "recente", "recentemente",
            "registrazione", "relativo", "riecco", "salvo", "sara", "sarai", "saranno", "sarebbe", "sarebbero", "sarei", "saremmo", "saremo",
            "sareste", "saresti", "sarete", "sarà", "sarò", "scola", "scopo", "scorso", "se", "secondo", "seguente", "seguito", "sei", "sembra",
            "sembrare", "sembrato", "sembri", "sempre", "senza", "sette", "si", "sia", "siamo", "siano", "siate", "siete", "sig", "solito",
            "solo", "soltanto", "sono", "sopra", "sotto", "spesso", "srl", "sta", "stai", "stando", "stanno", "starai", "staranno", "starebbe",
            "starebbero", "starei", "staremmo", "staremo", "stareste", "staresti", "starete", "starà", "starò", "stata", "state", "stati",
            "stato", "stava", "stavamo", "stavano", "stavate", "stavi", "stavo", "stemmo", "stessa", "stesse", "stessero", "stessi", "stessimo",
            "stesso", "steste", "stesti", "stette", "stettero", "stetti", "stia", "stiamo", "stiano", "stiate", "sto", "su", "sua", "subito",
            "successivamente", "successivo", "sue", "sugl", "sugli", "sui", "sul", "sull", "sulla", "sulle", "sullo", "suo", "suoi", "tale",
            "tali", "talvolta", "tanto", "te", "tempo", "ti", "titolo", "torino", "tra", "tranne", "tre", "trenta", "troppo", "trovato", "tu",
            "tua", "tue", "tuo", "tuoi", "tutta", "tuttavia", "tutte", "tutti", "tutto", "uguali", "ulteriore", "ultimo", "un", "una", "uno",
            "uomo", "va", "vale", "vari", "varia", "varie", "vario", "verso", "vi", "via", "vicino", "visto", "vita", "voi", "volta", "volte",
            "vostra", "vostre", "vostri", "vostro", "è"}

            Public ReadOnly StopWordsSpanish() As String = {"a", "actualmente", "acuerdo", "adelante", "ademas", "además", "adrede", "afirmó", "agregó", "ahi", "ahora",
            "ahí", "al", "algo", "alguna", "algunas", "alguno", "algunos", "algún", "alli", "allí", "alrededor", "ambos", "ampleamos",
            "antano", "antaño", "ante", "anterior", "antes", "apenas", "aproximadamente", "aquel", "aquella", "aquellas", "aquello",
            "aquellos", "aqui", "aquél", "aquélla", "aquéllas", "aquéllos", "aquí", "arriba", "arribaabajo", "aseguró", "asi", "así",
            "atras", "aun", "aunque", "ayer", "añadió", "aún", "b", "bajo", "bastante", "bien", "breve", "buen", "buena", "buenas", "bueno",
            "buenos", "c", "cada", "casi", "cerca", "cierta", "ciertas", "cierto", "ciertos", "cinco", "claro", "comentó", "como", "con",
            "conmigo", "conocer", "conseguimos", "conseguir", "considera", "consideró", "consigo", "consigue", "consiguen", "consigues",
            "contigo", "contra", "cosas", "creo", "cual", "cuales", "cualquier", "cuando", "cuanta", "cuantas", "cuanto", "cuantos", "cuatro",
            "cuenta", "cuál", "cuáles", "cuándo", "cuánta", "cuántas", "cuánto", "cuántos", "cómo", "d", "da", "dado", "dan", "dar", "de",
            "debajo", "debe", "deben", "debido", "decir", "dejó", "del", "delante", "demasiado", "demás", "dentro", "deprisa", "desde",
            "despacio", "despues", "después", "detras", "detrás", "dia", "dias", "dice", "dicen", "dicho", "dieron", "diferente", "diferentes",
            "dijeron", "dijo", "dio", "donde", "dos", "durante", "día", "días", "dónde", "e", "ejemplo", "el", "ella", "ellas", "ello", "ellos",
            "embargo", "empleais", "emplean", "emplear", "empleas", "empleo", "en", "encima", "encuentra", "enfrente", "enseguida", "entonces",
            "entre", "era", "eramos", "eran", "eras", "eres", "es", "esa", "esas", "ese", "eso", "esos", "esta", "estaba", "estaban", "estado",
            "estados", "estais", "estamos", "estan", "estar", "estará", "estas", "este", "esto", "estos", "estoy", "estuvo", "está", "están", "ex",
            "excepto", "existe", "existen", "explicó", "expresó", "f", "fin", "final", "fue", "fuera", "fueron", "fui", "fuimos", "g", "general",
            "gran", "grandes", "gueno", "h", "ha", "haber", "habia", "habla", "hablan", "habrá", "había", "habían", "hace", "haceis", "hacemos",
            "hacen", "hacer", "hacerlo", "haces", "hacia", "haciendo", "hago", "han", "hasta", "hay", "haya", "he", "hecho", "hemos", "hicieron",
            "hizo", "horas", "hoy", "hubo", "i", "igual", "incluso", "indicó", "informo", "informó", "intenta", "intentais", "intentamos", "intentan",
            "intentar", "intentas", "intento", "ir", "j", "junto", "k", "l", "la", "lado", "largo", "las", "le", "lejos", "les", "llegó", "lleva",
            "llevar", "lo", "los", "luego", "lugar", "m", "mal", "manera", "manifestó", "mas", "mayor", "me", "mediante", "medio", "mejor", "mencionó",
            "menos", "menudo", "mi", "mia", "mias", "mientras", "mio", "mios", "mis", "misma", "mismas", "mismo", "mismos", "modo", "momento", "mucha",
            "muchas", "mucho", "muchos", "muy", "más", "mí", "mía", "mías", "mío", "míos", "n", "nada", "nadie", "ni", "ninguna", "ningunas", "ninguno",
            "ningunos", "ningún", "no", "nos", "nosotras", "nosotros", "nuestra", "nuestras", "nuestro", "nuestros", "nueva", "nuevas", "nuevo",
            "nuevos", "nunca", "o", "ocho", "os", "otra", "otras", "otro", "otros", "p", "pais", "para", "parece", "parte", "partir", "pasada",
            "pasado", "paìs", "peor", "pero", "pesar", "poca", "pocas", "poco", "pocos", "podeis", "podemos", "poder", "podria", "podriais",
            "podriamos", "podrian", "podrias", "podrá", "podrán", "podría", "podrían", "poner", "por", "porque", "posible", "primer", "primera",
            "primero", "primeros", "principalmente", "pronto", "propia", "propias", "propio", "propios", "proximo", "próximo", "próximos", "pudo",
            "pueda", "puede", "pueden", "puedo", "pues", "q", "qeu", "que", "quedó", "queremos", "quien", "quienes", "quiere", "quiza", "quizas",
            "quizá", "quizás", "quién", "quiénes", "qué", "r", "raras", "realizado", "realizar", "realizó", "repente", "respecto", "s", "sabe",
            "sabeis", "sabemos", "saben", "saber", "sabes", "salvo", "se", "sea", "sean", "segun", "segunda", "segundo", "según", "seis", "ser",
            "sera", "será", "serán", "sería", "señaló", "si", "sido", "siempre", "siendo", "siete", "sigue", "siguiente", "sin", "sino", "sobre",
            "sois", "sola", "solamente", "solas", "solo", "solos", "somos", "son", "soy", "soyos", "su", "supuesto", "sus", "suya", "suyas", "suyo",
            "sé", "sí", "sólo", "t", "tal", "tambien", "también", "tampoco", "tan", "tanto", "tarde", "te", "temprano", "tendrá", "tendrán", "teneis",
            "tenemos", "tener", "tenga", "tengo", "tenido", "tenía", "tercera", "ti", "tiempo", "tiene", "tienen", "toda", "todas", "todavia",
            "todavía", "todo", "todos", "total", "trabaja", "trabajais", "trabajamos", "trabajan", "trabajar", "trabajas", "trabajo", "tras",
            "trata", "través", "tres", "tu", "tus", "tuvo", "tuya", "tuyas", "tuyo", "tuyos", "tú", "u", "ultimo", "un", "una", "unas", "uno", "unos",
            "usa", "usais", "usamos", "usan", "usar", "usas", "uso", "usted", "ustedes", "v", "va", "vais", "valor", "vamos", "van", "varias", "varios",
            "vaya", "veces", "ver", "verdad", "verdadera", "verdadero", "vez", "vosotras", "vosotros", "voy", "vuestra", "vuestras", "vuestro",
            "vuestros", "w", "x", "y", "ya", "yo", "z", "él", "ésa", "ésas", "ése", "ésos", "ésta", "éstas", "éste", "éstos", "última", "últimas",
            "último", "últimos"}

        End Module
        ''' <summary>
        ''' Used to Extract Random Batches of data 
        ''' Given a list of strings, 
        ''' The Tokens are  given a simple index, 
        ''' based on a Stored/Supplied vocabulary, or,
        ''' it will create a vocabulary based on the current document. 
        ''' This Vocabulary is a simple text List so its index is its ID_
        ''' Your personal vocabulary should maintain 
        ''' its ordering to Maintain decoding , 
        ''' This vocabulary file should also be used for encoding, 
        ''' Making it a universal Vocab list
        ''' </summary>
        Public Class DataExtraction
            ''' <summary>
            ''' The maximum sequence length for padding or truncating sequences.
            ''' </summary>
            Public Shared maxSequenceLength As Integer = 8

            ''' <summary>
            ''' The vocabulary used for encoding and decoding.
            ''' </summary>
            Public Shared TestVocab As New List(Of String)
            ''' <summary>
            ''' Initializes a new instance of the DataExtraction class with the specified maximum sequence length.
            ''' </summary>
            ''' <param name="MaxSeqLength">The maximum sequence length for padding or truncating sequences.</param>
            Public Sub New(ByRef MaxSeqLength As Integer)
                TestVocab = New List(Of String)
                maxSequenceLength = MaxSeqLength
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the DataExtraction class.
            ''' </summary>
            Public Sub New()
                TestVocab = New List(Of String)
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the DataExtraction class with the specified vocabulary.
            ''' </summary>
            ''' <param name="Vocab">The vocabulary used for encoding and decoding.</param>
            Public Sub New(ByRef Vocab As List(Of String))
                TestVocab = Vocab
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the DataExtraction class with the specified vocabulary and maximum sequence length.
            ''' </summary>
            ''' <param name="Vocab">The vocabulary used for encoding and decoding.</param>
            ''' <param name="MaxSeqLength">The maximum sequence length for padding or truncating sequences.</param>
            Public Sub New(ByRef Vocab As List(Of String), ByRef MaxSeqLength As Integer)
                TestVocab = Vocab
                maxSequenceLength = MaxSeqLength
            End Sub
            Public Shared Function ComputeLoss(predictions As List(Of List(Of Double)), targets As List(Of List(Of Integer))) As Double
                Dim loss As Double = 0

                For i As Integer = 0 To predictions.Count - 1
                    For j As Integer = 0 To predictions(i).Count - 1
                        loss += -Math.Log(predictions(i)(j)) * targets(i)(j)
                    Next
                Next

                Return loss
            End Function

            ''' <summary>
            ''' Creates batches of data for training.
            ''' </summary>
            ''' <param name="trainingData">The training data as a list of string sequences.</param>
            ''' <param name="batchSize">The size of each batch.</param>
            Public Sub CreateData(ByRef trainingData As List(Of List(Of String)), ByRef batchSize As Integer)
                For batchStart As Integer = 0 To trainingData.Count - 1 Step batchSize
                    Dim batchEnd As Integer = Math.Min(batchStart + batchSize - 1, trainingData.Count - 1)
                    Dim batchInputs As List(Of List(Of Integer)) = GetBatchInputs(trainingData, batchStart, batchEnd)
                    Dim batchTargets As List(Of List(Of Integer)) = GetBatchTargets(trainingData, batchStart, batchEnd)

                    ' Perform further operations on the batches
                Next

                ' Compute loss
                ' Dim loss As Double = ComputeLoss(predictions, batchTargets)
            End Sub

            ''' <summary>
            ''' Converts a batch of data from a list of string sequences to a list of integer sequences.
            ''' </summary>
            ''' <param name="data">The input data as a list of string sequences.</param>
            ''' <param name="startIndex">The starting index of the batch.</param>
            ''' <param name="endIndex">The ending index of the batch.</param>
            ''' <returns>A list of integer sequences representing the batch inputs.</returns>
            Public Function GetBatchInputs(data As List(Of List(Of String)),
                                       startIndex As Integer,
                                       endIndex As Integer) As List(Of List(Of Integer))
                Dim batchInputs As New List(Of List(Of Integer))

                For i As Integer = startIndex To endIndex
                    Dim sequence As List(Of String) = data(i)

                    ' Convert words to corresponding indices
                    Dim indices As List(Of Integer) = ConvertWordsToIndices(sequence)

                    ' Pad or truncate sequence to the maximum length
                    indices = PadOrTruncateSequence(indices, maxSequenceLength)

                    ' Add the sequence to the batch
                    batchInputs.Add(indices)
                Next

                Return batchInputs
            End Function
            ''' <summary>
            ''' Converts a batch of data from a list of string sequences to a list of integer sequences as targets.
            ''' </summary>
            ''' <param name="data">The input data as a list of string sequences.</param>
            ''' <param name="startIndex">The starting index of the batch.</param>
            ''' <param name="endIndex">The ending index of the batch.</param>
            ''' <returns>A list of integer sequences representing the batch targets.</returns>
            Public Function GetBatchTargets(data As List(Of List(Of String)), startIndex As Integer, endIndex As Integer) As List(Of List(Of Integer))
                Dim batchTargets As New List(Of List(Of Integer))

                For i As Integer = startIndex To endIndex
                    Dim sequence As List(Of String) = data(i)

                    ' Convert words to corresponding indices
                    Dim indices As List(Of Integer) = ConvertWordsToIndices(sequence)

                    ' Shift the sequence to get the target sequence
                    Dim targetIndices As List(Of Integer) = ShiftSequence(indices)

                    ' Pad or truncate sequence to the maximum length
                    targetIndices = PadOrTruncateSequence(targetIndices, maxSequenceLength)

                    ' Add the target sequence to the batch
                    batchTargets.Add(targetIndices)
                Next

                Return batchTargets
            End Function

            ''' <summary>
            ''' Pads or truncates a sequence to a specified length.
            ''' </summary>
            ''' <param name="sequence">The input sequence.</param>
            ''' <param name="length">The desired length.</param>
            ''' <returns>The padded or truncated sequence.</returns>
            Public Function PadOrTruncateSequence(sequence As List(Of Integer), length As Integer) As List(Of Integer)
                If sequence.Count < length Then
                    ' Pad the sequence with a special padding token
                    sequence.AddRange(Enumerable.Repeat(TestVocab.IndexOf("PAD"), length - sequence.Count))
                ElseIf sequence.Count > length Then
                    ' Truncate the sequence to the desired length
                    sequence = sequence.GetRange(0, length)
                End If

                Return sequence
            End Function

            ''' <summary>
            ''' Shifts a sequence to the right and adds a special token at the beginning.
            ''' </summary>
            ''' <param name="sequence">The input sequence.</param>
            ''' <returns>The shifted sequence.</returns>
            Public Function ShiftSequence(sequence As List(Of Integer)) As List(Of Integer)
                ' Shifts the sequence to the right and adds a special token at the beginning
                Dim shiftedSequence As New List(Of Integer) From {TestVocab.IndexOf("START")}

                For i As Integer = 0 To sequence.Count - 1
                    shiftedSequence.Add(sequence(i))
                Next

                Return shiftedSequence
            End Function

            ''' <summary>
            ''' Converts a list of words to a list of corresponding indices based on the vocabulary.
            ''' </summary>
            ''' <param name="words">The list of words to convert.</param>
            ''' <returns>A list of corresponding indices.</returns>
            Private Function ConvertWordsToIndices(words As List(Of String)) As List(Of Integer)
                Dim indices As New List(Of Integer)

                For Each word As String In words
                    If TestVocab.Contains(word) Then
                        indices.Add(TestVocab.IndexOf(word))
                    Else
                        indices.Add(TestVocab.IndexOf("UNK")) ' Unknown word
                    End If
                Next

                Return indices
            End Function
        End Class

    End Namespace

End Namespace

