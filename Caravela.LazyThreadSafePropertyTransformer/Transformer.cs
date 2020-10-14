using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynEx;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.LazyThreadSafePropertyTransformer
{
    [Transformer]
    class Transformer : ISourceTransformer
    {
        public Compilation Execute(TransformerContext context)
        {
            var rewriter = new Rewriter();
            var compilation = context.Compilation;
            foreach (var tree in compilation.SyntaxTrees)
            {
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(rewriter.Visit(tree.GetRoot()), tree.Options));
            }
            return compilation;
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            List<FieldDeclarationSyntax> fieldsToAdd;

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (!node.AttributeLists.SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() == "Memo" || a.Name.ToString() == "MemoAttribute"))
                    return node;

                if (node.ExpressionBody == null)
                    throw new NotSupportedException("Only expression-bodied properties are supported.");

                if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
                    throw new NotSupportedException("Static properties are not supported.");

                string fieldName = "@" + char.ToLowerInvariant(node.Identifier.ValueText[0]) + node.Identifier.ValueText.Substring(1);

                var expression = node.ExpressionBody.Expression;

                // PERF: read the field into a local, to avoid unnecessary double read in the fast case

                // if (field == null)
                //     Interlocked.CompareExchange(ref field, expression, null);
                // return field;

                var block = Block(
                    IfStatement(
                        BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(fieldName), LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        ExpressionStatement(InvocationExpression(ParseExpression("Interlocked.CompareExchange")).AddArgumentListArguments(
                            Argument(IdentifierName(fieldName)).WithRefKindKeyword(Token(SyntaxKind.RefKeyword)),
                            Argument(expression),
                            Argument(LiteralExpression(SyntaxKind.NullLiteralExpression))))),
                    ReturnStatement(IdentifierName(fieldName)));


                var newNode = node.WithExpressionBody(null).WithSemicolonToken(default)
                    .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, block));

                // PropertyType? field;
                fieldsToAdd.Add(FieldDeclaration(VariableDeclaration(NullableType(node.Type)).AddVariables(VariableDeclarator(fieldName))));

                return newNode;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                var parentFieldsToAdd = fieldsToAdd;
                fieldsToAdd = new();

                var result = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

                result = result.AddMembers(fieldsToAdd.ToArray());
                fieldsToAdd = parentFieldsToAdd;

                return result;
            }

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                string usingSystemThreading = "using System.Threading;";

                if (!node.Usings.Any(u => u.ToString() == usingSystemThreading))
                    node = node.AddUsings(ParseCompilationUnit(usingSystemThreading).Usings.Single());

                return base.VisitCompilationUnit(node);
            }
        }
    }
}
