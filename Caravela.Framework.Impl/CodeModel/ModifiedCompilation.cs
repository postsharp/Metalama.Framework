using Caravela.Framework.Aspects;
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

        public override IReactiveCollection<IAttribute> Attributes => this._originalCompilation.Attributes;

        public override INamedType? GetTypeByReflectionName( string reflectionName ) => this._originalCompilation.GetTypeByReflectionName( reflectionName );

        internal override CSharpCompilation GetPrimeCompilation() => this._originalCompilation.GetPrimeCompilation();

        internal override IReactiveCollection<AdviceInstance> CollectAdvices() => this._originalCompilation.CollectAdvices().Union( this._addedAdvices );

        internal override CSharpCompilation GetRoslynCompilation()
        {
            var adviceDriver = new AdviceDriver();

            // Modified compilations can form a linked list. First, find the Roslyn compilation at the start of the list and collect all advices from the list.
            var primeCompilation = this.GetPrimeCompilation();
            var transformations = this.CollectAdvices().SelectMany( a => adviceDriver.GetResult( a ).Transformations ).GetValue();

            var multiAdviceElements = transformations
                .GroupBy( t => t is OverriddenElement oe ? oe.OverriddenDeclaration : null )
                .Where( g => g.Count() > 1 );

            foreach (var element in multiAdviceElements)
            {
                if ( element.Key != null )
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MoreThanOneAdvicePerElement, element.Key.GetSyntaxNode().GetLocation(), element.Key );
            }

            var rewriter = new CompilationRewriter( transformations );

            // TODO: make this and the rewriter more efficient by avoiding unnecessary work
            var result = rewriter.VisitAllTrees( primeCompilation );

            return result;
        }

        sealed class CompilationRewriter : CSharpSyntaxRewriter
        {
            private readonly List<OverriddenMethod> _overriddenMethods;

            public CompilationRewriter( IEnumerable<Transformation> transformations )
            {
                transformations = transformations.ToList();

                this._overriddenMethods = transformations.OfType<OverriddenMethod>().ToList();

                // make sure all input transformations are accounted for
                Debug.Assert( this._overriddenMethods.Count == transformations.Count() );
            }

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private TypeDeclarationSyntax VisitTypeDeclaration( TypeDeclarationSyntax node )
            {
                var newMembers = new List<MemberDeclarationSyntax>( node.Members.Count );

                foreach ( var member in node.Members )
                {
                    var newMember = member.WithAttributeLists( this.VisitList( member.AttributeLists ) );

                    switch ( newMember )
                    {
                        case BaseMethodDeclarationSyntax newMethod:
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
                                //// original method, but with _Original added to its name
                                //members.Add(
                                //    newMethod.WithIdentifier( SyntaxFactory.Identifier( newMethod.Identifier.ValueText + AspectDriver.OriginalMemberSuffix )
                                //        .WithTriviaFrom( newMethod.Identifier ) ) );

                                // original method, but with its body replaced
                                newMembers.Add( newMethod.WithBody( foundTransformation.MethodBody ) );
                            }
                            else
                            {
                                newMembers.Add( newMethod );
                            }

                            break;

                        default:
                            newMembers.Add( newMember );
                            break;
                    }
                }

                return node
                    .WithMembers( SyntaxFactory.List( newMembers ) )
                    .WithAttributeLists( this.VisitList( node.AttributeLists ) );
            }
        }
    }
}
