// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Formatting
{
    internal static partial class EndOfLineHelper
    {
        private sealed class TriviaRewriter : SafeSyntaxRewriter
        {
            private readonly EndOfLineStyle _targetEndOfLineStyle;
            private NodeKind _currentNodeKind = NodeKind.SourceCode;

            public TriviaRewriter( EndOfLineStyle targetEndOfLineStyle ) : base( true )
            {
                this._targetEndOfLineStyle = targetEndOfLineStyle;
            }

            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                var oldNodeKind = this._currentNodeKind;

                if ( node.HasAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                {
                    this._currentNodeKind = NodeKind.GeneratedCode;
                }
                else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
                {
                    this._currentNodeKind = NodeKind.SourceCode;
                }

                var result = base.VisitCore( node );

                this._currentNodeKind = oldNodeKind;

                return result;
            }

            private bool IsInGeneratedCode()
            {
                // This assumes that every generated compilation unit has generated code annotation on itself.
                return this._currentNodeKind == NodeKind.GeneratedCode;
            }

            public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
            {
                if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) )
                {
                    var chars = trivia.ToString().AsSpan();
                    var endOfLineStyle = GetEndOfLineStyle( chars );

                    if ( this._targetEndOfLineStyle != endOfLineStyle )
                    {
                        if ( this.IsInGeneratedCode() )
                        {
                            return this._targetEndOfLineStyle switch
                            {
                                EndOfLineStyle.CR => ElasticEndOfLine( "\r" ),
                                EndOfLineStyle.LF => ElasticEndOfLine( "\n" ),
                                EndOfLineStyle.CRLF => ElasticEndOfLine( "\r\n" ),
                                _ => throw new AssertionFailedException()
                            };
                        }
                        else
                        {
                            // This is most probably an end-of-line that we did not properly mark as generated
                            // and this is a good place to have a breakpoint.
                        }
                    }
                }

                return base.VisitTrivia( trivia );
            }
        }
    }
}