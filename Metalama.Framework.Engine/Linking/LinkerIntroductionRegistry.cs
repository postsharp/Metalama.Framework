// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Stores information about introductions and intermediate compilation.
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

                if ( introducedMember.Introduction is IDeclarationBuilder builder )
                {
                    this._builderLookup[builder] = introducedMember;
                }
            }
        }

        /// <summary>
        /// Gets introduced members representing overrides of a symbol.
        /// </summary>
        /// <param name="referencedSymbol">Symbol.</param>
        /// <returns>List of introduced members.</returns>
        public IReadOnlyList<LinkerIntroducedMember> GetOverridesForSymbol( ISymbol referencedSymbol )
        {
            // TODO: Optimize.
            var declaringSyntax = referencedSymbol.GetPrimaryDeclaration();

            if ( declaringSyntax == null )
            {
                // Code is outside of the current compilation, so it cannot have overrides.
                // TODO: This should be checked more thoroughly.
                return Array.Empty<LinkerIntroducedMember>();
            }

            var memberDeclaration = GetMemberDeclaration( declaringSyntax );

            var annotation = memberDeclaration.GetAnnotations( IntroducedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                // Original code declaration - we should be able to get ICodeElement by symbol name.

                if ( !this._overrideTargetsByOriginalSymbolName.TryGetValue( referencedSymbol, out var originalElement ) )
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

        private static SyntaxNode GetMemberDeclaration( SyntaxNode declaringSyntax )
        {
            return declaringSyntax switch
            {
                VariableDeclaratorSyntax { Parent: { Parent: MemberDeclarationSyntax memberDeclaration } } => memberDeclaration,
                MemberDeclarationSyntax memberDeclaration => memberDeclaration,
                ParameterSyntax { Parent: { Parent: RecordDeclarationSyntax } } => declaringSyntax,
                _ => throw new AssertionFailedException()
            };
        }

        public ISymbol? GetOverrideTarget( LinkerIntroducedMember overrideIntroducedMember )
        {
            if ( !this._overrideTargetMap.TryGetValue( overrideIntroducedMember, out var overrideTarget ) )
            {
                // Coverage: ignore (coverage is irrelevant, needed for correctness)
                return null;
            }

            if ( overrideTarget is Declaration originalDeclaration )
            {
                return originalDeclaration.GetSymbol();
            }
            else if ( overrideTarget is IDeclarationBuilder builder )
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

            ISymbol? GetFromBuilder( IDeclarationBuilder builder )
            {
                var introducedBuilder = this._builderLookup[builder];
                var sourceSyntaxTree = ((IIntroduceMemberTransformation) builder).TransformedSyntaxTree.AssertNotNull();
                var intermediateSyntaxTree = this._introducedTreeMap[sourceSyntaxTree];
                var intermediateNode = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedBuilder.Syntax );
                var intermediateSemanticModel = this._intermediateCompilation.GetSemanticModel( intermediateSyntaxTree );

                var symbolNode = intermediateNode.AssertNotNull() switch
                {
                    EventFieldDeclarationSyntax eventFieldNode => (SyntaxNode) eventFieldNode.Declaration.Variables.First(),
                    _ => intermediateNode!
                };

                return intermediateSemanticModel.GetDeclaredSymbol( symbolNode );
            }
        }

        /// <summary>
        /// Gets an introduced member represented the declaration that resulted in the specified symbol in the intermediate compilation.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns>An introduced member, or <c>null</c> if the declaration represented by this symbol was not introduced.</returns>
        public LinkerIntroducedMember? GetIntroducedMemberForSymbol( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } propertyAccessorSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return this.GetIntroducedMemberForSymbol( propertyAccessorSymbol.AssociatedSymbol.AssertNotNull() );

                case IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove } eventAccessorSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return this.GetIntroducedMemberForSymbol( eventAccessorSymbol.AssociatedSymbol.AssertNotNull() );
            }

            var declaringSyntax = symbol.GetPrimaryDeclaration();

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
            var intermediateSyntaxTree = this._introducedTreeMap[introducedMember.Introduction.TransformedSyntaxTree];
            var intermediateSyntax = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedMember.Syntax ).AssertNotNull();

            SyntaxNode symbolSyntax = intermediateSyntax switch
            {
                EventFieldDeclarationSyntax eventFieldSyntax => eventFieldSyntax.Declaration.Variables.First(),
                FieldDeclarationSyntax fieldSyntax => fieldSyntax.Declaration.Variables.First(),
                _ => intermediateSyntax
            };

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
            var returned = new HashSet<ISymbol>( SymbolEqualityComparer.Default );

            foreach ( var introducedMember in this.GetIntroducedMembers() )
            {
                var symbol = this.GetSymbolForIntroducedMember( introducedMember );

                if ( this.IsOverride( symbol ) )
                {
                    if ( returned.Add( symbol ) )
                    {
                        yield return this.GetOverrideTarget( symbol ).AssertNotNull();
                    }
                }
            }
        }

        public ISymbol? GetOverrideTarget( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet } getterSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return ((IPropertySymbol?) this.GetOverrideTarget( getterSymbol.AssociatedSymbol.AssertNotNull() ))?.GetMethod;

                case IMethodSymbol { MethodKind: MethodKind.PropertySet } setterSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return ((IPropertySymbol?) this.GetOverrideTarget( setterSymbol.AssociatedSymbol.AssertNotNull() ))?.SetMethod;

                case IMethodSymbol { MethodKind: MethodKind.EventAdd } adderSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return ((IEventSymbol?) this.GetOverrideTarget( adderSymbol.AssociatedSymbol.AssertNotNull() ))?.AddMethod;

                case IMethodSymbol { MethodKind: MethodKind.EventRemove } removerSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return ((IEventSymbol?) this.GetOverrideTarget( removerSymbol.AssociatedSymbol.AssertNotNull() ))?.RemoveMethod;

                default:
                    var introducedMember = this.GetIntroducedMemberForSymbol( symbol );

                    if ( introducedMember == null )
                    {
                        return null;
                    }

                    return this.GetOverrideTarget( introducedMember );
            }
        }

        /// <summary>
        /// Gets the last (outermost) override of the method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns>Symbol.</returns>
        public ISymbol GetLastOverride( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet } getterSymbol:
                    return ((IPropertySymbol) this.GetLastOverride( getterSymbol.AssociatedSymbol.AssertNotNull() )).GetMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.PropertySet } setterSymbol:
                    return ((IPropertySymbol) this.GetLastOverride( setterSymbol.AssociatedSymbol.AssertNotNull() )).SetMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.EventAdd } adderSymbol:
                    return ((IEventSymbol) this.GetLastOverride( adderSymbol.AssociatedSymbol.AssertNotNull() )).AddMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.EventRemove } removerSymbol:
                    return ((IEventSymbol) this.GetLastOverride( removerSymbol.AssociatedSymbol.AssertNotNull() )).RemoveMethod.AssertNotNull();

                default:
                    var lastOverride = this.GetOverridesForSymbol( symbol ).LastOrDefault();

                    if ( lastOverride == null )
                    {
                        // Coverage: ignore (coverage is irrelevant, needed for correctness).
                        return symbol;
                    }

                    return this.GetSymbolForIntroducedMember( lastOverride );
            }
        }

        /// <summary>
        /// Determines whether the symbol represents override target.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns><c>True</c> if the method is override target, otherwise <c>false</c>.</returns>
        public bool IsOverrideTarget( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol methodSymbol
                 && (methodSymbol.MethodKind == MethodKind.PropertyGet
                     || methodSymbol.MethodKind == MethodKind.PropertySet
                     || methodSymbol.MethodKind == MethodKind.EventAdd
                     || methodSymbol.MethodKind == MethodKind.EventRemove) )
            {
                return this.IsOverrideTarget( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            return this.GetOverridesForSymbol( symbol ).Count > 0;
        }

        /// <summary>
        /// Determines whether the symbol represents an override method.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public bool IsOverride( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol methodSymbol
                 && (methodSymbol.MethodKind == MethodKind.PropertyGet
                     || methodSymbol.MethodKind == MethodKind.PropertySet
                     || methodSymbol.MethodKind == MethodKind.EventAdd
                     || methodSymbol.MethodKind == MethodKind.EventRemove) )
            {
                return this.IsOverride( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            var introducedMember = this.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Semantic == IntroducedMemberSemantic.Override;
        }

        public bool IsLastOverride( ISymbol symbol )
        {
            return this.IsOverride( symbol ) && SymbolEqualityComparer.Default.Equals(
                symbol,
                this.GetLastOverride( this.GetOverrideTarget( symbol ).AssertNotNull() ) );
        }

        // Not yet used.
        [ExcludeFromCodeCoverage]
        public ISymbol? GetPreviousOverride( ISymbol symbol )
        {
            var overrideTarget = this.GetOverrideTarget( symbol );

            if ( overrideTarget != null )
            {
                var overrides = this.GetOverridesForSymbol( overrideTarget );
                var matched = false;

                foreach ( var introducedMember in overrides.Reverse() )
                {
                    var overrideSymbol = this.GetSymbolForIntroducedMember( introducedMember );

                    if ( matched )
                    {
                        return overrideSymbol;
                    }

                    if ( SymbolEqualityComparer.Default.Equals( overrideSymbol, symbol ) )
                    {
                        matched = true;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}