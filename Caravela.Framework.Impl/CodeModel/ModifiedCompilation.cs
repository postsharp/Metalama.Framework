using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    // TODO: introductions
    sealed class ModifiedCompilation : BaseCompilation
    {
        private readonly BaseCompilation _originalCompilation;
        private readonly IReactiveCollection<AdviceInstance> _addedAdvices;

        public ModifiedCompilation( BaseCompilation originalCompilation, IReactiveCollection<AdviceInstance> addedAdvices )
        {
            this._originalCompilation = originalCompilation;
            this._addedAdvices = addedAdvices;
        }

        public override IReactiveCollection<INamedType> DeclaredTypes => this._originalCompilation.DeclaredTypes;

        public override IReactiveCollection<INamedType> DeclaredAndReferencedTypes => this._originalCompilation.DeclaredAndReferencedTypes;

        public override INamedType? GetTypeByReflectionName( string reflectionName ) => this._originalCompilation.GetTypeByReflectionName( reflectionName );

        internal override CSharpCompilation GetPrimeCompilation() => this._originalCompilation.GetPrimeCompilation();

        internal override IReactiveCollection<AdviceInstance> CollectAdvices() => this._originalCompilation.CollectAdvices().Union( this._addedAdvices );

        internal override CSharpCompilation GetRoslynCompilation()
        {
            var adviceDriver = new AdviceDriver();

            // Modified compilations can form a linked list. First, find the Roslyn compilation at the start of the list and collect all advices from the list.
            var primeCompilation = this.GetPrimeCompilation();
            var transformations = this.CollectAdvices().SelectMany( a => adviceDriver.GetResult( a ).Transformations ).GetValue();

            var result = primeCompilation;

            var rewriter = new CompilationRewriter( transformations );

            // TODO: make this and the rewriter more efficient by avoiding unnecessary work
            foreach (var tree in result.SyntaxTrees )
            {
                result = result.ReplaceSyntaxTree( tree, tree.WithRootAndOptions( rewriter.Visit( tree.GetRoot() ), tree.Options ) );
            }

            return result;
        }

        class CompilationRewriter : CSharpSyntaxRewriter
        {
            private readonly List<OverriddenMethod> _overriddenMethods;

            public CompilationRewriter(IEnumerable<Transformation> transformations)
            {
                transformations = transformations.ToList();

                this._overriddenMethods = transformations.OfType<OverriddenMethod>().ToList();

                // make sure all input transformations are accounted for
                Debug.Assert( this._overriddenMethods.Count == transformations.Count() );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private T? VisitTypeDeclaration<T>( T node ) where T : TypeDeclarationSyntax
            {
                var members = new List<MemberDeclarationSyntax>( node.Members.Count );

                foreach ( var member in node.Members )
                {
                    switch ( member )
                    {
                        case MethodDeclarationSyntax method:
                            OverriddenMethod? foundTransformation = null;

                            foreach ( var transformation in this._overriddenMethods )
                            {
                                if ( transformation.OverriddenDeclaration.GetSyntaxNode() == member )
                                {
                                    foundTransformation = transformation;
                                    break;
                                }
                            }

                            if (foundTransformation != null)
                            {
                                // original method, but with _Original added to its name
                                members.Add(
                                    method.WithIdentifier( SyntaxFactory.Identifier( method.Identifier.ValueText + "_Original" ).WithTriviaFrom( method.Identifier ) ) );

                                // original method, but with its body replaced
                                members.Add( method.WithBody( foundTransformation.MethodBody ) );
                            }
                            else
                            {
                                members.Add( method );
                            }

                            break;

                        default:
                            members.Add( member );
                            break;
                    }
                }

                return (T) node.WithMembers( SyntaxFactory.List( members ) );
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                var result = (MethodDeclarationSyntax) base.VisitMethodDeclaration( node )!;

                foreach (var transformation in this._overriddenMethods )
                {
                    if (transformation.OverriddenDeclaration.GetSyntaxNode() == node)
                    {
                        result = result.WithBody( transformation.MethodBody );
                    }
                }

                return result;
            }
        }
    }
}
