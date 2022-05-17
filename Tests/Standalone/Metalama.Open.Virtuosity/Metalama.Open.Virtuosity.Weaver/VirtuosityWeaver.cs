using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Metalama.Open.Virtuosity
{
    [MetalamaPlugIn, AspectWeaver(aspectType: typeof(VirtualizeAttribute))]
    public class VirtuosityWeaver : IAspectWeaver
    {
        void IAspectWeaver.Transform(AspectWeaverContext context)
        {
            Debugger.Break();
            context.RewriteAspectTargets(new Rewriter());
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private static readonly SyntaxKind[]? forbiddenModifiers = new[] { StaticKeyword, SealedKeyword, VirtualKeyword, OverrideKeyword };
            private static readonly SyntaxKind[]? requiredModifiers = new[] { PublicKeyword, ProtectedKeyword, InternalKeyword };

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Parent is not ClassDeclarationSyntax classDeclaration)
                {
                    return node;
                }

                // Note: this won't work if the method is in one part of a partial class and only the other part has the sealed modifier
                if (classDeclaration.Modifiers.Any(SealedKeyword))
                {
                    return node;
                }

                SyntaxTokenList modifiers = node.Modifiers;

                if (forbiddenModifiers.Any(modifier => node.Modifiers.Any(modifier))
                    || !requiredModifiers.Any(modifier => node.Modifiers.Any(modifier)))
                {
                    return node;
                }

                return node.AddModifiers(SyntaxFactory.Token(VirtualKeyword));
            }
        }
    }
}
