using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public interface ISemanticModel
{
    SyntaxTree SyntaxTree { get; }

    Compilation Compilation { get; }

    SymbolInfo GetSymbolInfo( SyntaxNode node, CancellationToken cancellationToken = default );

    INamedTypeSymbol? GetDeclaredSymbol( BaseTypeDeclarationSyntax node, CancellationToken cancellationToken = default );

    IMethodSymbol? GetDeclaredSymbol( MethodDeclarationSyntax node, CancellationToken cancellationToken = default );

    IEventSymbol? GetDeclaredSymbol( EventDeclarationSyntax node, CancellationToken cancellationToken = default );

    IPropertySymbol? GetDeclaredSymbol( PropertyDeclarationSyntax node, CancellationToken cancellationToken = default );

    ISymbol? GetDeclaredSymbol( SyntaxNode node, CancellationToken cancellationToken = default );

    NullableContext GetNullableContext( int nodeSpanStart );

    ImmutableArray<Diagnostic> GetDiagnostics( TextSpan span, CancellationToken cancellationToken = default );

    ImmutableArray<ISymbol> LookupSymbols( int spanEnd );

    TypeInfo GetTypeInfo( SyntaxNode node, CancellationToken cancellationToken = default );
    
    ControlFlowAnalysis AnalyzeControlFlow( BlockSyntax statement );
}