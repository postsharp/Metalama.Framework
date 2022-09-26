// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics
{
    public readonly struct CodeFixTitles
    {
        public const string DiagnosticPropertyKey = "Metalama.CodeFixes";
        public const char Separator = '\n';

        internal string? Value { get; }

        internal CodeFixTitles( string? value )
        {
            this.Value = value;
        }

        public static IReadOnlyList<string> GetCodeFixTitles( Diagnostic diagnostic )
        {
            if ( diagnostic.Properties.TryGetValue( DiagnosticPropertyKey, out var values ) && values != null )
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