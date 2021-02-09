using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    internal class AspectLinker
    {
        public AdviceLinkerResult ToResult( AdviceLinkerContext context )
        {
            return new AdviceLinkerResult( context.Compilation.GetRoslynCompilation(), Array.Empty<Diagnostic>().ToImmutableReactive() );
        }

        public class Walker : CSharpSyntaxWalker
        {
        }

        public class Rewriter : CSharpSyntaxRewriter
        {
            private readonly Dictionary<TypeDeclarationSyntax, List<IntroducedMethod>> _introducedMethods;
            private readonly Dictionary<MethodDeclarationSyntax, List<OverriddenMethod>> _overriddenMethods;

            public Rewriter(IEnumerable<Transformation> transformations)
            {
                // This is probably not the best way to match syntax nodes with transformations.

                this._introducedMethods =
                    transformations
                    .OfType<IntroducedMethod>()
                    .SelectMany(x => x.GetSyntaxNodes().Select( y => (type: ((IToSyntax) x.TargetDeclaration).GetSyntaxNode(), transformation: x) ) )
                    .GroupBy( x => x.type )
                    .ToDictionary( x => (TypeDeclarationSyntax) x.Key, x => x.Select( y => y.transformation ).ToList() );

                this._overriddenMethods =
                    transformations
                    .OfType<OverriddenMethod>()
                    .Select( x => (method: ((IToSyntax) x.OverridenDeclaration).GetSyntaxNode(), transformation: x) )
                    .GroupBy( x => x.method )
                    .ToDictionary( x => (MethodDeclarationSyntax) x.Key, x => x.Select( y => y.transformation ).ToList() );
            }

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private TypeDeclarationSyntax VisitTypeDeclaration( TypeDeclarationSyntax node )
            {
                return node;
            }
        }
    }
}
