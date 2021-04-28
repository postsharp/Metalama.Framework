// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Stores information about introductions.
    /// </summary>
    internal class LinkerIntroductionRegistry
    {
        public const string IntroducedNodeIdAnnotationId = "AspectLinker_IntroducedNodeId";

        private readonly Compilation _intermediateCompilation;
        private readonly Dictionary<string, LinkerIntroducedMember> _introducedMemberLookup;
        private readonly Dictionary<ICodeElement, List<LinkerIntroducedMember>> _overrideMap;
        private readonly Dictionary<ISymbol, ICodeElement> _overrideTargetsByOriginalSymbolName;
        private readonly Dictionary<SyntaxTree, SyntaxTree> _introducedTreeMap;

        public LinkerIntroductionRegistry(
            CompilationModel finalCompilationModel,
            Compilation intermediateCompilation,
            Dictionary<SyntaxTree, SyntaxTree> introducedTreeMap,
            IReadOnlyList<LinkerIntroducedMember> introducedMembers )
        {
            this._intermediateCompilation = intermediateCompilation;
            this._introducedMemberLookup = introducedMembers.ToDictionary( x => x.LinkerNodeId, x => x );
            this._introducedTreeMap = introducedTreeMap;
            this._overrideMap = new Dictionary<ICodeElement, List<LinkerIntroducedMember>>( finalCompilationModel.InvariantComparer );
            this._overrideTargetsByOriginalSymbolName = new Dictionary<ISymbol, ICodeElement>( StructuralSymbolComparer.Instance );

            foreach ( var introducedMember in introducedMembers )
            {
                if ( introducedMember.Introduction is IOverriddenElement overrideTransformation )
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

        /// <summary>
        /// Gets introduced members representing overrides of a symbol.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns>List of introduced members.</returns>
        public IReadOnlyList<LinkerIntroducedMember> GetOverridesForSymbol( IMethodSymbol symbol )
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

                if ( introducedMember.Introduction is ICodeElement introducedElement )
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

        /// <summary>
        /// Gets an introduced member represented the declaration that resulted in the specified symbol in the intermediate compilation.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns>An introduced member, or <c>null</c> if the declaration represented by this symbol was not introduced.</returns>
        public LinkerIntroducedMember? GetIntroducedMemberForSymbol( IMethodSymbol symbol )
        {
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            var annotation = declaringSyntax.GetAnnotations( IntroducedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                return null;
            }

            return this._introducedMemberLookup[annotation.Data.AssertNotNull()];
        }

        /// <summary>
        /// Gets a symbol in intermediate compilation that represents a declaration introduced by the introduced member.
        /// </summary>
        /// <param name="introducedMember"></param>
        /// <returns></returns>
        public ISymbol GetSymbolForIntroducedMember( LinkerIntroducedMember introducedMember )
        {
            var intermediateSyntaxTree = this._introducedTreeMap[introducedMember.Introduction.TargetSyntaxTree];
            var intermediateSyntax = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedMember.Syntax );

            return this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree ).GetDeclaredSymbol( intermediateSyntax ).AssertNotNull();
        }

        /// <summary>
        /// Gets introduced members for all transformations.
        /// </summary>
        /// <returns>Enumeration of introduced members.</returns>
        public IEnumerable<LinkerIntroducedMember> GetIntroducedMembers()
        {
            return this._introducedMemberLookup.Values;
        }

        /// <summary>
        /// Gets all symbols for overridden members.
        /// </summary>
        /// <returns>Enumeration of symbols.</returns>
        public IEnumerable<IMethodSymbol> GetOverriddenMembers()
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