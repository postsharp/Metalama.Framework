// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.SyntaxGeneration;

public record SyntaxGenerationOptions : IProjectService
{
    internal bool NormalizeWhitespace { get; }

    internal bool PreserveTrivia { get; }

    internal bool AddFormattingAnnotations { get; }

    internal SyntaxGenerationOptions( bool normalizeWhitespace, bool preserveTrivia, bool addFormattingAnnotations )
    {
        this.NormalizeWhitespace = normalizeWhitespace;
        this.PreserveTrivia = preserveTrivia;
        this.AddFormattingAnnotations = addFormattingAnnotations;
    }

    /// <summary>
    /// Gets options that the creation of fully formatted code.
    /// </summary>
    internal static SyntaxGenerationOptions Proof { get; } = new( true, true, true );
}