using System;
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

        public AdviceLinkerResult ToResult()
        {
            var resultingCompilation = this._input.Compilation;

            var transformationsBySyntaxTree =
                this._input.CompilationModel.ObservableTransformations.Values.OfType<ISyntaxTreeIntroduction>()
                    .Concat( this._input.NonObservableTransformations.OfType<ISyntaxTreeIntroduction>() )
                    .GroupBy( t => t.TargetSyntaxTree, t => t )
                    .ToDictionary( g => g.Key, g => g );

            // First pass. Add all transformations to the compilation, but we don't link them yet.
            var newSyntaxTrees = new List<SyntaxTree>( transformationsBySyntaxTree.Count );
            foreach ( var syntaxTreeGroup in transformationsBySyntaxTree )
            {
                var oldSyntaxTree = syntaxTreeGroup.Key;

                AddIntroducedElementsRewriter addIntroducedElementsRewriter = new ( syntaxTreeGroup.Value );

                var newRoot = addIntroducedElementsRewriter.Visit( oldSyntaxTree.GetRoot() );

                var newSyntaxTree = oldSyntaxTree.WithRootAndOptions( newRoot, oldSyntaxTree.Options );
                newSyntaxTrees.Add( newSyntaxTree );

                resultingCompilation = resultingCompilation.ReplaceSyntaxTree( oldSyntaxTree, newSyntaxTree );
            }

            // Second pass. Count references to modified methods.
            Dictionary<(ISymbol Symbol, int Version), int> referenceCounts = new();
            List<(AspectPart AspectPart, int Version)> aspectParts = new();
            aspectParts.Add( (null, 0) );
            aspectParts.AddRange( this._input.OrderedAspectParts.Select( ( ar, i ) => (ar, i + 1) ) );

            foreach ( var syntaxTree in newSyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot().GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
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
                                    p => p.AspectPart.AspectType.Name == linkerAnnotation.AspectTypeName && p.AspectPart.PartName == linkerAnnotation.PartName )
                                .Select( p => p.Version ).First();
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

            return new AdviceLinkerResult( resultingCompilation, Array.Empty<Diagnostic>() );
        }

        public class AddIntroducedElementsRewriter : CSharpSyntaxRewriter
        {
            private IReadOnlyList<IMemberIntroduction> _memberIntroductors;
            private IReadOnlyList<IInterfaceImplementationIntroduction> _interfaceImplementationIntroductors;
            private Dictionary<ISymbol, List<IntroducedMember>> OverridenSymbols = new();

            public AddIntroducedElementsRewriter( IEnumerable<ISyntaxTreeIntroduction> introductions ) : base()
            {
                this._memberIntroductors = introductions.OfType<IMemberIntroduction>().ToList();
                this._interfaceImplementationIntroductors = introductions.OfType<IInterfaceImplementationIntroduction>().ToList();
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var members = new List<MemberDeclarationSyntax>( node.Members.Count );
                foreach ( var member in node.Members )
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
    }
}
