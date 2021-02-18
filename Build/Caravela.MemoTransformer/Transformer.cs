using System.Collections.Generic;
using System.Linq;
using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.MemoTransformer
{
    [Transformer]
    internal class Transformer : ISourceTransformer
    {

        public Compilation Execute( TransformerContext context )
        {
            var rewriter = new Rewriter();
            var compilation = context.Compilation;
            foreach ( var tree in compilation.SyntaxTrees )
            {
                compilation = compilation.ReplaceSyntaxTree( tree, tree.WithRootAndOptions( rewriter.Visit( tree.GetRoot() ), tree.Options ) );
            }

            foreach ( var diagnostic in rewriter.Diagnostics )
            {
                context.ReportDiagnostic( diagnostic );
            }

            return compilation;
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private List<FieldDeclarationSyntax>? _fieldsToAdd;

            public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

            public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                if ( !node.AttributeLists.SelectMany( al => al.Attributes )
                    .Any( a => a.Name.ToString() == "Memo" || a.Name.ToString() == "MemoAttribute" ) )
                {
                    return node;
                }

                if ( node.ExpressionBody == null )
                {
                    this.Diagnostics.Add( Diagnostic.Create( _nonExpressionBodyError, node.GetLocation() ) );
                    return node;
                }

                if ( node.Modifiers.Any( SyntaxKind.StaticKeyword ) )
                {
                    this.Diagnostics.Add( Diagnostic.Create( _staticPropertyError, node.GetLocation() ) );
                    return node;
                }

                var fieldName = "@" + char.ToLowerInvariant( node.Identifier.ValueText[0] ) + node.Identifier.ValueText.Substring( 1 );

                var expression = node.ExpressionBody.Expression;

                // PERF: read the field into a local, to avoid unnecessary double read in the fast case

                // if (field == null)
                //     Interlocked.CompareExchange(ref field, expression, null);
                // return field;

                var block = Block(
                    IfStatement(
                        BinaryExpression( SyntaxKind.EqualsExpression, IdentifierName( fieldName ), LiteralExpression( SyntaxKind.NullLiteralExpression ) ),
                        ExpressionStatement( InvocationExpression( ParseExpression( "Interlocked.CompareExchange" ) ).AddArgumentListArguments(
                            Argument( IdentifierName( fieldName ) ).WithRefKindKeyword( Token( SyntaxKind.RefKeyword ) ),
                            Argument( expression ),
                            Argument( LiteralExpression( SyntaxKind.NullLiteralExpression ) ) ) ) ),
                    ReturnStatement( IdentifierName( fieldName ) ) );

                var newNode = node.WithExpressionBody( null ).WithSemicolonToken( default )
                    .AddAccessorListAccessors( AccessorDeclaration( SyntaxKind.GetAccessorDeclaration, block ) );

                // PropertyType? field;
                this._fieldsToAdd!.Add( FieldDeclaration( VariableDeclaration( NullableType( node.Type ) ).AddVariables( VariableDeclarator( fieldName ) ) ) );

                return newNode;
            }

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var parentFieldsToAdd = this._fieldsToAdd;
                this._fieldsToAdd = new();

                var result = (ClassDeclarationSyntax) base.VisitClassDeclaration( node )!;

                result = result.AddMembers( this._fieldsToAdd.ToArray<MemberDeclarationSyntax>() );

                this._fieldsToAdd = parentFieldsToAdd;

                return result;
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                const string usingSystemThreading = "using System.Threading;";

                node = (CompilationUnitSyntax) base.VisitCompilationUnit( node )!;

                if ( node.Usings.All( u => u.ToString() != usingSystemThreading ) )
                {
                    node = node.AddUsings( ParseCompilationUnit( usingSystemThreading ).Usings.Single() );
                }

                return node;
            }
        }

        private static readonly DiagnosticDescriptor _nonExpressionBodyError =
            new DiagnosticDescriptor(
                "CMT001",
                "Only expression-bodied properties are supported.",
                "Only expression-bodied properties are supported.",
                "Caravela.MemoTransformer",
                DiagnosticSeverity.Error,
                true );

        private static readonly DiagnosticDescriptor _staticPropertyError =
            new DiagnosticDescriptor(
                "CMT002",
                "Static properties are not supported.",
                "Static properties are not supported.",
                "Caravela.MemoTransformer",
                DiagnosticSeverity.Error,
                true );
    }
}
