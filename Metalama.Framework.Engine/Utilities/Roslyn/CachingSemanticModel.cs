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
    private ConcurrentDictionary<SyntaxNode, ISymbol?>? _declaredSymbols;
    private ConcurrentDictionary<SyntaxNode, TypeInfo>? _typeInfos;

    internal CachingSemanticModel( SemanticModel semanticModel )
    {
        this._semanticModel = semanticModel;
    }

    public static ISemanticModel GetInstance( SemanticModel semanticModel ) => SemanticModelProvider.GetSemanticModel( semanticModel );

    public SyntaxTree SyntaxTree => this._semanticModel.SyntaxTree;

    public Compilation Compilation => this._semanticModel.Compilation;

    public SymbolInfo GetSymbolInfo( SyntaxNode node, CancellationToken cancellationToken = default )
    {
        this._symbolInfos ??= new ConcurrentDictionary<SyntaxNode, SymbolInfo>();

        return this._symbolInfos.GetOrAdd( node, static ( n, x ) => x.o._semanticModel.GetSymbolInfo( n, x.cancellationToken ), (o: this, cancellationToken) );
    }

    public INamedTypeSymbol? GetDeclaredSymbol( BaseTypeDeclarationSyntax node, CancellationToken cancellationToken = default )
        => (INamedTypeSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IMethodSymbol? GetDeclaredSymbol( MethodDeclarationSyntax node, CancellationToken cancellationToken = default )
        => (IMethodSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IEventSymbol? GetDeclaredSymbol( EventDeclarationSyntax node, CancellationToken cancellationToken = default )
        => (IEventSymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public IPropertySymbol? GetDeclaredSymbol( PropertyDeclarationSyntax node, CancellationToken cancellationToken = default )
        => (IPropertySymbol?) this.GetDeclaredSymbolCore( node, cancellationToken );

    public ISymbol? GetDeclaredSymbol( SyntaxNode node, CancellationToken cancellationToken = default )
        => this.GetDeclaredSymbolCore( node, cancellationToken );

    private ISymbol? GetDeclaredSymbolCore( SyntaxNode node, CancellationToken cancellationToken )
    {
        this._declaredSymbols ??= new ConcurrentDictionary<SyntaxNode, ISymbol?>();

        return this._declaredSymbols.GetOrAdd( node, static ( n, x ) => x.o._semanticModel.GetDeclaredSymbol( n, x.cancellationToken ), (o: this, cancellationToken) );
    }

    public NullableContext GetNullableContext( int nodeSpanStart )
    {
        return this._semanticModel.GetNullableContext( nodeSpanStart );
    }

    public ImmutableArray<Diagnostic> GetDiagnostics( TextSpan span, CancellationToken cancellationToken )
        => this._semanticModel.GetDiagnostics( span, cancellationToken );

    public ImmutableArray<ISymbol> LookupSymbols( int spanEnd ) => this._semanticModel.LookupSymbols( spanEnd );

    public ControlFlowAnalysis AnalyzeControlFlow( BlockSyntax statement ) => this._semanticModel.AnalyzeControlFlow( statement );

    public TypeInfo GetTypeInfo( SyntaxNode node, CancellationToken cancellationToken = default )
    {
        this._typeInfos = new ConcurrentDictionary<SyntaxNode, TypeInfo>();

        return this._typeInfos.GetOrAdd( node, static ( n, x ) => x.obj._semanticModel.GetTypeInfo( n, x.cancellationToken ), (obj: this, cancellationToken) );
    }
}