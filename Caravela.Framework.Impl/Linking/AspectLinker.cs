using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal class AspectLinker
    {
        private readonly AdviceLinkerInput _input;

        public AspectLinker( AdviceLinkerInput input )
        {
            this._input = input;
        }
        public AdviceLinkerResult ToResult(  )
        {
            var resultingCompilation = this._input.Compilation;
            
            
            
            var transformationsBySyntaxTree =
                this._input.CompilationModel.IntroducedElements.Values
                    .GroupBy( t => t.TargetSyntaxTree, t => t )
                    .ToDictionary( g =>  g.Key, g => g );

          
            // First pass. Add all transformations to the compilation, but we don't link them yet.
            var newSyntaxTrees = new List<SyntaxTree>( transformationsBySyntaxTree.Count );
            foreach ( var syntaxTreeGroup in transformationsBySyntaxTree )
            {
                var oldSyntaxTree = syntaxTreeGroup.Key;
                
                AddTransformationRewriter addTransformationRewriter = new (syntaxTreeGroup);
                
                var newRoot = addTransformationRewriter.Visit( oldSyntaxTree.GetRoot() );

                var newSyntaxTree = oldSyntaxTree.WithRootAndOptions( newRoot, default );
                newSyntaxTrees.Add(  newSyntaxTree );
                
                resultingCompilation = resultingCompilation.ReplaceSyntaxTree( oldSyntaxTree, newSyntaxTree );
            }
            
            // Second pass. Count references to modified methods.
            Dictionary<ISymbol, int> referenceCounts = new Dictionary<ISymbol, int>();
            foreach ( var syntaxTree in newSyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot(  ).GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
                {
                    var symbol = resultingCompilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode ).Symbol;
                    if ( referenceCounts.TryGetValue( symbol, out var count ) )
                    {
                        referenceCounts[symbol] = count + 1;
                    }
                    else
                    {
                        referenceCounts[symbol] = 1;
                    }
                }
            }
            
            // Third pass. Linker.
            

        }


        public class AddTransformationRewriter : CSharpSyntaxRewriter
        {
            private IEnumerable<Transformation> _transformationsOnSyntaxTree;


            public AddTransformationRewriter( IEnumerable<Transformation> transformationsOnSyntaxTree, bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
            {
                this._transformationsOnSyntaxTree = transformationsOnSyntaxTree;
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var members = new List<MemberDeclarationSyntax>(node.Members.Count);
                foreach ( var member in members )
                {
                    members.Add( member );
                    members.AddRange( this._transformationsOnSyntaxTree.Where( t => t.InsertPositionNode == node ).Select( t => t.GeneratePreLinkerCode() ) );
                }

                return node.WithMembers( List( members ) );
            }
        }

        public class Walker : CSharpSyntaxWalker
        {
        }

        public class Rewriter : CSharpSyntaxRewriter
        {
            private readonly Dictionary<TypeDeclarationSyntax, List<MethodBuilder>> _introducedMethods;
            private readonly Dictionary<MethodDeclarationSyntax, List<OverriddenMethod>> _overriddenMethods;

            public Rewriter(IEnumerable<Transformation> transformations)
            {
                // This is probably not the best way to match syntax nodes with transformations.

                this._introducedMethods =
                    transformations
                    .OfType<MethodBuilder>()
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
