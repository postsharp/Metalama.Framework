﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Formatting
{
    internal static partial class EndOfLineHelper
    {
        private sealed class TriviaRewriter : SafeSyntaxRewriter, IDisposable
        {
            private readonly ReusableTextWriter _sourceWriter = new();
            private readonly ReusableTextWriter _destWriter = new();
            private readonly EndOfLineStyle _targetEndOfLineStyle;
            private readonly Stack<NodeKind> _nodeAnnotationStack = new();

            public TriviaRewriter( EndOfLineStyle targetEndOfLineStyle ) : base( true )
            {
                this._targetEndOfLineStyle = targetEndOfLineStyle;
            }

            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                if ( node is not { ContainsAnnotations: true } )
                {
                    // If there are no annotations, there is no generated code, i.e. we don't have to recurse.
                    return node;
                }
                else
                {
                    // Add Source/Generated code annotations on the stack.
                    if ( node.HasAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                    {
                        try
                        {
                            this._nodeAnnotationStack.Push( NodeKind.GeneratedCode );

                            return base.VisitCore( node );
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

                            return base.VisitCore( node );
                        }
                        finally
                        {
                            this._nodeAnnotationStack.Pop();
                        }
                    }
                    else
                    {
                        return base.VisitCore( node );
                    }
                }
            }

            private bool IsInGeneratedCode()
            {
                // This assumes that every generated compilation unit has generated code annotation on itself.
                return this._nodeAnnotationStack.Count > 0 && this._nodeAnnotationStack.Peek() == NodeKind.GeneratedCode;
            }

            public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
            {
                // Don't rewrite trivia anywhere else than in generated code.
                if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) && this.IsInGeneratedCode() )
                {
                    // Get the trivia string (is there a better way to read trivia content?)
                    trivia.WriteTo( this._sourceWriter );

                    var chars = this._sourceWriter.Data;

                    var endOfLineStyle = GetEndOfLineStyle( chars );

                    // Check whether the target style is not unknown and not equal to style on this trivia.
                    if ( this._targetEndOfLineStyle != endOfLineStyle && this._targetEndOfLineStyle != EndOfLineStyle.Unknown )
                    {
                        try
                        {
                            switch (endOfLineStyle, this._targetEndOfLineStyle)
                            {
                                // CRLF -> CR or CRLF -> LF
                                case (EndOfLineStyle.Windows, EndOfLineStyle.CR or EndOfLineStyle.LF):
                                    this._destWriter.Write( chars, 0, chars.Length - 2 );

                                    this._destWriter.Write(
                                        this._targetEndOfLineStyle switch
                                        {
                                            EndOfLineStyle.CR => '\r',
                                            EndOfLineStyle.LF => '\n',
                                            _ => throw new AssertionFailedException( $"Unexpected EOL style: {this._targetEndOfLineStyle}" )
                                        } );

                                    break;

                                // LF -> CRLF or CR -> CRLF
                                case (EndOfLineStyle.LF or EndOfLineStyle.CR, EndOfLineStyle.Windows):
                                    this._destWriter.Write( chars, 0, chars.Length - 1 );
                                    this._destWriter.Write( '\r' );
                                    this._destWriter.Write( '\n' );

                                    break;

                                // LF -> CR or CR -> LF
                                case (EndOfLineStyle.LF or EndOfLineStyle.CR, EndOfLineStyle.CR or EndOfLineStyle.LF):
                                    this._destWriter.Write( chars, 0, chars.Length - 1 );

                                    this._destWriter.Write(
                                        this._targetEndOfLineStyle switch
                                        {
                                            EndOfLineStyle.CR => '\r',
                                            EndOfLineStyle.LF => '\n',
                                            _ => throw new AssertionFailedException( $"Unexpected EOL style: {this._targetEndOfLineStyle}" )
                                        } );

                                    break;

                                default:
                                    throw new AssertionFailedException( $"Unexpected combination: ({endOfLineStyle}, {this._targetEndOfLineStyle})." );
                            }

                            return EndOfLine( this._destWriter.Data.ToString() );
                        }
                        finally
                        {
                            this._destWriter.Reset();
                            this._sourceWriter.Reset();
                        }
                    }
                    else
                    {
                        this._sourceWriter.Reset();

                        return trivia;
                    }
                }
                else
                {
                    return trivia;
                }
            }

            public void Dispose()
            {
                this._sourceWriter.Dispose();
                this._destWriter.Dispose();
            }
        }
    }
}