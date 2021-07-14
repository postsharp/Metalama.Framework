// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
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
        private readonly Dictionary<IDeclaration, List<LinkerIntroducedMember>> _overrideMap;
        private readonly Dictionary<LinkerIntroducedMember, IDeclaration> _overrideTargetMap;
        private readonly Dictionary<ISymbol, IDeclaration> _overrideTargetsByOriginalSymbolName;
        private readonly Dictionary<SyntaxTree, SyntaxTree> _introducedTreeMap;
        private readonly Dictionary<IDeclaration, LinkerIntroducedMember> _builderLookup;

        public LinkerIntroductionRegistry(
            CompilationModel finalCompilationModel,
            Compilation intermediateCompilation,
            Dictionary<SyntaxTree, SyntaxTree> introducedTreeMap,
            IReadOnlyList<LinkerIntroducedMember> introducedMembers )
        {
            this._intermediateCompilation = intermediateCompilation;
            this._introducedMemberLookup = introducedMembers.ToDictionary( x => x.LinkerNodeId, x => x );
            this._introducedTreeMap = introducedTreeMap;
            this._overrideMap = new Dictionary<IDeclaration, List<LinkerIntroducedMember>>( finalCompilationModel.InvariantComparer );
            this._overrideTargetMap = new Dictionary<LinkerIntroducedMember, IDeclaration>();
            this._overrideTargetsByOriginalSymbolName = new Dictionary<ISymbol, IDeclaration>( StructuralSymbolComparer.Default );
            this._builderLookup = new Dictionary<IDeclaration, LinkerIntroducedMember>();

            foreach ( var introducedMember in introducedMembers )
            {
                if ( introducedMember.Introduction is IOverriddenDeclaration overrideTransformation )
                {
                    if ( !this._overrideMap.TryGetValue( overrideTransformation.OverriddenDeclaration, out var overrideList ) )
                    {
                        this._overrideMap[overrideTransformation.OverriddenDeclaration] = overrideList = new List<LinkerIntroducedMember>();
                    }

                    this._overrideTargetMap[introducedMember] = overrideTransformation.OverriddenDeclaration;
                    overrideList.Add( introducedMember );

                    if ( overrideTransformation.OverriddenDeclaration is Declaration declaration )
                    {
                        this._overrideTargetsByOriginalSymbolName[declaration.Symbol] = declaration;
                    }
                }

                if ( introducedMember.Introduction is MemberBuilder builder )
                {
                    this._builderLookup[builder] = introducedMember;
                }
            }
        }

        /// <summary>
        /// Gets introduced members representing overrides of a symbol.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns>List of introduced members.</returns>
        public IReadOnlyList<LinkerIntroducedMember> GetOverridesForSymbol( ISymbol symbol )
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

                if ( introducedMember.Introduction is IDeclaration introducedElement )
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

        public ISymbol? GetOverrideTarget( LinkerIntroducedMember overrideIntroducedMember )
        {
            if ( overrideIntroducedMember == null )
            {
                return null;
            }

            if ( !this._overrideTargetMap.TryGetValue( overrideIntroducedMember, out var overrideTarget ) )
            {
                return null;
            }

            if ( overrideTarget is Declaration originalDeclaration )
            {
                return originalDeclaration.GetSymbol();
            }
            else if ( overrideTarget is MemberBuilder builder )
            {
                return GetFromBuilder( builder );
            }
            else if ( overrideTarget is BuiltMember builtMember )
            {
                return GetFromBuilder( builtMember.Builder );
            }
            else
            {
                throw new AssertionFailedException();
            }

            ISymbol? GetFromBuilder( DeclarationBuilder builder )
            {
                var introducedBuilder = this._builderLookup[builder];
                var intermediateSyntaxTree = this._introducedTreeMap[((ISyntaxTreeTransformation) builder).TargetSyntaxTree];
                var intermediateNode = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedBuilder.Syntax );
                var intermediateSemanticModel = this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree );

                return intermediateSemanticModel.GetDeclaredSymbol( intermediateNode );
            }
        }

        /// <summary>
        /// Gets an introduced member represented the declaration that resulted in the specified symbol in the intermediate compilation.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns>An introduced member, or <c>null</c> if the declaration represented by this symbol was not introduced.</returns>
        public LinkerIntroducedMember? GetIntroducedMemberForSymbol( ISymbol symbol )
        {
            var declaringSyntax = symbol.GetPrimaryDeclaration().AssertNotNull();

            if ( declaringSyntax == null )
            {
                return null;
            }

            if ( symbol is IEventSymbol && declaringSyntax is VariableDeclaratorSyntax )
            {
                // TODO: Move this to special method, we are going to need the same thing for fields.
                declaringSyntax = declaringSyntax.Parent?.Parent.AssertNotNull();
            }

            var annotation = declaringSyntax.AssertNotNull().GetAnnotations( IntroducedNodeIdAnnotationId ).SingleOrDefault();

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

            SyntaxNode symbolSyntax;

            if ( intermediateSyntax is EventFieldDeclarationSyntax eventFieldSyntax )
            {
                symbolSyntax = eventFieldSyntax.Declaration.Variables.First();
            }
            else
            {
                symbolSyntax = intermediateSyntax;
            }

            return this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree ).GetDeclaredSymbol( symbolSyntax ).AssertNotNull();
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
        public IEnumerable<ISymbol> GetOverriddenMembers()
        {
            // TODO: This is not efficient.
            var overriddenMembers = new List<ISymbol>();

            foreach ( var intermediateSyntaxTree in this._intermediateCompilation.SyntaxTrees )
            {
                var semanticModel = this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree );

                foreach ( var methodDeclaration in intermediateSyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>() )
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol( methodDeclaration );

                    if ( methodSymbol != null && this._overrideTargetsByOriginalSymbolName.ContainsKey( methodSymbol ) )
                    {
                        overriddenMembers.Add( methodSymbol );
                    }
                }

                foreach ( var propertyDeclaration in intermediateSyntaxTree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>() )
                {
                    var propertySymbol = semanticModel.GetDeclaredSymbol( propertyDeclaration );

                    if ( propertySymbol != null && this._overrideTargetsByOriginalSymbolName.ContainsKey( propertySymbol ) )
                    {
                        overriddenMembers.Add( propertySymbol );
                    }
                }

                foreach ( var eventDeclaration in intermediateSyntaxTree.GetRoot().DescendantNodes().OfType<EventDeclarationSyntax>() )
                {
                    var eventSymbol = semanticModel.GetDeclaredSymbol( eventDeclaration );

                    if ( eventSymbol != null && this._overrideTargetsByOriginalSymbolName.ContainsKey( eventSymbol ) )
                    {
                        overriddenMembers.Add( eventSymbol );
                    }
                }
            }

            return overriddenMembers;
        }
    }
}