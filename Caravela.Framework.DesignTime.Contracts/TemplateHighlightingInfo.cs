using Microsoft.CodeAnalysis.Text;
using System;

namespace Caravela.Framework.DesignTime.Contracts
{
    public class TemplateHighlightingInfo
    {
        public TemplateHighlightingInfo( SourceText text, ITextSpanClassifier classifier )
        {
            this.Text = text;
            this.Classifier = classifier;
        }

         
        public SourceText Text { get; }
        public ITextSpanClassifier Classifier { get; }
    }
}
