// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Services;

public sealed class CompilationContext
{
    private readonly CompilationContextFactory _compilationContextFactory;

    internal CompilationContext( Compilation compilation, ServiceProvider<IProjectService> serviceProvider, CompilationContextFactory factory )
    {
        this.Compilation = compilation;
        this.ServiceProvider = serviceProvider;
        this._compilationContextFactory = factory;
    }

    [Memo]
    internal CompileTimeTypeFactory CompileTimeTypeFactory => new( this.SerializableTypeIdProvider );

    [Memo]
    internal CompilationComparers Comparers => new( this.ReflectionMapper, this.Compilation );

    public Compilation Compilation { get; }

    [Memo]
    internal ReflectionMapper ReflectionMapper => new( this.Compilation );

    [Memo]
    internal ISymbolClassifier SymbolClassifier => this.GetSymbolClassifierCore();

    [Memo]
    private AttributeDeserializer AttributeDeserializer => new( this.ServiceProvider, new CurrentAppDomainTypeResolver( this ) );

    private ISymbolClassifier GetSymbolClassifierCore()
    {
        var hasMetalamaReference = this.Compilation.GetTypeByMetadataName( typeof(RunTimeOrCompileTimeAttribute).FullName.AssertNotNull() ) != null;

        return hasMetalamaReference
            ? new SymbolClassifier( this.ServiceProvider, this.Compilation, this.AttributeDeserializer )
            : new SymbolClassifier( this.ServiceProvider, null, this.AttributeDeserializer );
    }

    [Memo]
    internal SerializableTypeIdProvider SerializableTypeIdProvider => new( this.Compilation );

    public ProjectServiceProvider ServiceProvider { get; }

    [Memo]
    internal SyntaxGenerationContextFactory SyntaxGenerationContextFactory => new( this );

    [Memo]
    public ISymbolClassificationService SymbolClassificationService => new SymbolClassificationService( this );

    [Memo]
    internal SystemTypeResolver SystemTypeResolver => this.ServiceProvider.Global.GetRequiredService<ISystemTypeResolverFactory>().Create( this );

    [Memo]
    public SemanticModelProvider SemanticModelProvider => this.Compilation.GetSemanticModelProvider();

    public CompilationContext ForCompilation( Compilation compilation ) => this._compilationContextFactory.GetInstance( compilation );

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxNode node )
    {
        return SyntaxGenerationContext.Create( this, node );
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( SyntaxTree tree, int nodeSpanStart )
    {
        return SyntaxGenerationContext.Create( this, tree, nodeSpanStart );
    }

    public SyntaxGenerationContext GetSyntaxGenerationContext( bool isPartial = false )
    {
        return SyntaxGenerationContext.Create( this, isPartial );
    }
}