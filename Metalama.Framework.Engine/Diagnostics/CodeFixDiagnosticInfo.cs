// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics
{
    public readonly struct CodeFixDiagnosticInfo
    {
        public const string TitlesPropertyKey = "Metalama.CodeFixes";
        public const string SourceAssemblyNamePropertyKey = "Metalama.SourceAssembly";
        public const string RedistributionLicenseKeyPropertyKey = "Metalama.RedistributionLicenseKey";
        public const char Separator = '\n';

        internal string? Titles { get; }

        internal string? SourceAssemblyName { get; }

        internal string? SourceRedistributionLicenseKey { get; }

        internal CodeFixDiagnosticInfo( string? titles, string? sourceAssemblyName, string? sourceRedistributionLicenseKey )
        {
            this.Titles = titles;
            this.SourceAssemblyName = sourceAssemblyName;
            this.SourceRedistributionLicenseKey = sourceRedistributionLicenseKey;
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