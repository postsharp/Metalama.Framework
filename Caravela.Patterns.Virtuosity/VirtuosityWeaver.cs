using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Caravela.Patterns.Virtuosity
{
    [AspectWeaver( typeof( VirtuosityAspect ) )]
    class VirtuosityWeaver : IAspectWeaver
    {
        public CSharpCompilation Transform( AspectWeaverContext context ) => new Rewriter().VisitAllTrees( context.Compilation );

        class Rewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Parent is not ClassDeclarationSyntax classDeclaration)
                    return node;

                // Note: this won't work if the method is in one part of a partial class and only the other part has the sealed modifier
                if (classDeclaration.Modifiers.Any(SealedKeyword))
                    return node;

                var modifiers = node.Modifiers;

                var forbiddenModifiers = new[] { StaticKeyword, SealedKeyword, VirtualKeyword, OverrideKeyword };
                var requiredModifiers = new[] { PublicKeyword, ProtectedKeyword, InternalKeyword };

                if (forbiddenModifiers.Any(modifier => node.Modifiers.Any(modifier))
                    || !requiredModifiers.Any(modifier => node.Modifiers.Any(modifier)))
                    return node;

                return node.AddModifiers(SyntaxFactory.Token(VirtualKeyword));
            }
        }
    }
}
