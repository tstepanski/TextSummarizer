using System.Collections.Generic;

namespace TextSummarizerCSharp
{
    public sealed class SummarizedDocument
    {
        public SummarizedDocument(IReadOnlyList<string> meaningfulSentences, decimal reductionPercentage)
        {
            MeaningfulSentences = meaningfulSentences;
            ReductionPercentage = reductionPercentage;
        }

        public IReadOnlyList<string> MeaningfulSentences { get; }
        public decimal ReductionPercentage { get; }
    }
}