// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Formatting
{
    internal partial class EndOfLineHelper
    {
        private sealed class TriviaWalker : SafeSyntaxWalker
        {
            private readonly Stack<NodeKind> _nodeAnnotationStack = new();

            private int _crLfNumber;
            private int _crNumber;
            private int _lfNumber;

            public TriviaWalker() : base( SyntaxWalkerDepth.StructuredTrivia ) { }

            public EndOfLineStyle EndOfLineStyle
            {
                get
                {
                    if ( this._crLfNumber > this._crNumber && this._crLfNumber > this._lfNumber )
                    {
                        return EndOfLineStyle.Windows;
                    }
                    else if ( this._lfNumber > this._crLfNumber && this._lfNumber > this._crNumber )
                    {
                        return EndOfLineStyle.Unix;
                    }
                    else if ( this._crNumber > this._crLfNumber && this._crNumber > this._lfNumber )
                    {
                        return EndOfLineStyle.MacOs;
                    }
                    else
                    {
                        return EndOfLineStyle.Unknown;
                    }
                }
            }

            protected override void VisitCore( SyntaxNode? node )
            {
                if ( node is { ContainsAnnotations: true } )
                {
                    // Add Source/Generated code annotations on the stack.
                    if ( node.HasAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                    {
                        try
                        {
                            this._nodeAnnotationStack.Push( NodeKind.GeneratedCode );

                            base.VisitCore( node );
                        }
                        finally
                        {
                            this._nodeAnnotationStack.Pop();
                        }
                    }
                    else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
                    {
                        try
                        {
                            this._nodeAnnotationStack.Push( NodeKind.SourceCode );

                            base.VisitCore( node );
                        }
                        finally
                        {
                            this._nodeAnnotationStack.Pop();
                        }
                    }
                    else
                    {
                        base.VisitCore( node );
                    }
                }
                else
                {
                    base.VisitCore( node );
                }
            }

            public bool IsMixed
                => this._crLfNumber != 0
                    ? this._lfNumber != 0 || this._crNumber != 0
                    : this._lfNumber != 0 && this._crNumber != 0;

            private bool IsInSourceCode()
            {
                // This assumes that every generated compilation unit has generated code annotation on itself.
                return this._nodeAnnotationStack.Count == 0 || this._nodeAnnotationStack.Peek() == NodeKind.SourceCode;
            }

            public override void VisitTrivia( SyntaxTrivia trivia )
            {
                // We are interested only in trivia in source code.
                if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) && this.IsInSourceCode() )
                {
                    var chars = trivia.ToString().AsSpan();

                    switch ( GetEndOfLineStyle( chars ) )
                    {
                        case EndOfLineStyle.Windows:
                            this._crLfNumber++;

                            break;

                        case EndOfLineStyle.Unix:
                            this._lfNumber++;

                            break;

                        case EndOfLineStyle.MacOs:
                            this._crNumber++;

                            break;
                    }
                }

                base.VisitTrivia( trivia );
            }
        }
    }
}