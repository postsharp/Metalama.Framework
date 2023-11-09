// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

// ReSharper disable WrongIndentSize

#pragma warning disable SA1115, SA1113, SA1001, SA1111

namespace Metalama.Framework.Engine.Utilities;

/// <summary>
/// Exposes the versions of the C# language supported by Metalama.
/// </summary>
[PublicAPI]
public static class SupportedCSharpVersions
{
    /// <summary>
    /// Gets the default C# version.
    /// </summary>
    public static LanguageVersion Default
#if ROSLYN_4_8_0_OR_GREATER
        => LanguageVersion.CSharp12;
#elif ROSLYN_4_4_0_OR_GREATER
        => LanguageVersion.CSharp11;
#else
        => LanguageVersion.CSharp10;
#endif

#pragma warning disable SA1114 // Parameter list should follow declaration
    /// <summary>
    /// Gets all supported language versions.
    /// </summary>
    public static ImmutableHashSet<LanguageVersion> All { get; } = ImmutableHashSet.Create(
#if ROSLYN_4_8_0_OR_GREATER
        LanguageVersion.CSharp12,
#endif
#if ROSLYN_4_4_0_OR_GREATER
        LanguageVersion.CSharp11,
#endif
        LanguageVersion.CSharp10
    );

    /// <summary>
    /// Gets the default parse options.
    /// </summary>
    public static CSharpParseOptions DefaultParseOptions { get; } = CSharpParseOptions.Default.WithLanguageVersion( Default );
}