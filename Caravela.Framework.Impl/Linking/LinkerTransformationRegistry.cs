// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Optimization structure which tracks introductions created by introduction step of the linker.
    /// </summary>
    internal class LinkerTransformationRegistry
    {
        // TODO: This whole class is a mess, should be refactored and some caches should be moved elsewhere where it makes more sense.
        //       Only thing this does is to map intermediate compilation symbols to syntax/transformations/original code model.
        private const string _introducedSyntaxAnnotationId = "AspectLinker_IntroducedSyntax";

        private readonly CompilationModel _compilationModel;
        private readonly Dictionary<IMemberIntroduction, IReadOnlyList<IntroducedMember>> _introducedMembers;
        private readonly Dictionary<IntroducedMember, (SyntaxTree OriginalSyntaxTree, int AnnotationId, MemberDeclarationSyntax AnnotatedSyntax)> _introducedMemberToMarkId;
        private readonly Dictionary<int, (SyntaxTree OriginalSyntaxTree, IntroducedMember IntroducedMember, MemberDeclarationSyntax AnnotatedSyntax)> _introducedMarkIdToMember;
        private readonly Dictionary<ISymbol, ICodeElement> _overrideTargetsByOriginalSymbolName;
        private readonly Dictionary<ICodeElement, List<IntroducedMember>> _overrideMap;
        private readonly Dictionary<SyntaxTree, SyntaxTree> _introducedTreeMap;

        private bool _frozen;
        private int _nextAnnotationId;
        private CSharpCompilation? _intermediateCompilation;

        public LinkerTransformationRegistry( CompilationModel compilationModel )
        {
            this._compilationModel = compilationModel;
            this._introducedMembers = new Dictionary<IMemberIntroduction, IReadOnlyList<IntroducedMember>>();
            this._introducedMemberToMarkId = new Dictionary<IntroducedMember, (SyntaxTree, int, MemberDeclarationSyntax)>();
            this._introducedMarkIdToMember = new Dictionary<int, (SyntaxTree, IntroducedMember, MemberDeclarationSyntax)>();
            this._introducedTreeMap = new Dictionary<SyntaxTree, SyntaxTree>();
            this._overrideMap = new Dictionary<ICodeElement, List<IntroducedMember>>();
            this._overrideTargetsByOriginalSymbolName = new Dictionary<ISymbol, ICodeElement>( StructuralSymbolComparer.Instance );
        }

        public void SetIntroducedMembers( IMemberIntroduction memberIntroduction, IEnumerable<IntroducedMember> introducedMembers )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._introducedMembers.Add( memberIntroduction, introducedMembers.ToList() );

            foreach ( var introducedMember in introducedMembers )
            {
                var annotationId = this._nextAnnotationId++;
                var annotatedSyntax = introducedMember.Syntax.WithAdditionalAnnotations( new SyntaxAnnotation( _introducedSyntaxAnnotationId, annotationId.ToString() ) );

                /* This was for debugging, if we want to have it in intermediate compilations, we would need to remove it afterwards.
#if DEBUG
                
                // Add trivia with the introduced syntax id.
                annotatedSyntax = annotatedSyntax.WithLeadingTrivia(
                    annotatedSyntax.HasLeadingTrivia
                    ? new[] { Whitespace( "\n" ), Comment($"// Introduction (ID:{annotationId})"), Whitespace("\n") }.Concat(annotatedSyntax.GetLeadingTrivia())
                    : new[] { Whitespace( "\n" ), Comment( $"// Introduction (ID:{annotationId})"), Whitespace( "\n" ) }
                    );
#endif
                */

                if ( memberIntroduction is IOverriddenElement overrideTransformation )
                {
                    if ( !this._overrideMap.TryGetValue( overrideTransformation.OverriddenElement, out var overrideList ) )
                    {
                        this._overrideMap[overrideTransformation.OverriddenElement] = overrideList = new List<IntroducedMember>();
                    }

                    overrideList.Add( introducedMember );

                    if ( overrideTransformation.OverriddenElement is CodeElement codeElement )
                    {
                        this._overrideTargetsByOriginalSymbolName[codeElement.Symbol] = codeElement;
                    }
                }

                this._introducedMemberToMarkId.Add( introducedMember, (memberIntroduction.TargetSyntaxTree, annotationId, annotatedSyntax) );
                this._introducedMarkIdToMember.Add( annotationId, (memberIntroduction.TargetSyntaxTree, introducedMember, annotatedSyntax) );
            }
        }

        internal IEnumerable<IntroducedMember> GetIntroducedMembers()
        {
            return this._introducedMembers.Values.SelectMany( x => x );
        }

        public void SetIntermediateSyntaxTreeMapping( SyntaxTree originalTree, SyntaxTree intermediateTree )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._introducedTreeMap.Add( originalTree, intermediateTree );
        }

        public void SetIntermediateCompilation( CSharpCompilation intermediateCompilation )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._intermediateCompilation = intermediateCompilation;
        }

        public void Freeze()
        {
            this._frozen = true;
        }

        public IEnumerable<MemberDeclarationSyntax> GetIntroducedSyntaxNodesOnPosition( MemberDeclarationSyntax position )
        {
            // TODO: Optimize.
            return
                this._introducedMembers
                .SelectMany( kvp =>
                    from im in kvp.Value
                    let imr = this._introducedMemberToMarkId[im]
                    select (kvp.Key.InsertPositionNode, IntroducedMember: im, AnnotatedSyntax: imr.AnnotatedSyntax) )
                .Where( p => p.InsertPositionNode == position )
                .Select( p => p.AnnotatedSyntax );
        }

        public ISymbol GetSymbolForIntroducedMember( IntroducedMember introducedMember )
        {
            var introducedMemberRecord = this._introducedMemberToMarkId[introducedMember];
            var intermediateSyntaxTree = this._introducedTreeMap[introducedMemberRecord.OriginalSyntaxTree];

            // TODO: Precompute, it's really really slow (visits the whole tree).
            var intermediateSyntax =
                intermediateSyntaxTree.GetRoot()
                .GetAnnotatedNodes( _introducedSyntaxAnnotationId )
                .Where( x => int.Parse( x.GetAnnotations( _introducedSyntaxAnnotationId ).Single().Data ) == introducedMemberRecord.AnnotationId )
                .Single();

            return this._intermediateCompilation.AssertNotNull().GetSemanticModel( intermediateSyntaxTree ).GetDeclaredSymbol( intermediateSyntax ).AssertNotNull();
        }

        public IReadOnlyList<IntroducedMember> GetMethodOverridesForSymbol( IMethodSymbol symbol )
        {
            // TODO: Optimize.
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            var annotation = declaringSyntax.GetAnnotations( _introducedSyntaxAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                // Original code declaration - we should be able to get ICodeElement by symbol name.

                if ( !this._overrideTargetsByOriginalSymbolName.TryGetValue( symbol, out var originalElement ) )
                {
                    return Array.Empty<IntroducedMember>();
                }

                return this._overrideMap[originalElement];
            }
            else
            {
                // Introduced declaration - we should get ICodeElement from introduced member.
                var id = int.Parse( annotation.Data );
                var memberRecord = this._introducedMarkIdToMember[id];

                if ( memberRecord.IntroducedMember.Introductor is ICodeElement introducedElement )
                {
                    if ( this._overrideMap.TryGetValue( introducedElement, out var overrides ) )
                    {
                        return overrides;
                    }
                    else
                    {
                        return Array.Empty<IntroducedMember>();
                    }
                }
                else
                {
                    return Array.Empty<IntroducedMember>();
                }
            }
        }

        public IReadOnlyList<IMethodSymbol> GetOverriddenMethods()
        {
            // TODO: This is not efficient.
            var overriddenMethods = new List<IMethodSymbol>();
            foreach ( var intermediateSyntaxTree in this._intermediateCompilation.AssertNotNull().SyntaxTrees)
            {
                var semanticModel = this._intermediateCompilation.AssertNotNull().GetSemanticModel( intermediateSyntaxTree );

                foreach (var methodDeclaration in intermediateSyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol( methodDeclaration );

                    if (methodSymbol != null && this._overrideTargetsByOriginalSymbolName.ContainsKey(methodSymbol ))
                    {
                        overriddenMethods.Add( methodSymbol );
                    }
                }
            }

            return overriddenMethods;
        }

        public IntroducedMember? GetIntroducedMemberForSymbol( ISymbol symbol )
        {
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            var annotation = declaringSyntax.GetAnnotations( _introducedSyntaxAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                return null;
            }

            var id = int.Parse( annotation.Data );
            var memberRecord = this._introducedMarkIdToMember[id];
            return memberRecord.IntroducedMember;
        }
    }
}
