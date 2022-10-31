﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Stores information about injections and intermediate compilation.
    /// </summary>
    internal class LinkerInjectionRegistry
    {
        public const string InjectedNodeIdAnnotationId = "AspectLinker_InjectedNodeId";

        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly Compilation _intermediateCompilation;
        private readonly SemanticModelProvider _semanticModelProvider;
        private readonly IReadOnlyDictionary<string, LinkerInjectedMember> _injectedMemberLookup;
        private readonly IReadOnlyDictionary<IDeclaration, UnsortedConcurrentLinkedList<LinkerInjectedMember>> _overrideMap;
        private readonly IReadOnlyDictionary<LinkerInjectedMember, IDeclaration> _overrideTargetMap;
        private readonly IReadOnlyDictionary<ISymbol, IDeclaration> _overrideTargetsByOriginalSymbol;
        private readonly IReadOnlyDictionary<IDeclaration, LinkerInjectedMember> _builderLookup;
        private readonly IReadOnlyDictionary<SyntaxTree, SyntaxTree> _transformedSyntaxTreeMap;

        public LinkerInjectionRegistry(
            TransformationLinkerOrderComparer comparer,
            CompilationModel finalCompilationModel,
            Compilation intermediateCompilation,
            IReadOnlyDictionary<SyntaxTree, SyntaxTree> transformedSyntaxTreeMap,
            IReadOnlyCollection<LinkerInjectedMember> injectedMembers )
        {
            Dictionary<IDeclaration, UnsortedConcurrentLinkedList<LinkerInjectedMember>> overrideMap;
            Dictionary<LinkerInjectedMember, IDeclaration> overrideTargetMap;
            Dictionary<ISymbol, IDeclaration> overrideTargetsByOriginalSymbol;
            Dictionary<IDeclaration, LinkerInjectedMember> builderLookup;

            this._comparer = comparer;
            this._intermediateCompilation = intermediateCompilation;
            this._semanticModelProvider = intermediateCompilation.GetSemanticModelProvider();
            this._injectedMemberLookup = injectedMembers.ToDictionary( x => x.LinkerNodeId, x => x );
            this._transformedSyntaxTreeMap = transformedSyntaxTreeMap;

            this._overrideMap = overrideMap =
                new Dictionary<IDeclaration, UnsortedConcurrentLinkedList<LinkerInjectedMember>>( finalCompilationModel.Comparers.Default );

            this._overrideTargetMap = overrideTargetMap = new Dictionary<LinkerInjectedMember, IDeclaration>();
            this._overrideTargetsByOriginalSymbol = overrideTargetsByOriginalSymbol = new Dictionary<ISymbol, IDeclaration>( StructuralSymbolComparer.Default );
            this._builderLookup = builderLookup = new Dictionary<IDeclaration, LinkerInjectedMember>( ReferenceEqualityComparer<IDeclaration>.Instance );

            // TODO: This could be parallelized. The collections could be built in the LinkerInjectionStep, it is in
            // the same spirit as the Index* methods.

            foreach ( var injectedMember in injectedMembers )
            {
                if ( injectedMember.Transformation is IOverrideDeclarationTransformation overrideTransformation )
                {
                    if ( !overrideMap.TryGetValue( overrideTransformation.OverriddenDeclaration, out var overrideList ) )
                    {
                        overrideMap[overrideTransformation.OverriddenDeclaration] = overrideList = new UnsortedConcurrentLinkedList<LinkerInjectedMember>();
                    }

                    overrideTargetMap[injectedMember] = overrideTransformation.OverriddenDeclaration;
                    overrideList.Add( injectedMember );

                    if ( overrideTransformation.OverriddenDeclaration is Declaration declaration )
                    {
                        overrideTargetsByOriginalSymbol[declaration.Symbol] = declaration;
                    }
                }

                if ( injectedMember.Transformation is IIntroduceDeclarationTransformation introduceTransformation )
                {
                    builderLookup[introduceTransformation.DeclarationBuilder] = injectedMember;
                }
            }
        }

        /// <summary>
        /// Gets introduced members representing overrides of a symbol.
        /// </summary>
        /// <param name="referencedSymbol">Symbol.</param>
        /// <returns>List of introduced members.</returns>
        public IReadOnlyList<LinkerInjectedMember> GetOverridesForSymbol( ISymbol referencedSymbol )
        {
            IReadOnlyList<LinkerInjectedMember> Sort( UnsortedConcurrentLinkedList<LinkerInjectedMember> list )
                => list.GetSortedItems( ( x, y ) => this._comparer.Compare( x.Transformation, y.Transformation ) );

            // TODO: Optimize.
            var declaringSyntax = referencedSymbol.GetPrimaryDeclaration();

            if ( declaringSyntax == null )
            {
                // Code is outside of the current compilation, so it cannot have overrides.
                // TODO: This should be checked more thoroughly.
                return Array.Empty<LinkerInjectedMember>();
            }

            var memberDeclaration = GetMemberDeclaration( declaringSyntax );

            var annotation = memberDeclaration.GetAnnotations( InjectedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                // Original code declaration - we should be able to get ICodeElement by symbol name.

                if ( !this._overrideTargetsByOriginalSymbol.TryGetValue( referencedSymbol, out var originalElement ) )
                {
                    return Array.Empty<LinkerInjectedMember>();
                }

                return Sort( this._overrideMap[originalElement] );
            }
            else
            {
                // Introduced declaration - we should get ICodeElement from introduced member.
                var injectedMember = this._injectedMemberLookup[annotation.Data.AssertNotNull()];

                if ( injectedMember.Transformation is IIntroduceDeclarationTransformation introductionTransformation )
                {
                    if ( this._overrideMap.TryGetValue( introductionTransformation.DeclarationBuilder, out var overrides ) )
                    {
                        return Sort( overrides );
                    }
                    else
                    {
                        return Array.Empty<LinkerInjectedMember>();
                    }
                }
                else
                {
                    return Array.Empty<LinkerInjectedMember>();
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
                _ => throw new AssertionFailedException( $"Unexpected node of kind {declaringSyntax.Kind()} at '{declaringSyntax.GetLocation()}'." )
            };
        }

        public ISymbol? GetOverrideTarget( LinkerInjectedMember overrideInjectedMember )
        {
            if ( !this._overrideTargetMap.TryGetValue( overrideInjectedMember, out var overrideTarget ) )
            {
                // Coverage: ignore (coverage is irrelevant, needed for correctness)
                return null;
            }

            if ( overrideTarget is Declaration originalDeclaration )
            {
                return SymbolTranslator.GetInstance( this._intermediateCompilation ).Translate( originalDeclaration.GetSymbol().AssertNotNull() );
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
                throw new AssertionFailedException( $"Unexpected declaration: '{overrideTarget}'." );
            }

            ISymbol? GetFromBuilder( IDeclarationBuilder builder )
            {
                var introducedBuilder = this._builderLookup[builder];
                var sourceSyntaxTree = ((IDeclarationImpl) builder).PrimarySyntaxTree.AssertNotNull();
                var intermediateSyntaxTree = this._transformedSyntaxTreeMap[sourceSyntaxTree];
                var intermediateNode = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedBuilder.Syntax );
                var intermediateSemanticModel = this._semanticModelProvider.GetSemanticModel( intermediateSyntaxTree );

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
        public LinkerInjectedMember? GetInjectedMemberForSymbol( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } propertyAccessorSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return this.GetInjectedMemberForSymbol( propertyAccessorSymbol.AssociatedSymbol.AssertNotNull() );

                case IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove } eventAccessorSymbol:
                    // Coverage: ignore (coverage is irrelevant, needed for correctness).
                    return this.GetInjectedMemberForSymbol( eventAccessorSymbol.AssociatedSymbol.AssertNotNull() );
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

            var annotation = declaringSyntax.AssertNotNull().GetAnnotations( InjectedNodeIdAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                return null;
            }

            return this._injectedMemberLookup[annotation.Data.AssertNotNull()];
        }

        /// <summary>
        /// Gets a symbol in intermediate compilation that represents a declaration introduced by the introduced member.
        /// </summary>
        /// <param name="injectedMember"></param>
        /// <returns></returns>
        public ISymbol GetSymbolForInjectedMember( LinkerInjectedMember injectedMember )
        {
            var intermediateSyntaxTree = this._transformedSyntaxTreeMap[injectedMember.Transformation.TransformedSyntaxTree];
            var intermediateSyntax = intermediateSyntaxTree.GetRoot().GetCurrentNode( injectedMember.Syntax ).AssertNotNull();

            SyntaxNode symbolSyntax = intermediateSyntax switch
            {
                EventFieldDeclarationSyntax eventFieldSyntax => eventFieldSyntax.Declaration.Variables.First(),
                FieldDeclarationSyntax fieldSyntax => fieldSyntax.Declaration.Variables.First(),
                _ => intermediateSyntax
            };

            return this._semanticModelProvider.GetSemanticModel( intermediateSyntaxTree ).GetDeclaredSymbol( symbolSyntax ).AssertNotNull();
        }

        /// <summary>
        /// Gets introduced members for all transformations.
        /// </summary>
        /// <returns>Enumeration of introduced members.</returns>
        public IEnumerable<LinkerInjectedMember> GetInjectedMembers()
        {
            return this._injectedMemberLookup.Values;
        }

        /// <summary>
        /// Gets all symbols for overridden members.
        /// </summary>
        /// <returns>Enumeration of symbols.</returns>
        public IEnumerable<ISymbol> GetOverriddenMembers()
        {
            // TODO: This is not efficient.
            var returned = new HashSet<ISymbol>( SymbolEqualityComparer.Default );

            foreach ( var injectedMember in this.GetInjectedMembers() )
            {
                var symbol = this.GetSymbolForInjectedMember( injectedMember );

                if ( this.IsOverride( symbol ) )
                {
                    var overrideTarget = this.GetOverrideTarget( symbol ).AssertNotNull();

                    if ( returned.Add( overrideTarget ) )
                    {
                        yield return overrideTarget;
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
                    var injectedMember = this.GetInjectedMemberForSymbol( symbol );

                    if ( injectedMember == null )
                    {
                        return null;
                    }

                    return this.GetOverrideTarget( injectedMember );
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

                    return this.GetSymbolForInjectedMember( lastOverride );
            }
        }

        /// <summary>
        /// Gets the last (outermost) override of the method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns>Symbol.</returns>
        public IMethodSymbol GetLastOverride( IMethodSymbol symbol )
        {
            return (IMethodSymbol) this.GetLastOverride( (ISymbol) symbol );
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

            var injectedMember = this.GetInjectedMemberForSymbol( symbol );

            if ( injectedMember == null )
            {
                return false;
            }

            return injectedMember.Semantic == InjectedMemberSemantic.Override;
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

                foreach ( var injectedMember in overrides.Reverse() )
                {
                    var overrideSymbol = this.GetSymbolForInjectedMember( injectedMember );

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