namespace HRMarket.Configuration.Moderation;

public interface IProfanityDetectionService
{
    Task<ProfanityCheckResult> CheckTextAsync(string text, string language = "ro");
    Task<ProfanityCheckResult> CheckMultipleTextsAsync(IEnumerable<string> texts, string language = "ro");
}

public class ProfanityCheckResult
{
    public bool ContainsProfanity { get; set; }
    public List<string> DetectedWords { get; set; } = [];
    public List<ProfanityMatch> Matches { get; set; } = [];
    public string SanitizedText { get; set; } = string.Empty;
}

public class ProfanityMatch
{
    public string Word { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Context { get; set; } = string.Empty;
}

public class ProfanityDetectionService : IProfanityDetectionService
{
    private readonly ILogger<ProfanityDetectionService> _logger;
    private readonly Dictionary<string, HashSet<string>> _profanityDictionaries;
    private readonly Dictionary<string, HashSet<string>> _leetSpeakMappings;

    public ProfanityDetectionService(ILogger<ProfanityDetectionService> logger)
    {
        _logger = logger;
        _profanityDictionaries = new Dictionary<string, HashSet<string>>();
        _leetSpeakMappings = new Dictionary<string, HashSet<string>>();
        InitializeDictionaries();
    }

    public async Task<ProfanityCheckResult> CheckTextAsync(string text, string language = "ro")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ProfanityCheckResult { SanitizedText = text };
        }

