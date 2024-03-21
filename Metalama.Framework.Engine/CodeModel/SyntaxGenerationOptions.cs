// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CodeModel;

public record SyntaxGenerationOptions : IProjectService
{
    public bool NormalizeWhitespace { get; }

    public bool PreserveTrivia { get; }

    internal SyntaxGenerationOptions( bool normalizeWhitespace, bool preserveTrivia )
    {
        this.NormalizeWhitespace = normalizeWhitespace;
        this.PreserveTrivia = preserveTrivia;
    }

    /// <summary>
    /// Gets options for fast creation of the syntax tree when the object model does not need to be rendered as text.
    /// </summary>
    public static SyntaxGenerationOptions Draft { get; } = new( false, false );

    /// <summary>
    /// Gets options that the creation of fully formatted code.
    /// </summary>
    public static SyntaxGenerationOptions Proof { get; } = new( true, true );
}