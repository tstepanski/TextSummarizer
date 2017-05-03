using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextSummarizerCSharp
{
    public sealed class TextSummarizer
    {
        private static readonly IReadOnlyList<string> WordsToIgnore = new[]
        {
            "i",
            "me",
            "my",
            "myself",
            "we",
            "us",
            "our",
            "ours",
            "ourselves",
            "you",
            "your",
            "yours",
            "yourself",
            "yourselves",
            "he",
            "him",
            "his",
            "himself",
            "she",
            "her",
            "hers",
            "herself",
            "it",
            "its",
            "itself",
            "they",
            "them",
            "their",
            "theirs",
            "themselves",
            "what",
            "which",
            "who",
            "whom",
            "this",
            "that",
            "these",
            "those",
            "am",
            "is",
            "are",
            "was",
            "were",
            "be",
            "been",
            "being",
            "have",
            "has",
            "had",
            "having",
            "do",
            "does",
            "did",
            "doing",
            "will",
            "would",
            "shall",
            "should",
            "can",
            "could",
            "may",
            "might",
            "must",
            "ought",
            "i'm",
            "you're",
            "he's",
            "she's",
            "it's",
            "we're",
            "they're",
            "i've",
            "you've",
            "we've",
            "they've",
            "i'd",
            "you'd",
            "he'd",
            "she'd",
            "we'd",
            "they'd",
            "i'll",
            "you'll",
            "he'll",
            "she'll",
            "we'll",
            "they'll",
            "isn't",
            "aren't",
            "wasn't",
            "weren't",
            "hasn't",
            "haven't",
            "hadn't",
            "doesn't",
            "don't",
            "didn't",
            "won't",
            "wouldn't",
            "shan't",
            "shouldn't",
            "can't",
            "cannot",
            "couldn't",
            "mustn't",
            "let's",
            "that's",
            "who's",
            "what's",
            "here's",
            "there's",
            "when's",
            "where's",
            "why's",
            "how's",
            "a",
            "an",
            "the",
            "and",
            "but",
            "if",
            "or",
            "because",
            "as",
            "until",
            "while",
            "of",
            "at",
            "by",
            "for",
            "with",
            "about",
            "against",
            "between",
            "into",
            "through",
            "during",
            "before",
            "after",
            "above",
            "below",
            "to",
            "from",
            "up",
            "down",
            "in",
            "out",
            "on",
            "off",
            "over",
            "under",
            "again",
            "further",
            "then",
            "once",
            "here",
            "there",
            "when",
            "where",
            "why",
            "how",
            "all",
            "any",
            "both",
            "each",
            "few",
            "more",
            "most",
            "other",
            "some",
            "such",
            "no",
            "nor",
            "not",
            "only",
            "own",
            "same",
            "so",
            "than",
            "too",
            "very",
            "one",
            "every",
            "least",
            "less",
            "many",
            "now",
            "ever",
            "never",
            "say",
            "says",
            "said",
            "also",
            "get",
            "go",
            "goes",
            "just",
            "made",
            "make",
            "put",
            "see",
            "seen",
            "whether",
            "like",
            "well",
            "back",
            "even",
            "still",
            "way",
            "take",
            "since",
            "another",
            "however",
            "two",
            "three",
            "four",
            "five",
            "first",
            "second",
            "new",
            "old",
            "high",
            "long"
        };

        public SummarizedDocument SummarizeDocument(string documentText, decimal reductionPercentage)
        {
            var abbreviationTokens =
                new Regex(
                    @"(Dr\.|Esq\.|Hon\.|Jr\.|Mr\.|Mrs\.|Ms\.|Messrs\.|Mmes\.|Msgr\.|Prof\.|Rev\.|Rt\. Hon\.|Sr\.|St\.|I\.E\.|E\.G\.|([a-zA-Z]\.(?=[a-zA-Z])))",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

            documentText = abbreviationTokens.Replace(documentText, match => match.Value.Replace(@".", string.Empty));

            var sentenceMatches = Regex.Matches(documentText, @"(([^.!?])+)([.!?]+)", RegexOptions.Compiled);
            var sentencesText = GetStringsFromMatches(sentenceMatches);

            var sentences = sentencesText
                .Select((sentenceText, index) =>
                    new Sentence((uint) index, sentenceText, GetMeaningfulWordFrequencies(sentenceText)))
                .ToArray();

            var documentMeaningfulWordFrequencies = GetDocumentMeaningfulWordFrequencies(sentences);

            var document = new Document(sentences, documentMeaningfulWordFrequencies, reductionPercentage);

            return SummarizeDocument(document);
        }

        private static SummarizedDocument SummarizeDocument(Document document)
        {
            ScoreSentences(document);

            var originalSentences = document.Sentences;
            var originalSentenceCount = originalSentences.Count;
            var sentences = originalSentences.ToDictionary(sentence => sentence.OriginalPosition);
            var sentencesByLackOfMeaningfulness = originalSentences.OrderBy(sentence => sentence.Score);

            Sentence lastSentenceRemoved = null;
            var lastReductionPercentage = 0.0m;

            var goalReductionPercentage = document.GoalReductionPercentage;
            var reductionPercentage = 0.0m;

            foreach (var sentence in sentencesByLackOfMeaningfulness)
            {
                sentences.Remove(sentence.OriginalPosition);

                reductionPercentage = 1 - (decimal) sentences.Count / originalSentenceCount;

                if (reductionPercentage < goalReductionPercentage)
                {
                    lastSentenceRemoved = sentence;
                    lastReductionPercentage = reductionPercentage;

                    continue;
                }

                sentences.Add(sentence.OriginalPosition, sentence);

                var currentProximityToGoal = Math.Abs(goalReductionPercentage - reductionPercentage);
                var previousProximityToGoal = Math.Abs(goalReductionPercentage - lastReductionPercentage);

                if (currentProximityToGoal > previousProximityToGoal && lastSentenceRemoved != null)
                {
                    sentences.Add(lastSentenceRemoved.OriginalPosition, lastSentenceRemoved);
                }

                break;
            }

            var sentencesRemainingOrderedByOriginalPosition = sentences
                .Values
                .OrderBy(sentence => sentence.OriginalPosition)
                .Select(sentence => sentence.Text)
                .ToArray();

            return new SummarizedDocument(sentencesRemainingOrderedByOriginalPosition, reductionPercentage);
        }

        private static void ScoreSentences(Document document)
        {
            var documentMeaningfulWordFrequencies = document.MeaningfulWordFrequencies;

            foreach (var sentence in document.Sentences)
            {
                var sentenceMeaningfulWordFrequencies = sentence.MeaningfulWordFrequencies;

                sentence.Score = ScoreSentence(sentenceMeaningfulWordFrequencies, documentMeaningfulWordFrequencies);
            }
        }

        private static long ScoreSentence(IReadOnlyDictionary<string, uint> sentenceMeaningfulWordFrequencies,
            IReadOnlyDictionary<string, uint> documentMeaningfulWordFrequencies)
        {
            Func<KeyValuePair<string, uint>, string> getWordFromPairFunction = wordAndCountPair => wordAndCountPair.Key;

            Func<KeyValuePair<string, uint>, KeyValuePair<string, uint>, uint> valuationFunction =
                (sentenceWordAndCountPair, documentWordAndCountPair) =>
                    sentenceWordAndCountPair.Value * documentWordAndCountPair.Value;

            return sentenceMeaningfulWordFrequencies
                .Join(documentMeaningfulWordFrequencies, getWordFromPairFunction, getWordFromPairFunction,
                    valuationFunction)
                .Sum(wordValuation => wordValuation);
        }

        private static IReadOnlyDictionary<string, uint> GetDocumentMeaningfulWordFrequencies(
            IEnumerable<Sentence> sentences)
        {
            var wordsAndCounts = sentences.SelectMany(sentence => sentence.MeaningfulWordFrequencies);

            var wordsAndCountsSummed = wordsAndCounts
                .GroupBy(wordAndCountPair => wordAndCountPair.Key, pair => pair.Value,
                    (word, counts) => new KeyValuePair<string, uint>(word, (uint) counts.Sum(count => count)));

            return wordsAndCountsSummed
                .ToDictionary(wordAndCountPair => wordAndCountPair.Key, wordAndCountPair => wordAndCountPair.Value);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static IEnumerable<string> GetStringsFromMatches(MatchCollection matchCollection)
            => matchCollection.Cast<Match>().Select(match => match.ToString().Trim());

        private static IReadOnlyDictionary<string, uint> GetMeaningfulWordFrequencies(string originalText)
        {
            var wordMatches = Regex.Matches(originalText, @"([0-9a-zA-Z\.\-'`]+)", RegexOptions.Compiled);
            var words = GetStringsFromMatches(wordMatches);

            var meaningfulWords = words
                .Where(word => !WordsToIgnore.Contains(word, OriginalIgnoreCaseEqualityComparer.Instance));

            var meaningfulWordsAndCounts = meaningfulWords
                .GroupBy(word => word, word => word,
                    (word, instances) => new KeyValuePair<string, uint>(word, (uint) instances.Count()));

            return meaningfulWordsAndCounts
                .ToDictionary(wordAndCountPair => wordAndCountPair.Key, wordAndCountPair => wordAndCountPair.Value);
        }

        private sealed class OriginalIgnoreCaseEqualityComparer : IEqualityComparer<string>
        {
            private OriginalIgnoreCaseEqualityComparer()
            {
            }

            public static OriginalIgnoreCaseEqualityComparer Instance { get; } =
                new OriginalIgnoreCaseEqualityComparer();

            public bool Equals(string x, string y) => x.Equals(y, StringComparison.OrdinalIgnoreCase);
            public int GetHashCode(string obj) => obj.ToUpper().GetHashCode();
        }

        private sealed class Document
        {
            public Document(IReadOnlyList<Sentence> sentences,
                IReadOnlyDictionary<string, uint> meaningfulWordFrequencies, decimal goalReductionPercentage)
            {
                Sentences = sentences;
                MeaningfulWordFrequencies = meaningfulWordFrequencies;
                GoalReductionPercentage = goalReductionPercentage;
            }

            public IReadOnlyList<Sentence> Sentences { get; }
            public IReadOnlyDictionary<string, uint> MeaningfulWordFrequencies { get; }
            public decimal GoalReductionPercentage { get; }
        }

        private sealed class Sentence
        {
            public Sentence(uint originalPosition, string text,
                IReadOnlyDictionary<string, uint> meaningfulWordFrequencies)
            {
                OriginalPosition = originalPosition;
                Text = text;
                MeaningfulWordFrequencies = meaningfulWordFrequencies;
            }

            public uint OriginalPosition { get; }
            public string Text { get; }
            public IReadOnlyDictionary<string, uint> MeaningfulWordFrequencies { get; }
            public long Score { get; set; }
        }
    }
}