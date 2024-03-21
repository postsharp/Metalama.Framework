// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class SyntaxGenerationContext
{
    private Compilation Compilation => this.CompilationContext.Compilation;

    internal SyntaxGeneratorWithContext SyntaxGenerator { get; }

    internal CompilationContext CompilationContext { get; }

    internal bool IsPartial { get; }

    internal ReflectionMapper ReflectionMapper => this.CompilationContext.ReflectionMapper;

    internal bool RequiresStructFieldInitialization => this.Compilation.GetLanguageVersion() < (LanguageVersion) 1100;

    internal bool NormalizeWhitespace => this.Options.NormalizeWhitespace;

    internal bool PreserveTrivia => this.Options.PreserveTrivia;

    [Memo]
    internal bool SupportsInitAccessors => this.Compilation.GetTypeByMetadataName( typeof(IsExternalInit).FullName! ) != null;

    public SyntaxGenerationOptions Options { get; }

    // Should only be called by CompilationContext
    internal SyntaxGenerationContext(
        CompilationContext compilationContext,
        OurSyntaxGenerator syntaxGenerator,
        bool isPartial,
        SyntaxGenerationOptions? syntaxGenerationOptions )
    {
        this.CompilationContext = compilationContext;
        this.IsPartial = isPartial;
        this.Options = syntaxGenerationOptions ?? SyntaxGenerationOptions.Proof;
        this.SyntaxGenerator = new SyntaxGeneratorWithContext( syntaxGenerator, this );
    }

    public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";

    // used for debug assert
    public bool Equals( SyntaxGenerationContext? other )
        => other != null &&
           this.CompilationContext == other.CompilationContext &&
           this.SyntaxGenerator.IsNullAware == other.SyntaxGenerator.IsNullAware &&
           this.IsPartial == other.IsPartial;
}