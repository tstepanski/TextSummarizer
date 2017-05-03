using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextSummarizerCSharp.Tests
{
    [TestClass]
    public class TextSummarizerTests
    {
        [TestMethod]
        public void SummarizeDocument_GivenExample1_ReturnsSummarizedText()
        {
            const string exampleText =
                @"The purpose of this paper is to extend existing research on entrepreneurial team formation under 
a competence-based perspective by empirically testing the influence of the sectoral context on 
that dynamics. We use inductive, theory-building design to understand how different sectoral 
characteristics moderate the influence of entrepreneurial opportunity recognition on subsequent 
entrepreneurial team formation. A sample of 195 founders who teamed up in the nascent phase of 
Interned-based and Cleantech sectors is analysed. The results suggest a twofold moderating effect 
of the sectoral context. First, a technologically more challenging sector (i.e. Cleantech) demands 
technically more skilled entrepreneurs, but at the same time, it requires still fairly 
commercially experienced and economically competent individuals. Furthermore, the business context 
also appears to exert an important influence on team formation dynamics: data reveals that 
individuals are more prone to team up with co-founders possessing complementary know-how when they 
are starting a new business venture in Cleantech rather than in the Internet-based sector. 
Overall, these results stress how the business context cannot be ignored when analysing 
entrepreneurial team formation dynamics by offering interesting insights on the matter to 
prospective entrepreneurs and interested policymakers.";

            var textSummarizer = new TextSummarizer();
            var result = textSummarizer.SummarizeDocument(exampleText, .66m);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.MeaningfulSentences.Any());
        }

        //[TestMethod]
        public void SummarizeDocument_OriginOfSpecies_ReturnsSummarizedText()
        {
            var httpClient = new HttpClient();
            var book = httpClient.GetStringAsync(@"http://www.gutenberg.org/cache/epub/2009/pg2009.txt").Result;
            var textSummarizer = new TextSummarizer();
            var result = textSummarizer.SummarizeDocument(book, .999m);

            Assert.IsNotNull(result);

            var stringResult = result.MeaningfulSentences
                .Aggregate(new StringBuilder(), (stringBuilder, sentence) => stringBuilder.AppendLine(sentence))
                .ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(stringResult));
        }

        //[TestMethod]
        public void SummarizeDocument_WarAndPeace_ReturnsSummarizedText()
        {
            var httpClient = new HttpClient();
            var book = httpClient.GetStringAsync(@"http://www.gutenberg.org/files/2600/2600-0.txt").Result;
            var textSummarizer = new TextSummarizer();
            var result = textSummarizer.SummarizeDocument(book, .999m);

            Assert.IsNotNull(result);

            var stringResult = result.MeaningfulSentences
                .Aggregate(new StringBuilder(), (stringBuilder, sentence) => stringBuilder.AppendLine(sentence))
                .ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(stringResult));
        }

        [TestMethod]
        public void SummarizeDocument_PrideAndPrejudice_ReturnsSummarizedText()
        {
            const string chapter1 = @"Chapter 1";

            var httpClient = new HttpClient();
            var book = httpClient.GetStringAsync(@"http://www.gutenberg.org/files/1342/1342-0.txt").Result;

            book =
                book.Substring(book.IndexOf(chapter1, StringComparison.OrdinalIgnoreCase) + chapter1.Length);

            book = Regex.Replace(book, @"Chapter([\s0-9]+)", string.Empty);

            var textSummarizer = new TextSummarizer();
            var result = textSummarizer.SummarizeDocument(book, .999m);

            Assert.IsNotNull(result);

            var stringResult = result.MeaningfulSentences
                .Aggregate(new StringBuilder(), (stringBuilder, sentence) => stringBuilder.AppendLine(sentence))
                .ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(stringResult));
        }
    }
}