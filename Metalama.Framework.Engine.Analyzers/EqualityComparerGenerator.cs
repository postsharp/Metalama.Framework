// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Analyzers;

[Generator( LanguageNames.CSharp )]
public class EqualityComparerGenerator : IIncrementalGenerator
{
    public void Initialize( IncrementalGeneratorInitializationContext context )
    {
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static ( node, _ ) =>
                    node is ClassDeclarationSyntax { AttributeLists: var attributeLists }
                    && attributeLists.Any( al => al.Attributes.Any( a => a.Name is IdentifierNameSyntax { Identifier.ValueText: "GenerateEquality" } ) ),
                static ( ctx, _ ) => (Node: (ClassDeclarationSyntax) ctx.Node, ctx.SemanticModel) );

        context.RegisterSourceOutput( syntaxProvider, static ( spc, tuple ) => Execute( tuple.Node, tuple.SemanticModel, spc ) );
    }

    private static void Execute( ClassDeclarationSyntax node, SemanticModel semanticModel, SourceProductionContext context )
    {
        var cancellationToken = context.CancellationToken;

        var comparerSymbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

        var targetType = (INamedTypeSymbol) comparerSymbol.GetAttributes()
            .Single( a => a.AttributeClass?.Name == "GenerateEqualityAttribute" )
            .ConstructorArguments
            .Single()
            .Value;

        var targetTypeSyntax = (TypeDeclarationSyntax) targetType.DeclaringSyntaxReferences.SingleOrDefault().GetSyntax( cancellationToken );
        var usings = targetTypeSyntax.SyntaxTree.GetCompilationUnitRoot().Usings;

        usings = usings
            .Add( UsingDirective( ParseName( "System.Collections.Generic" ) ) )
            .Add( UsingDirective( ParseName( "System.Linq" ) ) );

        var properties = targetType.GetMembers().OfType<IPropertySymbol>();

        var compilation = semanticModel.Compilation;
        var iEnumerableSymbol = compilation.GetTypeByMetadataName( typeof(System.Collections.IEnumerable).FullName! );

        ExpressionSyntax GetEqualityExpression( IPropertySymbol property )
        {
            var propertySyntax = (PropertyDeclarationSyntax) property.DeclaringSyntaxReferences.Single().GetSyntax( cancellationToken );

            // For enumerables (except strings), we use SequenceEqual.
            if ( property.Type.SpecialType != SpecialType.System_String
                 && compilation.ClassifyConversion( property.Type, iEnumerableSymbol ) is { IsImplicit: true, IsUserDefined: false } )
            {
                if ( property.Type.NullableAnnotation == NullableAnnotation.Annotated )
                {
                    throw new NotImplementedException( "Nullable enumerables are not implemented." );
                }

                // x.Property.SequenceEqual( y.Property )
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "x" ),
                                IdentifierName( propertySyntax.Identifier ) ),
                            IdentifierName( "SequenceEqual" ) ) )
                    .AddArgumentListArguments(
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "y" ),
                                IdentifierName( propertySyntax.Identifier ) ) ) );
            }
            else
            {
                // EqualityComparer<Type>.Default.Equals( x.Property, y.Property )
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                GenericName( Identifier( "EqualityComparer" ) ).AddTypeArgumentListArguments( propertySyntax.Type.WithoutTrivia() ),
                                IdentifierName( "Default" ) ),
                            IdentifierName( "Equals" ) ) )
                    .AddArgumentListArguments(
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "x" ),
                                IdentifierName( propertySyntax.Identifier ) ) ),
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "y" ),
                                IdentifierName( propertySyntax.Identifier ) ) ) );
            }
        }

        var memberwiseEqualityExpression = properties
            .Select( GetEqualityExpression )
            .Aggregate( ( x, y ) => BinaryExpression( SyntaxKind.LogicalAndExpression, x, y ) );

        var referenceEqualityExpression = InvocationExpression( IdentifierName( "ReferenceEquals" ) )
            .AddArgumentListArguments(
                Argument( IdentifierName( "x" ) ),
                Argument( IdentifierName( "y" ) ) );

        var equalityExpression = BinaryExpression( SyntaxKind.LogicalOrExpression, referenceEqualityExpression, ParenthesizedExpression( memberwiseEqualityExpression ) );

        var equalsMethod = MethodDeclaration( PredefinedType( Token( SyntaxKind.BoolKeyword ) ), "Equals" )
            .AddModifiers( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.StaticKeyword ) )
            .AddParameterListParameters(
                Parameter( Identifier( "x" ) ).WithType( IdentifierName( targetTypeSyntax.Identifier ) ),
                Parameter( Identifier( "y" ) ).WithType( IdentifierName( targetTypeSyntax.Identifier ) ) )
            .WithExpressionBody( ArrowExpressionClause( equalityExpression ) )
            .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

        var typeDeclaration = ClassDeclaration( node.Identifier )
            .AddModifiers( Token( SyntaxKind.PartialKeyword ) )
            .AddMembers( equalsMethod )
            .WithLeadingTrivia( TriviaList( Trivia( NullableDirectiveTrivia( Token( SyntaxKind.EnableKeyword ), true ) ) ) );

        var nodeNamespace = (BaseNamespaceDeclarationSyntax) node.SyntaxTree.GetCompilationUnitRoot().Members.Single();

        var namespaceDeclaration = FileScopedNamespaceDeclaration( nodeNamespace.Name )
            .AddMembers( typeDeclaration );

        var compilationUnit = CompilationUnit()
            .WithUsings( usings )
            .AddMembers( namespaceDeclaration );

        context.AddSource( $"{node.Identifier.ValueText}.Generated.cs", compilationUnit.NormalizeWhitespace().ToFullString() );
    }
}
