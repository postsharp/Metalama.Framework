// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Formatting
{
    internal partial class EndOfLineHelper
    {
        public static CompilationUnitSyntax NormalizeEndOfLineStyle( CompilationUnitSyntax compilationUnit )
        {
            DetermineEndOfLineStyle( compilationUnit, out var dominantStyle, out var isMixed );

            if ( !isMixed )
            {
                return compilationUnit;
            }
            else
            {
                var rewriter = new TriviaRewriter( dominantStyle );

                return (CompilationUnitSyntax) rewriter.Visit( compilationUnit ).AssertNotNull();
            }
        }

        /// <summary>
        /// Returns the first EOL string without allocating memory.
        /// </summary>
        public static string DetermineEndOfLineStyleFast( SyntaxTree syntaxTree )
            => DetermineEndOfLineStyleFast( syntaxTree.GetRoot() )
               ?? "\r\n";

        private static string? DetermineEndOfLineStyleFast( SyntaxTriviaList list )
        {
            foreach ( var trivia in list )
            {
                if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) )
                {
                    // The whole point of this game is to avoid memory allocation.
                    if ( trivia.SyntaxTree?.TryGetText( out var text ) != true || text == null )
                    {
                        return trivia.ToString();
                    }
                    else
                    {
                        switch ( trivia.Span.Length )
                        {
                            case 1:
                                switch ( text[trivia.SpanStart] )
                                {
                                    case '\r':
                                        return "\r";

                                    case '\n':
                                        return "\n";
                                }

                                break;

                            case 2:
                                switch ( (text[trivia.SpanStart], text[trivia.SpanStart + 1]) )
                                {
                                    case ('\r', '\n'):
                                        return "\r\n";
                                }

                                break;
                        }

                        return trivia.ToString();
                    }
                }
            }

            return null;
        }

        private static string? DetermineEndOfLineStyleFast( SyntaxNode node )
        {
            foreach ( var child in node.ChildNodesAndTokens() )
            {
                if ( child.IsNode )
                {
                    var eol = DetermineEndOfLineStyleFast( child.AsNode().AssertNotNull() );

                    if ( eol != null )
                    {
                        return eol;
                    }
                }
                else
                {
                    var token = child.AsToken();
                    var eol = DetermineEndOfLineStyleFast( token.LeadingTrivia ) ?? DetermineEndOfLineStyleFast( token.TrailingTrivia );

                    if ( eol != null )
                    {
                        return eol;
                    }
                }
            }

            return null;
        }

        private static void DetermineEndOfLineStyle( CompilationUnitSyntax compilationUnit, out EndOfLineStyle endOfLineStyle, out bool isMixed )
        {
            var visitor = new TriviaWalker();

            visitor.Visit( compilationUnit );

            isMixed = visitor.IsMixed;
            endOfLineStyle = visitor.EndOfLineStyle;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static EndOfLineStyle GetEndOfLineStyle( in ReadOnlySpan<char> chars )
        {
            if ( chars.Length >= 1 )
            {
                if ( chars.EndsWith( "\r\n".AsSpan() ) )
                {
                    return EndOfLineStyle.Windows;
                }
                else
                {
                    return chars[^1] switch
                    {
                        '\n' => EndOfLineStyle.Unix,
                        '\r' => EndOfLineStyle.MacOs,
                        _ => EndOfLineStyle.Unknown
                    };
                }
            }
            else
            {
                return EndOfLineStyle.Unknown;
            }
        }
    }
}