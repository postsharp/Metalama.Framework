﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// Exposes the versions of the C# language supported by Metalama.
    /// </summary>
    public static class SupportedCSharpVersions
    {
        /// <summary>
        /// Gets the default C# version.
        /// </summary>
        public static LanguageVersion Default => LanguageVersion.CSharp10;

        /// <summary>
        /// Gets all supported language versions.
        /// </summary>
        public static ImmutableHashSet<LanguageVersion> All { get; } = ImmutableHashSet.Create( LanguageVersion.CSharp10 );

        /// <summary>
        /// Gets the default parse options.
        /// </summary>
        public static CSharpParseOptions DefaultParseOptions { get; } = CSharpParseOptions.Default.WithLanguageVersion( Default );
    }
}