// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Services;

public sealed class CompilationContext : ICompilationServices, ITemplateReflectionContext
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
    internal ResolvingCompileTimeTypeFactory CompileTimeTypeFactory => new( this.SerializableTypeIdResolver );

    [Memo]
    internal CompilationComparers Comparers => new( this.ReflectionMapper, this.Compilation );

    ISymbolClassifier ITemplateReflectionContext.SymbolClassifier => this.SymbolClassifier;

    public Compilation Compilation { get; }

    AttributeDeserializer ITemplateReflectionContext.AttributeDeserializer => this.AttributeDeserializer;

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
    internal ISymbolClassifier SymbolClassifier => this.GetSymbolClassifierCore();

    [Memo]
    internal AttributeDeserializer AttributeDeserializer => this._serviceProvider.GetRequiredService<AttributeDeserializer>();

    private ISymbolClassifier GetSymbolClassifierCore()
    {
        var hasMetalamaReference = this.Compilation.GetTypeByMetadataName( typeof(RunTimeOrCompileTimeAttribute).FullName.AssertNotNull() ) != null;

        return hasMetalamaReference
            ? new SymbolClassifier( this._serviceProvider, this.Compilation, this.AttributeDeserializer )
            : new SymbolClassifier( this._serviceProvider, null, this.AttributeDeserializer );
    }

    [Memo]
    public SerializableTypeIdResolver SerializableTypeIdResolver => new( this.Compilation );

    [Memo]
    internal SyntaxGenerationContextFactory SyntaxGenerationContextFactory => new( this );

    [Memo]
    public ISymbolClassificationService SymbolClassificationService => new SymbolClassificationService( this );

    [Memo]
    internal SystemTypeResolver SystemTypeResolver
        => this._serviceProvider.Global.GetRequiredService<ISystemTypeResolverFactory>().Create( this._serviceProvider, this );

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