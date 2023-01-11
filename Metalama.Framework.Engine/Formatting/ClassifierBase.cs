// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.Engine.Formatting
{
    internal abstract class ClassifierBase : SafeSyntaxWalker
    {
        public ClassifiedTextSpanCollection ClassifiedTextSpans { get; }

#if !DEBUG
#pragma warning disable IDE0052 // Remove unread private members

        // ReSharper disable once NotAccessedField.Local
#endif
        private readonly SourceText? _sourceText;

        public ClassifierBase( ClassifiedTextSpanCollection classifiedTextSpans, SourceText? sourceText = null )
            : base( SyntaxWalkerDepth.Trivia )
        {
            this.ClassifiedTextSpans = classifiedTextSpans;
            this._sourceText = sourceText;
        }

        private bool _isAfterEndOfLine;

        private void VisitTriviaList( SyntaxTriviaList triviaList )
        {
            foreach ( var trivia in triviaList )
            {
                switch ( trivia.Kind() )
                {
                    case SyntaxKind.EndOfLineTrivia:
                        this._isAfterEndOfLine = true;

                        break;

                    case SyntaxKind.WhitespaceTrivia when this._isAfterEndOfLine:
                    case SyntaxKind.MultiLineCommentTrivia:
                    case SyntaxKind.SingleLineCommentTrivia:
                        this.Mark( trivia.Span, TextSpanClassification.NeutralTrivia );

                        break;
                }
            }
        }

        public override void VisitLeadingTrivia( SyntaxToken token )
        {
            this.VisitTriviaList( token.LeadingTrivia );
        }

        public override void VisitTrailingTrivia( SyntaxToken token )
        {
            this._isAfterEndOfLine = false;

            this.VisitTriviaList( token.TrailingTrivia );
        }

        protected void Mark( TextSpan span, TextSpanClassification classification )
        {
            if ( span.IsEmpty )
            {
                return;
            }

#if DEBUG

            // ReSharper disable once UnusedVariable
            var text = this._sourceText?.GetSubText( span ).ToString();
#endif

            this.ClassifiedTextSpans.Add( span, classification );
        }
    }
}