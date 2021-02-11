using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Transformations;
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
        
        public AdviceLinkerResult ToResult( )
        {
            var resultingCompilation = this._input.Compilation;

            var transformationsBySyntaxTree =
                this._input.CompilationModel.ObservableTransformations.Values.OfType<ISyntaxTreeIntroduction>()
                    .Concat( this._input.NonObservableTransformations.OfType<ISyntaxTreeIntroduction>() )
                    .GroupBy( t => t.TargetSyntaxTree, t => t )
                    .ToDictionary( g =>  g.Key, g => g );

            // First pass. Add all transformations to the compilation, but we don't link them yet.
            var newSyntaxTrees = new List<SyntaxTree>( transformationsBySyntaxTree.Count );
            foreach ( var syntaxTreeGroup in transformationsBySyntaxTree )
            {
                var oldSyntaxTree = syntaxTreeGroup.Key;
                
                AddIntroducedElementsRewriter addIntroducedElementsRewriter = new (syntaxTreeGroup.Value);
                
                var newRoot = addIntroducedElementsRewriter.Visit( oldSyntaxTree.GetRoot() );

                var newSyntaxTree = oldSyntaxTree.WithRootAndOptions( newRoot, default );
                newSyntaxTrees.Add(  newSyntaxTree );
                
                resultingCompilation = resultingCompilation.ReplaceSyntaxTree( oldSyntaxTree, newSyntaxTree );
            }
            
            // Second pass. Count references to modified methods.
            Dictionary<(ISymbol symbol, int version), int> referenceCounts = new ();
            List<(AspectPart aspectPart, int version)> aspectParts = new();
            aspectParts.Add( (null, 0) );
            aspectParts.AddRange( this._input.OrderedAspectParts.Select( (ar, i) => (ar, i + 1) ) );
            
            foreach ( var syntaxTree in newSyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot( ).GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
                {
                    var linkerAnnotation = referencingNode.GetLinkerAnnotation();
                    int targetVersion;

                    // Determine which version of the semantic is being invoked.
                    switch ( linkerAnnotation.Order )
                    {
                        case LinkerAnnotationOrder.Original:
                            targetVersion = 0;
                            break;
                            
                        case LinkerAnnotationOrder.Default: // Next one.
                            var originatingVersion = aspectParts.Where(
                                    p => p.aspectPart.AspectType.Name == linkerAnnotation.AspectTypeName && p.aspectPart.PartName == linkerAnnotation.PartName )
                                .Select( p => p.version ).First();
                            targetVersion = originatingVersion + 1;
                            break;
                            
                            default:
                                throw new AssertionFailedException();
                    }
                    

                    // Increment the usage count.
                    var symbol = resultingCompilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode ).Symbol.AssertNotNull();
                    var symbolVersion = (symbol, targetVersion);
                    
                    if ( referenceCounts.TryGetValue( symbolVersion, out var count ) )
                    {
                        referenceCounts[symbolVersion] = count + 1;
                    }
                    else
                    {
                        referenceCounts[symbolVersion] = 1;
                    }
                }
            }
            
            // Third pass. Linker.
            // Two things it should do:
            //   1. Replace calls to the vanilla method to the call to the right "override" method.
            

        }


        public class AddIntroducedElementsRewriter : CSharpSyntaxRewriter
        {
            private IReadOnlyList<IMemberIntroduction> _memberIntroductors;
            private IReadOnlyList<IInterfaceImplementationIntroduction> _interfaceImplementationIntroductors;
            private Dictionary<ISymbol, List<IntroducedMember>> OverridenSymbols = new ();


            public AddIntroducedElementsRewriter( IEnumerable<ISyntaxTreeIntroduction> introductions) : base()
            {
                this._memberIntroductors = introductions.OfType<IMemberIntroduction>().ToList();
                this._interfaceImplementationIntroductors = introductions.OfType<IInterfaceImplementationIntroduction>().ToList();
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                 
                var members = new List<MemberDeclarationSyntax>(node.Members.Count);
                foreach ( var member in members )
                {
                    members.Add( member );
                    
                    // TODO: optimize linq
                    var introducedMembers = this._memberIntroductors
                        .Where( t => t.InsertPositionNode == member )
                        .SelectMany( t => t.GetIntroducedMembers() )
                        .ToList();
                    
                    members.AddRange( introducedMembers.Select( i => i.Syntax ) );
                    
                    
                    // TODO: add to OverridenSymbols if the introduction implements IOverridenElement
                }

                members.AddRange( this._memberIntroductors
                    .Where( t => t.InsertPositionNode == node )
                    .SelectMany( t => t.GetIntroducedMembers() )
                    .Select( i => i.Syntax ) );

                return node.WithMembers( List( members ) );
            }
        }

        public class Walker : CSharpSyntaxWalker
        {
        }

        public class Rewriter : CSharpSyntaxRewriter
        {
            private readonly Dictionary<TypeDeclarationSyntax, List<MethodTransformationBuilder>> _introducedMethods;
            private readonly Dictionary<MethodDeclarationSyntax, List<OverriddenMethod>> _overriddenMethods;

            public Rewriter(IEnumerable<INonObservableTransformation> transformations)
            {
                // This is probably not the best way to match syntax nodes with transformations.

                this._introducedMethods =
                    transformations
                    .OfType<MethodTransformationBuilder>()
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
