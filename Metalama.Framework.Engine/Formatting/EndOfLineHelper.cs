﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

                return (CompilationUnitSyntax)rewriter.Visit( compilationUnit ).AssertNotNull();
            }
        }

        public static void DetermineEndOfLineStyle( CompilationUnitSyntax compilationUnit, out EndOfLineStyle endOfLineStyle, out bool isMixed )
        {
            var visitor = new TriviaWalker();

            visitor.Visit( compilationUnit );

            isMixed = visitor.IsMixed;
            endOfLineStyle = visitor.EndOfLineStyle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EndOfLineStyle GetEndOfLineStyle(in Span<char> chars)
        {
            if ( chars.Length >= 1 )
            {
                if ( chars.Length >= 2 && chars[chars.Length - 2] == '\r' && chars[chars.Length - 1] == '\n' )
                {
                    return EndOfLineStyle.Windows;
                }
                else if ( chars[chars.Length - 1] == '\n' )
                {
                    return EndOfLineStyle.Unix;
                }
                else if ( chars[chars.Length - 1] == '\r' )
                {
                    return EndOfLineStyle.MacOs;
                }
                else
                {
                    return EndOfLineStyle.Unknown;
                }
            }
            else
            {
                return EndOfLineStyle.Unknown;
            }
        }
    }
}