using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Sdk;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Patterns.AutoCancellationToken
{
    [AspectWeaver(typeof(AutoCancellationTokenAttribute))]
    class AutoCancellationTokenWeaver : IAspectWeaver
    {
        public CSharpCompilation Transform(AspectWeaverContext context)
        {
            var compilation = context.Compilation;
            var instancesNodes = context.AspectInstances.SelectMany(a => a.CodeElement.ToSyntaxNodes());
            RunRewriter(new AnnotateNodesRewriter(instancesNodes));
            RunRewriter(new AddCancellationTokenToMethodsRewriter(compilation));
            RunRewriter(new AddCancellationTokenToInvocationsRewriter(compilation));
            return compilation;

            void RunRewriter(CSharpSyntaxRewriter rewriter)
            {
                foreach (var tree in compilation.SyntaxTrees)
                {
                    compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(rewriter.Visit(tree.GetRoot()), tree.Options));
                }
            }
        }

        abstract class RewriterBase : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => this.VisitTypeDeclaration(node, base.VisitInterfaceDeclaration);
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) => this.VisitTypeDeclaration(node, base.VisitClassDeclaration);
            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node) => this.VisitTypeDeclaration(node, base.VisitStructDeclaration);
            public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node) => this.VisitTypeDeclaration(node, base.VisitRecordDeclaration);

            protected abstract T VisitTypeDeclaration<T>(T node, Func<T, SyntaxNode> baseVisit) where T : TypeDeclarationSyntax;

            protected static readonly TypeSyntax CancellationTokenType = ParseTypeName(typeof(CancellationToken).FullName);

            protected static bool IsCancellationToken(IParameterSymbol parameter) => parameter.OriginalDefinition.Type.ToString() == typeof(CancellationToken).FullName;

            // Make sure VisitInvocationExpression is not called for expressions inside members that are not methods
            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) => node;
            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node) => node;
            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node) => node;
            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node) => node;
            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => node;
            public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node) => node;

            protected const string CancellationAttributeName = "Caravela.Patterns.AutoCancellationToken.AutoCancellationTokenAttribute";
        }

        sealed class AnnotateNodesRewriter : RewriterBase
        {
            private readonly HashSet<CSharpSyntaxNode> instancesNodes;

            public AnnotateNodesRewriter(IEnumerable<CSharpSyntaxNode> instancesNodes) => this.instancesNodes = new(instancesNodes);

            public static SyntaxAnnotation Annotation { get; } = new SyntaxAnnotation();

            protected override T VisitTypeDeclaration<T>(T node, Func<T, SyntaxNode> baseVisit)
            {
                if (!this.instancesNodes.Contains(node))
                    return node;

                return node.WithAdditionalAnnotations(Annotation);
            }
        }

        sealed class AddCancellationTokenToMethodsRewriter : RewriterBase
        {
            private readonly Compilation compilation;

            public AddCancellationTokenToMethodsRewriter(Compilation compilation) => this.compilation = compilation;

            protected override T VisitTypeDeclaration<T>(T node, Func<T, SyntaxNode> baseVisit)
            {
                if (!node.HasAnnotation(AnnotateNodesRewriter.Annotation))
                    return node;

                return (T)baseVisit(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var semanticModel = this.compilation.GetSemanticModel(node.SyntaxTree);

                var methodSymbol = semanticModel.GetDeclaredSymbol(node);

                if (!methodSymbol.IsAsync || methodSymbol.Parameters.Any(IsCancellationToken))
                    return node;

                return node.AddParameterListParameters(Parameter(
                    default, default, CancellationTokenType, Identifier("cancellationToken"),
                    EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression))));
            }
        }

        sealed class AddCancellationTokenToInvocationsRewriter : RewriterBase
        {
            private readonly Compilation compilation;

            public AddCancellationTokenToInvocationsRewriter(Compilation compilation) => this.compilation = compilation;

            protected override T VisitTypeDeclaration<T>(T node, Func<T, SyntaxNode> baseVisit)
            {
                if (!node.HasAnnotation(AnnotateNodesRewriter.Annotation))
                    return node;

                var semanticModel = this.compilation.GetSemanticModel(node.SyntaxTree);

                var symbol = semanticModel.GetDeclaredSymbol(node);

                var attributeData = symbol.GetAttributes().SingleOrDefault(a => a.AttributeClass.ToString() == CancellationAttributeName);

                // TODO: removing of attributes should probably be handled automatically by the framework

                var attributeSyntax = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
                var attributeList = (AttributeListSyntax)attributeSyntax.Parent;
                int attributeIndex = attributeList.Attributes.IndexOf(attributeSyntax);
                int attributeListIndex = node.AttributeLists.IndexOf(attributeList);

                var newNode = (T)baseVisit(node);

                attributeList = newNode.AttributeLists[attributeListIndex];
                if (attributeList.Attributes.Count == 1)
                {
                    newNode = newNode.RemoveNode(attributeList, default);
                }
                else
                {
                    attributeSyntax = attributeList.Attributes[attributeIndex];
                    newNode = newNode.RemoveNode(attributeSyntax, default);
                }

                return newNode;
            }

            private string cancellationTokenParameterName;

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var semanticModel = this.compilation.GetSemanticModel(node.SyntaxTree);

                var methodSymbol = semanticModel.GetDeclaredSymbol(node);

                if (!methodSymbol.IsAsync)
                    return node;

                var cancellationTokenParmeters = methodSymbol.Parameters.Where(IsCancellationToken);

                if (cancellationTokenParmeters.Count() != 1)
                    return node;

                this.cancellationTokenParameterName = cancellationTokenParmeters.Single().Name;

                return base.VisitMethodDeclaration(node);
            }

            public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) => this.VisitFunction(node, false, base.VisitAnonymousMethodExpression);
            // TODO: add support for static lambdas when MS.CA.CS is upgraded to 3.8
            public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => this.VisitFunction(node, false, base.VisitParenthesizedLambdaExpression);
            public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => this.VisitFunction(node, false, base.VisitSimpleLambdaExpression);
            public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) =>
                this.VisitFunction(node, node.Modifiers.Any(SyntaxKind.StaticKeyword), base.VisitLocalFunctionStatement);

            private T VisitFunction<T>(T node, bool isStatic, Func<T, SyntaxNode> baseVisit) where T : SyntaxNode
            {
                if (isStatic)
                    return node;

                return (T)baseVisit(node);
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                bool addCt = false;

                var semanticModel = this.compilation.GetSemanticModel(node.SyntaxTree);

                var invocationWithCt = node.AddArgumentListArguments(Argument(DefaultExpression(CancellationTokenType)));
                int newInvocationArgumentsCount = invocationWithCt.ArgumentList.Arguments.Count;

                var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(node.SpanStart, invocationWithCt, default).Symbol as IMethodSymbol;

                if (
                    // the code compiles
                    speculativeSymbol != null &&
                    // the added parameter corresponds to its own argument
                    speculativeSymbol.Parameters.Length >= newInvocationArgumentsCount &&
                    // that argument is CancellationToken
                    IsCancellationToken(speculativeSymbol.Parameters[newInvocationArgumentsCount - 1]))
                {
                    addCt = true;
                }

                node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

                if (addCt)
                    node = node.AddArgumentListArguments(Argument(IdentifierName( this.cancellationTokenParameterName )));

                return node;
            }
        }
    }
}
