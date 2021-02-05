using Caravela.Framework.Advices;
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
    sealed class ModifiedCompilationModel : CompilationModel
    {
        private readonly CompilationModel _originalCompilation;
        private readonly IReactiveCollection<Transformation> _transformations;

        public ModifiedCompilationModel( CompilationModel originalCompilation, IReactiveCollection<Transformation> transformations )
        {
            this._originalCompilation = originalCompilation;
            this._transformations = transformations;
        }

        public override IReactiveCollection<INamedType> DeclaredTypes => this._originalCompilation.DeclaredTypes;

        public override IReactiveCollection<INamedType> DeclaredAndReferencedTypes => this._originalCompilation.DeclaredAndReferencedTypes;

        public override IReactiveCollection<IAttribute> Attributes => this._originalCompilation.Attributes;

        public override INamedType? GetTypeByReflectionName( string reflectionName ) => this._originalCompilation.GetTypeByReflectionName( reflectionName );

        internal override CSharpCompilation GetPrimeCompilation() => this._originalCompilation.GetPrimeCompilation();

        internal override IReactiveCollection<Transformation> CollectTransformations() => this._originalCompilation.CollectTransformations().Union( this._transformations );

        internal override CSharpCompilation GetRoslynCompilation()
        {
            // Modified compilations can form a linked list. First, find the Roslyn compilation at the start of the list and collect all advices from the list.
            var primeCompilation = this.GetPrimeCompilation();
            var transformations = this.CollectTransformations();

            var rewriter = new CompilationRewriter( this._originalCompilation, transformations );

            // TODO: make this and the rewriter more efficient by avoiding unnecessary work
            var result = rewriter.VisitAllTrees( primeCompilation );

            return result;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null ) => this._originalCompilation.ToDisplayString( format, context );


        sealed class CompilationRewriter : CSharpSyntaxRewriter
        {
            private ICompilation _baseCompilation;
            private readonly List<IntroducedMethod> _introducedMethods;
            private readonly List<OverriddenMethod> _overriddenMethods;

            public CompilationRewriter( ICompilation baseCompilation, IReactiveCollection<Transformation> transformations )
            {
                IList<Transformation> transformationList = (IList<Transformation>) transformations.Materialize().GetValue().ToList();

                this._baseCompilation = baseCompilation;
                this._introducedMethods = transformationList.OfType<IntroducedMethod>().ToList();
                this._overriddenMethods = transformationList.OfType<OverriddenMethod>().ToList();
            }

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private TypeDeclarationSyntax VisitTypeDeclaration( TypeDeclarationSyntax node )
            {
                var newMembers = new List<MemberDeclarationSyntax>( node.Members.Count );

                foreach (var transformation in this._introducedMethods)
                {
                    if (transformation.ContainingElement!.GetSyntaxNode() == node)
                    {
                        newMembers.Add( transformation.GetDeclaration() );
                    }
                }

                var introducedMembers = new List<MemberDeclarationSyntax>( newMembers );

                foreach ( var member in node.Members.Concat( introducedMembers ) )
                {
                    var newMember = member.WithAttributeLists( this.VisitList( member.AttributeLists ) );

                    foreach(var transformation in this._overriddenMethods)
                    {
                        var overridenSyntax = transformation.OverridenDeclaration!.GetSyntaxNodes().FirstOrDefault();

                        if ( overridenSyntax == member )
                        {
                            foreach (var overridingSyntax in transformation.GetOverrides(this._baseCompilation))
                            {
                                newMembers.Add( overridingSyntax );
                            }
                        }
                    }
                }

                newMembers.AddRange( node.Members );

                return node
                    .WithMembers( SyntaxFactory.List( newMembers ) )
                    .WithAttributeLists( this.VisitList( node.AttributeLists ) );
            }
        }
    }
}
