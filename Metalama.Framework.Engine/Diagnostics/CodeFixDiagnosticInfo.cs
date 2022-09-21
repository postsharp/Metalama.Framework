// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics
{
    public readonly struct CodeFixDiagnosticInfo
    {
        public const string TitlesPropertyKey = "Metalama.CodeFixes";
        public const char Separator = '\n';

        internal string? Titles { get; }

        internal CodeFixDiagnosticInfo( string? titles )
        {
            this.Titles = titles;
        }

        public static IReadOnlyList<string> GetCodeFixTitles( Diagnostic diagnostic )
        {
            if ( diagnostic.Properties.TryGetValue( TitlesPropertyKey, out var values ) && values != null )
            {
                return values.Split( Separator );
            }
            else
            {
                return Array.Empty<string>();
            }
        }
    }
}