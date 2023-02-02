// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Services;

public sealed class CompilationContext : ICompilationServices, ITemplateReflectionContext
{
    private readonly CompilationContextFactory _compilationContextFactory;

    internal CompilationContext( Compilation compilation, CompilationContextFactory factory )
    {
        this.Compilation = compilation;
        this._compilationContextFactory = factory;
    }

    [Memo]
    internal ResolvingCompileTimeTypeFactory CompileTimeTypeFactory => new( this.SerializableTypeIdResolver );

    [Memo]
    internal CompilationComparers Comparers => new( this.ReflectionMapper, this.Compilation );

    public Compilation Compilation { get; }

    CompilationModel ITemplateReflectionContext.GetCompilationModel( ICompilation sourceCompilation )
    {
        // When the current CompilationContext is used for reflecting the template code
        // (because the template is defined in source code, so it does not have its own ITemplateReflectionContext),
        // we use the source compilation.

        return (CompilationModel) sourceCompilation;
    }

    IReflectionMapper ICompilationServices.ReflectionMapper => this.ReflectionMapper;

    [Memo]
    internal ReflectionMapper ReflectionMapper => new( this.Compilation );
    
    [Memo]
    public SerializableTypeIdResolver SerializableTypeIdResolver => new( this.Compilation );

    [Memo]
    internal SyntaxGenerationContextFactory SyntaxGenerationContextFactory => new( this );

    [Memo]
    public SemanticModelProvider SemanticModelProvider => this.Compilation.GetSemanticModelProvider();

    internal CompilationContext ForCompilation( Compilation compilation ) => this._compilationContextFactory.GetInstance( compilation );

    internal SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        return SyntaxGenerationContext.Create( this, node );
    }

    internal SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxTree tree, int nodeSpanStart )
    {
        return SyntaxGenerationContext.Create( this, tree, nodeSpanStart );
    }

    internal SyntaxGenerationContext GetSyntaxGenerationContext( bool isPartial = false )
    {
        return SyntaxGenerationContext.Create( this, isPartial );
    }
}