        return await Task.Run(() => PerformCheck(text, language));
    }

    public async Task<ProfanityCheckResult> CheckMultipleTextsAsync(
        IEnumerable<string> texts, 
        string language = "ro")
    {
        var combinedResult = new ProfanityCheckResult();
        var sanitizedTexts = new List<string>();

        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                sanitizedTexts.Add(text);
                continue;
            }

            var result = await CheckTextAsync(text, language);
            
            if (result.ContainsProfanity)
            {
                combinedResult.ContainsProfanity = true;
                combinedResult.DetectedWords.AddRange(result.DetectedWords);
                combinedResult.Matches.AddRange(result.Matches);
            }
            
            sanitizedTexts.Add(result.SanitizedText);
        }

        combinedResult.DetectedWords = combinedResult.DetectedWords.Distinct().ToList();
        combinedResult.SanitizedText = string.Join(" | ", sanitizedTexts);

        return combinedResult;
    }

    private ProfanityCheckResult PerformCheck(string text, string language)
    {
        var result = new ProfanityCheckResult
        {
            SanitizedText = text
        };

        // Normalize text for checking
        var normalizedText = NormalizeText(text);
        
        // Get appropriate dictionary
        if (!_profanityDictionaries.TryGetValue(language.ToLower(), out var dictionary))
        {
            _logger.LogWarning("No profanity dictionary found for language: {Language}. Using 'ro' as default.", language);
            dictionary = _profanityDictionaries["ro"];
        }

        // Check for profanity
        var words = normalizedText.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i].ToLower();
            var originalWord = GetOriginalWord(text, word, i);

            // Direct match
            if (dictionary.Contains(word))
            {
                AddMatch(result, originalWord, word, i, text);
                result.SanitizedText = ReplaceProfanity(result.SanitizedText, originalWord);
                continue;
            }

            // Check for leet speak variations
            var deLeetWord = DecodeLeetSpeak(word);
            if (deLeetWord != word && dictionary.Contains(deLeetWord))
            {
                AddMatch(result, originalWord, deLeetWord, i, text);
                result.SanitizedText = ReplaceProfanity(result.SanitizedText, originalWord);
                continue;
            }

            // Check for repeated characters (e.g., "shiiiit" -> "shit")
            var reducedWord = ReduceRepeatedCharacters(word);
            if (reducedWord != word && dictionary.Contains(reducedWord))
            {
                AddMatch(result, originalWord, reducedWord, i, text);
                result.SanitizedText = ReplaceProfanity(result.SanitizedText, originalWord);
                continue;
            }

            // Check for character substitutions (e.g., "@ss" -> "ass")
            var decodedWord = DecodeCharacterSubstitutions(word);
            if (decodedWord == word || !dictionary.Contains(decodedWord)) continue;
            AddMatch(result, originalWord, decodedWord, i, text);
            result.SanitizedText = ReplaceProfanity(result.SanitizedText, originalWord);
        }

        result.ContainsProfanity = result.DetectedWords.Count > 0;
        return result;
    }

    private static void AddMatch(ProfanityCheckResult result, string originalWord, string detectedWord, int position, string fullText)
    {
        if (!result.DetectedWords.Contains(detectedWord))
        {
            result.DetectedWords.Add(detectedWord);
        }

        var contextStart = Math.Max(0, fullText.IndexOf(originalWord, StringComparison.OrdinalIgnoreCase) - 20);
        var contextEnd = Math.Min(fullText.Length, contextStart + originalWord.Length + 40);
        var context = fullText[contextStart..contextEnd];

        result.Matches.Add(new ProfanityMatch
        {
            Word = detectedWord,
            Position = position,
            Context = context
        });
    }

    private static string GetOriginalWord(string text, string normalizedWord, int wordIndex)
    {
        var words = text.Split([' ', '.', ',', '!', '?', ';', ':', '\n', '\r', '\t'], 
            StringSplitOptions.RemoveEmptyEntries);
        return wordIndex < words.Length ? words[wordIndex] : normalizedWord;
    }

    private static string ReplaceProfanity(string text, string word)
    {
        var replacement = new string('*', word.Length);
        return System.Text.RegularExpressions.Regex.Replace(
            text, 
            System.Text.RegularExpressions.Regex.Escape(word), 
            replacement, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string NormalizeText(string text)
    {
        // Remove diacritics for Romanian
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => 
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != 
            System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
    }

    private string DecodeLeetSpeak(string word)
    {
        return _leetSpeakMappings.Aggregate(word, (current1, mapping) =>
            mapping.Value.Aggregate(current1, (current, variant) => 
                current.Replace(variant, mapping.Key)));
    }

    private static string ReduceRepeatedCharacters(string word)
    {
        if (word.Length <= 2) return word;
        
        var result = new System.Text.StringBuilder();
        var lastChar = '\0';
        var repeatCount = 0;

        foreach (var c in word)
        {
            if (c == lastChar)
            {
                repeatCount++;
                if (repeatCount < 2) // Allow one repetition
                {
                    result.Append(c);
                }
            }
            else
            {
                result.Append(c);
                lastChar = c;
                repeatCount = 0;
            }
        }

        return result.ToString();
    }

    private static string DecodeCharacterSubstitutions(string word)
    {
        return word
            .Replace("@", "a")
            .Replace("4", "a")
            .Replace("3", "e")
            .Replace("1", "i")
            .Replace("!", "i")
            .Replace("0", "o")
            .Replace("5", "s")
            .Replace("$", "s")
            .Replace("7", "t")
            .Replace("|", "l")
            .Replace("8", "b");
    }

    private void InitializeDictionaries()
    {
        // Romanian profanity dictionary
        _profanityDictionaries["ro"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Common Romanian profanity (sanitized examples - you should expand this)
            "prost", "proasta", "pula", "muie", "cacat", "rahat", "fut", "futut",
            "nenorocit", "nenorocita", "cacanar", "idiot", "idiota", "retardat",
            "imbecil", "tembel", "tampit", "tampita", "jegos", "jegoasa",
            "pizda", "curva", "tarfa", "scarba", "gunoi", "nesimtit",
            // Add more as needed
        };

        // English profanity dictionary
        _profanityDictionaries["en"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Common English profanity (sanitized examples - you should expand this)
            "fuck", "shit", "ass", "bitch", "damn", "bastard", "crap",
            "dick", "cock", "pussy", "whore", "slut", "asshole",
            "motherfucker", "goddamn", "piss", "retard", "fag",
            // Add more as needed
        };

        // Populate mappings without reassigning the readonly field
        _leetSpeakMappings["a"] = ["@", "4"];
        _leetSpeakMappings["e"] = ["3"];
        _leetSpeakMappings["i"] = ["1", "!"];
        _leetSpeakMappings["o"] = ["0"];
        _leetSpeakMappings["s"] = ["5", "$"];
        _leetSpeakMappings["t"] = ["7"];
        _leetSpeakMappings["l"] = ["|"];
        _leetSpeakMappings["b"] = ["8"];
    }
}