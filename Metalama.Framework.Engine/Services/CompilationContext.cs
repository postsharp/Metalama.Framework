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

    // The service provider is intentionally private because the CompilationContext is not created with the latest version of
    // the service provider. Additionally, the CompilationContext is cached in the CompilationContextFactory and CompilationContextFactory may
    // return an old instance of CompilationContext, created with an old service provider, even when requested from a new service provider.
    private readonly ProjectServiceProvider _serviceProvider;

    internal CompilationContext( Compilation compilation, ServiceProvider<IProjectService> serviceProvider, CompilationContextFactory factory )
    {
        this.Compilation = compilation;
        this._serviceProvider = serviceProvider;
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
    internal AttributeDeserializer AttributeDeserializer => new( this._serviceProvider, new CurrentAppDomainTypeResolver( this ) );

    private ISymbolClassifier GetSymbolClassifierCore()
    {
        var hasMetalamaReference = this.Compilation.GetTypeByMetadataName( typeof(RunTimeOrCompileTimeAttribute).FullName.AssertNotNull() ) != null;

        return hasMetalamaReference
            ? new SymbolClassifier( this._serviceProvider, this.Compilation, this.AttributeDeserializer )
            : new SymbolClassifier( this._serviceProvider, null, this.AttributeDeserializer );
    }

    [Memo]
    public SerializableTypeIdProvider SerializableTypeIdProvider => new( this.Compilation );

    [Memo]
    internal SyntaxGenerationContextFactory SyntaxGenerationContextFactory => new( this );

    [Memo]
    public ISymbolClassificationService SymbolClassificationService => new SymbolClassificationService( this );

    [Memo]
    internal SystemTypeResolver SystemTypeResolver
        => this._serviceProvider.Global.GetRequiredService<ISystemTypeResolverFactory>().Create( this._serviceProvider, this );

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