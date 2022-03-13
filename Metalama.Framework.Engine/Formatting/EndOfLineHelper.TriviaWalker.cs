// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Formatting
{
    internal partial class EndOfLineHelper
    {
        private class TriviaWalker : CSharpSyntaxWalker
        {
            private readonly ReusableTextWriter _writer = new();
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

            public override void Visit( SyntaxNode? node )
            {
                if ( node != null && node.ContainsAnnotations )
                {
                    // Add Source/Generated code annotations on the stack.
                    if ( node.HasAnnotation( FormattingAnnotations.GeneratedCode ) )
                    {
                        try
                        {
                            this._nodeAnnotationStack.Push( NodeKind.GeneratedCode );

                            base.Visit( node );
                        }
                        finally
                        {
                            this._nodeAnnotationStack.Pop();
                        }
                    }
                    else if ( node.HasAnnotation( FormattingAnnotations.SourceCode ) )
                    {
                        try
                        {
                            this._nodeAnnotationStack.Push( NodeKind.SourceCode );

                            base.Visit( node );
                        }
                        finally
                        {
                            this._nodeAnnotationStack.Pop();
                        }
                    }
                    else
                    {
                        base.Visit( node );
                    }
                }
                else
                {
                    base.Visit( node );
                }
            }

            public bool IsMixed
                => this._crLfNumber != 0
                    ? this._lfNumber != 0 || this._crNumber != 0
                    : this._lfNumber != 0 && this._crNumber != 0;

            private bool IsInSourceCode()
            {
                return this._nodeAnnotationStack.Count == 0 || this._nodeAnnotationStack.Peek() == NodeKind.SourceCode;
            }

            public override void VisitTrivia( SyntaxTrivia trivia )
            {
                // We are interested only in trivia in source code.
                if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) && this.IsInSourceCode() )
                {
                    trivia.WriteTo( this._writer );

                    var chars = this._writer.Data;

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

                    this._writer.Reset();
                }

                base.VisitTrivia( trivia );
            }
        }
    }
}