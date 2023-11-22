// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Roslyn;


internal sealed class CachingSemanticModel : ISemanticModel
{
    private readonly SemanticModel _semanticModel;
    private ConcurrentDictionary<SyntaxNode, SymbolInfo>? _symbolInfos;
    private ConcurrentDictionary<SyntaxNode, ISymbol?> _declaredSymbols;
    private ConcurrentDictionary<SyntaxNode, TypeInfo>? _typeInfos;

    internal CachingSemanticModel( SemanticModel semanticModel )
    {
        this._semanticModel = semanticModel;
    }
    
    public SyntaxTree SyntaxTree => this._semanticModel.SyntaxTree;

    public Compilation Compilation => this._semanticModel.Compilation;

    public SymbolInfo GetSymbolInfo( SyntaxNode node, CancellationToken cancellationToken = default )
    {
        this._symbolInfos ??= new ConcurrentDictionary<SyntaxNode, SymbolInfo>();

        return this._symbolInfos.GetOrAdd( node, ( n, x ) => x.o._semanticModel.GetSymbolInfo( n, x.cancellationToken ), (o: this,cancellationToken) );
    }

    public INamedTypeSymbol? GetDeclaredSymbol( BaseTypeDeclarationSyntax node, CancellationToken cancellationToken = default ) => (INamedTypeSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IMethodSymbol? GetDeclaredSymbol( MethodDeclarationSyntax node , CancellationToken cancellationToken = default) => (IMethodSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IEventSymbol? GetDeclaredSymbol( EventDeclarationSyntax node, CancellationToken cancellationToken = default ) => (IEventSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IPropertySymbol? GetDeclaredSymbol( PropertyDeclarationSyntax node, CancellationToken cancellationToken = default ) => (IPropertySymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public ISymbol? GetDeclaredSymbol( SyntaxNode node, CancellationToken cancellationToken = default ) => this.GetDeclaredSymbolCore( node, cancellationToken );

    private ISymbol? GetDeclaredSymbolCore( SyntaxNode node, CancellationToken cancellationToken )
    {
        this._declaredSymbols ??= new ConcurrentDictionary<SyntaxNode, ISymbol>();

        return this._declaredSymbols.GetOrAdd( node, ( n, x ) => x.o._semanticModel.GetDeclaredSymbol( n, x.cancellationToken ), (o:this,cancellationToken) );
    }

    public NullableContext GetNullableContext( int nodeSpanStart )
    {
        return this._semanticModel.GetNullableContext( nodeSpanStart );
    }

    public ImmutableArray<Diagnostic> GetDiagnostics( TextSpan span, CancellationToken cancellationToken )
        => this._semanticModel.GetDiagnostics( span, cancellationToken );

    public ImmutableArray<ISymbol> LookupSymbols( int spanEnd ) => this._semanticModel.LookupSymbols( spanEnd );

    public ControlFlowAnalysis AnalyzeControlFlow( BlockSyntax statement ) => this._semanticModel.AnalyzeControlFlow( statement );

    public TypeInfo GetTypeInfo( SyntaxNode node, CancellationToken cancellationToken = default)
    {
        this._typeInfos = new();

        return this._typeInfos.GetOrAdd( node, ( n, x ) => x.obj._semanticModel.GetTypeInfo( n, x.cancellationToken ), (obj: this, cancellationToken) );
    }
}


public sealed class SemanticModelProvider
{
    private static readonly WeakCache<Compilation, SemanticModelProvider> _instances = new();
    private readonly Compilation _compilation;
    private readonly ConcurrentDictionary<SyntaxTree, Cached> _semanticModels = new();

    private SemanticModelProvider( Compilation compilation )
    {
        this._compilation = compilation;
    }

    internal static SemanticModelProvider GetInstance( Compilation compilation ) => _instances.GetOrAdd( compilation, c => new SemanticModelProvider( c ) );

    public static ISemanticModel GetSemanticModel( SemanticModel semanticModel )
    {
        var provider = GetInstance( semanticModel.Compilation );
        
        var node = provider._semanticModels.GetOrAdd( semanticModel.SyntaxTree, _ => new Cached() );

        if ( semanticModel.IgnoresAccessibility )
        {
            node.IgnoringAccessibility ??= new CachingSemanticModel( semanticModel );

            return node.IgnoringAccessibility;
        }
        else
        {
            node.Default ??= new CachingSemanticModel( semanticModel );

            return node.Default;
        }
    }
    
    public ISemanticModel GetSemanticModel( SyntaxTree syntaxTree, bool ignoreAccessibility = false )
    {
        var node = this._semanticModels.GetOrAdd( syntaxTree, _ => new Cached() );

        if ( ignoreAccessibility )
        {
            node.IgnoringAccessibility ??= new CachingSemanticModel( this._compilation.GetSemanticModel( syntaxTree, true ) );

            return node.IgnoringAccessibility;
        }
        else
        {
            node.Default ??= new CachingSemanticModel( this._compilation.GetSemanticModel( syntaxTree ) );

            return node.Default;
        }
    }

    private sealed class Cached
    {
        public ISemanticModel? Default { get; set; }

        public ISemanticModel? IgnoringAccessibility { get; set; }
    }
}