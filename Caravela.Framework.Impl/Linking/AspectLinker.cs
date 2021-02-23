using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {
        private const string _introducedSyntaxAnnotationId = "AspectLinker_IntroducedSyntax";

        private readonly AdviceLinkerInput _input;

        public AspectLinker( AdviceLinkerInput input )
        {
            this._input = input;
        }

        public AdviceLinkerResult ToResult()
        {
            DiagnosticList diagnostics = new( null );

            var intermediateCompilation = this._input.Compilation;

            var allTransformations =
                this._input.CompilationModel.GetAllObservableTransformations()
                .OfType<ISyntaxTreeTransformation>()
                .Concat( this._input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                .ToList();

            var transformationsBySyntaxTree =
                allTransformations
                .GroupBy( t => t.TargetSyntaxTree, t => t )
                .ToDictionary( g => g.Key, g => g );

            // First pass. Add all transformations to the compilation, but we don't link them yet.
            var newSyntaxTrees = new List<SyntaxTree>( transformationsBySyntaxTree.Count );
            var intermediateIntroducedSyntax = ImmutableMultiValueDictionary<IMemberIntroduction, (SyntaxTree Tree, int NodeAnnotationId)>.Empty;
            foreach ( var syntaxTreeGroup in transformationsBySyntaxTree )
            {
                var oldSyntaxTree = syntaxTreeGroup.Key;

                AddIntroducedElementsRewriter addIntroducedElementsRewriter = new( syntaxTreeGroup.Value, diagnostics );

                var newRoot = addIntroducedElementsRewriter.Visit( oldSyntaxTree.GetRoot() );

                var newSyntaxTree = oldSyntaxTree.WithRootAndOptions( newRoot, oldSyntaxTree.Options );
                newSyntaxTrees.Add( newSyntaxTree );

                intermediateCompilation = intermediateCompilation.ReplaceSyntaxTree( oldSyntaxTree, newSyntaxTree );
                intermediateIntroducedSyntax = intermediateIntroducedSyntax.Merge(
                    addIntroducedElementsRewriter.IntroducedSyntax.SelectMany( x => x.Select( id => (x.Key, Tree: newSyntaxTree, NodeAnnotationId: id) ) )
                    .ToMultiValueDictionary( x => x.Key, x => (x.Tree, x.NodeAnnotationId) ));
            }

            // Second pass. Count references to modified methods.
            Dictionary<(ISymbol Symbol, int Version), int> referenceCounts = new();
            List<(OrderedAspectLayer AspectLayer, int Version)> aspectLayers = new();
            aspectLayers.AddRange( this._input.OrderedAspectLayers.Select( ( ar, i ) => (ar, i + 1) ) );

            foreach ( var syntaxTree in newSyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot().GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
                {
                    var linkerAnnotation = referencingNode.GetLinkerAnnotation()!;
                    int targetVersion;

                    // Determine which version of the semantic is being invoked.
                    switch ( linkerAnnotation.Order )
                    {
                        case LinkerAnnotationOrder.Original:
                            targetVersion = 0;
                            break;

                        case LinkerAnnotationOrder.Default: // Next one.
                            var originatingVersion = aspectLayers.Where(
                                    p => p.AspectLayer.AspectLayerId == linkerAnnotation.AspectLayerId )
                                .Select( p => p.Version ).First();
                            targetVersion = originatingVersion + 1;
                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    // Increment the usage count.
                    var symbolInfo = intermediateCompilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode );

                    if ( symbolInfo.Symbol == null )
                    {
                        continue;
                    }

                    var symbol = symbolInfo.Symbol.AssertNotNull();
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

            var resultingCompilation = intermediateCompilation;

            // Third pass. Linking.
            // Two things it should do:
            //   1. Replace calls to the vanilla method to the call to the right "override" method.

            var nodeAnnotationIds =
                intermediateCompilation.SyntaxTrees
                .SelectMany( st =>
                     st.GetRoot()
                     .GetAnnotatedNodes( _introducedSyntaxAnnotationId )
                     .Select( sn => (Node: sn, Id: sn.GetAnnotations( _introducedSyntaxAnnotationId ).Select( a => int.Parse( a.Data ) ).Single()) ))
                .ToDictionary( x => x.Id, x => x.Node );

            var symbolOverrides =
                transformationsBySyntaxTree.SelectMany( g =>
                    g.Value
                    .OfType<IOverriddenElement>()
                    .OfType<IMemberIntroduction>()
                    .SelectMany( mi => mi.GetIntroducedMembers( new MemberIntroductionContext( diagnostics ) ) )
                    .Select( x => ((IOverriddenElement) x.Introductor).OverriddenElement switch
                     {
                        Method method => (Element: ((IOverriddenElement) x.Introductor).OverriddenElement, Symbol: method.Symbol, IntroducedMember: x),
                        MethodBuilder builder => (Element: ((IOverriddenElement) x.Introductor).OverriddenElement, Symbol: FindInIntermediateCompilation( builder ), IntroducedMember: x),
                        _ => throw new AssertionFailedException()
                    } ));

            var symbolOverridesLookup =
                symbolOverrides.ToMultiValueDictionary( x => x.Symbol.AssertNotNull(), x => x.IntroducedMember, StructuralSymbolComparer.Instance );

            var rewriter = new OverrideOrderRewriter( intermediateCompilation, this._input.OrderedAspectLayers, symbolOverridesLookup );

            foreach ( var syntaxTree in intermediateCompilation.SyntaxTrees )
            {
                var newRoot = rewriter.Visit( syntaxTree.GetRoot() );

                var newSyntaxTree = syntaxTree.WithRootAndOptions( newRoot, syntaxTree.Options );

                resultingCompilation = resultingCompilation.ReplaceSyntaxTree( syntaxTree, newSyntaxTree );
            }

            return new AdviceLinkerResult( resultingCompilation, diagnostics.Diagnostics );

            ISymbol FindInIntermediateCompilation( ICodeElement codeElement )
            {
                if ( codeElement is MethodBuilder method )
                {
                    var pair = intermediateIntroducedSyntax[method].Single();
                    var symbol = intermediateCompilation.GetSemanticModel( pair.Tree ).GetDeclaredSymbol( nodeAnnotationIds[pair.NodeAnnotationId] ).AssertNotNull();
                    return symbol;
                }

                throw new NotSupportedException();
            }
        }
    }
}
