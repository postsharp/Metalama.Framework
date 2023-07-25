// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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