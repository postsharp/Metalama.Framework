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
    /// Stores information about introductions.
    /// </summary>
    internal partial class LinkerIntroductionRegistry
    {
        public const string IntroducedNodeIdAnnotationId = "AspectLinker_IntroducedNodeId";

        private readonly CSharpCompilation _intermediateCompilation;
        private readonly Dictionary<string, LinkerIntroducedMember> _introducedMemberLookup;
        private readonly Dictionary<ICodeElement, List<LinkerIntroducedMember>> _overrideMap;
        private readonly Dictionary<ISymbol, ICodeElement> _overrideTargetsByOriginalSymbolName;
        private readonly Dictionary<SyntaxTree, SyntaxTree> _introducedTreeMap;

        public LinkerIntroductionRegistry( CSharpCompilation intermediateCompilation, Dictionary<SyntaxTree, SyntaxTree> introducedTreeMap, IEnumerable<LinkerIntroducedMember> introducedMembers )
        {
            this._intermediateCompilation = intermediateCompilation;
            this._introducedMemberLookup = introducedMembers.ToDictionary( x => x.LinkerNodeId, x => x );
            this._introducedTreeMap = introducedTreeMap;
            this._overrideMap = new Dictionary<ICodeElement, List<LinkerIntroducedMember>>();
            this._overrideTargetsByOriginalSymbolName = new Dictionary<ISymbol, ICodeElement>( StructuralSymbolComparer.Instance );

            foreach ( var introducedMember in introducedMembers )
            {
                if ( introducedMember.Introductor is IOverriddenElement overrideTransformation )
                {
                    if ( !this._overrideMap.TryGetValue( overrideTransformation.OverriddenElement, out var overrideList ) )
                    {
                        this._overrideMap[overrideTransformation.OverriddenElement] = overrideList = new List<LinkerIntroducedMember>();
                    }

                    overrideList.Add( introducedMember );

                    if ( overrideTransformation.OverriddenElement is CodeElement codeElement )
                    {
                        this._overrideTargetsByOriginalSymbolName[codeElement.Symbol] = codeElement;
                    }
                }
            }
        }

        public IReadOnlyList<LinkerIntroducedMember> GetMethodOverridesForSymbol( IMethodSymbol symbol )
        {
            // TODO: Optimize.
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            var annotation = declaringSyntax.GetAnnotations( IntroducedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                // Original code declaration - we should be able to get ICodeElement by symbol name.

                if ( !this._overrideTargetsByOriginalSymbolName.TryGetValue( symbol, out var originalElement ) )
                {
                    return Array.Empty<LinkerIntroducedMember>();
                }

                return this._overrideMap[originalElement];
            }
            else
            {
                // Introduced declaration - we should get ICodeElement from introduced member.
                var introducedMember = this._introducedMemberLookup[annotation.Data.AssertNotNull()];

                if ( introducedMember.Introductor is ICodeElement introducedElement )
                {
                    if ( this._overrideMap.TryGetValue( introducedElement, out var overrides ) )
                    {
                        return overrides;
                    }
                    else
                    {
                        return Array.Empty<LinkerIntroducedMember>();
                    }
                }
                else
                {
                    return Array.Empty<LinkerIntroducedMember>();
                }
            }
        }

        internal LinkerIntroducedMember? GetIntroducedMemberForSymbol( IMethodSymbol symbol )
        {
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            var annotation = declaringSyntax.GetAnnotations( IntroducedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                return null;
            }

            return this._introducedMemberLookup[annotation.Data.AssertNotNull()];
        }

        internal ISymbol GetSymbolForIntroducedMember( LinkerIntroducedMember introducedMember )
        {
            var intermediateSyntaxTree = this._introducedTreeMap[introducedMember.Introductor.TargetSyntaxTree];
            var intermediateSyntax = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedMember.Syntax);

            return this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree ).GetDeclaredSymbol( intermediateSyntax ).AssertNotNull();
        }

        public IEnumerable<LinkerIntroducedMember> GetIntroducedMembers()
        {
            return this._introducedMemberLookup.Values;
        }

        public IEnumerable<IMethodSymbol> GetOverriddenMethods()
        {           
            // TODO: This is not efficient.
            var overriddenMethods = new List<IMethodSymbol>();
            foreach ( var intermediateSyntaxTree in this._intermediateCompilation.SyntaxTrees )
            {
                var semanticModel = this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree );

                foreach ( var methodDeclaration in intermediateSyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>() )
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol( methodDeclaration );

                    if ( methodSymbol != null && this._overrideTargetsByOriginalSymbolName.ContainsKey( methodSymbol ) )
                    {
                        overriddenMethods.Add( methodSymbol );
                    }
                }
            }

            return overriddenMethods;
        }
    }
}
