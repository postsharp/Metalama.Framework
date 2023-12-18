// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Stores information about injections and intermediate compilation.
    /// </summary>
    internal sealed class LinkerInjectionRegistry
    {
        public const string InjectedNodeIdAnnotationId = "AspectLinker_InjectedNodeId";

        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly PartialCompilation _intermediateCompilation;
        private readonly IReadOnlyDictionary<SyntaxTree, SyntaxTree> _transformedSyntaxTreeMap;
        private readonly IReadOnlyList<LinkerInjectedMember> _injectedMembers;
        private readonly IReadOnlyList<ISymbol> _overrideTargets;
        private readonly IReadOnlyDictionary<IDeclarationBuilder, LinkerInjectedMember> _builderToInjectedMemberMap;
        private readonly IReadOnlyDictionary<ISymbol, IReadOnlyList<ISymbol>> _overrideTargetToOverrideListMap;
        private readonly IReadOnlyDictionary<ISymbol, LinkerInjectedMember> _symbolToInjectedMemberMap;
        private readonly IReadOnlyDictionary<LinkerInjectedMember, ISymbol> _injectedMemberToSymbolMap;
        private readonly IReadOnlyDictionary<ISymbol, ISymbol> _overrideToOverrideTargetMap;

        public LinkerInjectionRegistry(
            TransformationLinkerOrderComparer comparer,
            CompilationModel finalCompilationModel,
            PartialCompilation intermediateCompilation,
            IEnumerable<SyntaxTreeTransformation> transformations,
            IReadOnlyCollection<LinkerInjectedMember> injectedMembers,
            IDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap )
        {
            List<ISymbol> overrideTargets;
            Dictionary<ISymbol, IReadOnlyList<ISymbol>> overrideMap;
            Dictionary<ISymbol, ISymbol> overrideTargetMap;
            Dictionary<ISymbol, LinkerInjectedMember> symbolToInjectedMemberMap;
            Dictionary<LinkerInjectedMember, ISymbol> injectedMemberToSymbolMap;
            Dictionary<IDeclarationBuilder, LinkerInjectedMember> builderToInjectedMemberMap;

            this._comparer = comparer;
            this._intermediateCompilation = intermediateCompilation;

            this._transformedSyntaxTreeMap = transformations
                .Where( m => m.Kind == SyntaxTreeTransformationKind.Replace )
                .ToDictionary( m => m.OldTree.AssertNotNull(), m => m.NewTree.AssertNotNull() );
            this._injectedMembers = injectedMembers.ToList();

            var injectedMemberByNodeId = injectedMembers.ToDictionary( x => x.LinkerNodeId, x => x );
            this._overrideTargets = overrideTargets = new List<ISymbol>();
            this._builderToInjectedMemberMap = builderToInjectedMemberMap = new Dictionary<IDeclarationBuilder, LinkerInjectedMember>();
            this._overrideTargetToOverrideListMap = overrideMap = new Dictionary<ISymbol, IReadOnlyList<ISymbol>>( intermediateCompilation.CompilationContext.SymbolComparer );
            this._overrideToOverrideTargetMap = overrideTargetMap = new Dictionary<ISymbol, ISymbol>( intermediateCompilation.CompilationContext.SymbolComparer );
            this._symbolToInjectedMemberMap = symbolToInjectedMemberMap = new Dictionary<ISymbol, LinkerInjectedMember>( intermediateCompilation.CompilationContext.SymbolComparer );
            this._injectedMemberToSymbolMap = injectedMemberToSymbolMap = new Dictionary<LinkerInjectedMember, ISymbol>();

            // TODO: This could be parallelized. The collections could be built in the LinkerInjectionStep, it is in
            //       the same spirit as the Index* methods.
            //       However, even for very large projects it seems to would have very small impact.

            var overriddenDeclarations = new Dictionary<IDeclaration, List<ISymbol>>( intermediateCompilation.CompilationContext.Comparers.Default );

            foreach ( var injectedMember in this._injectedMembers )
            {
                var symbol = GetSymbolForInjectedMember( injectedMember );

                // Basic maps.
                symbolToInjectedMemberMap[symbol] = injectedMember;
                injectedMemberToSymbolMap[injectedMember] = symbol;

                if ( injectedMember.Transformation is IOverrideDeclarationTransformation overrideTransformation )
                {
                    if ( !overriddenDeclarations.TryGetValue( overrideTransformation.OverriddenDeclaration, out var overrideInjectedMembers ) )
                    {
                        overriddenDeclarations[overrideTransformation.OverriddenDeclaration] = overrideInjectedMembers = new List<ISymbol>();
                    }

                    overrideInjectedMembers.Add( symbol );
                }

                if ( injectedMember.Transformation is IIntroduceDeclarationTransformation introduceTransformation )
                {
                    builderToInjectedMemberMap[introduceTransformation.DeclarationBuilder] = injectedMember;
                }
            }

            foreach ( var overriddenDeclaration in overriddenDeclarations )
            {
                var overrideTargetSymbol = GetOverrideTargetSymbol( overriddenDeclaration.Key );

                overriddenDeclaration.Value.Sort( ( x, y ) => this._comparer.Compare( symbolToInjectedMemberMap[x].Transformation, symbolToInjectedMemberMap[y].Transformation ) );

                overrideTargets.Add( overrideTargetSymbol );
                overrideMap.Add( overrideTargetSymbol, overriddenDeclaration.Value );

                foreach(var overrideSymbol in overriddenDeclaration.Value)
                {
                    overrideTargetMap.Add( overrideSymbol, overrideTargetSymbol );
                }
            }

            ISymbol GetSymbolForInjectedMember( LinkerInjectedMember injectedMember )
            {
                var intermediateSyntaxTree = this._transformedSyntaxTreeMap[injectedMember.Transformation.TransformedSyntaxTree];
                var intermediateSyntax = intermediateSyntaxTree.GetRoot().GetCurrentNode( injectedMember.Syntax ).AssertNotNull();

                SyntaxNode symbolSyntax = intermediateSyntax switch
                {
                    EventFieldDeclarationSyntax eventFieldSyntax => eventFieldSyntax.Declaration.Variables.First(),
                    FieldDeclarationSyntax fieldSyntax => fieldSyntax.Declaration.Variables.First(),
                    _ => intermediateSyntax
                };

                return this._intermediateCompilation.CompilationContext.SemanticModelProvider.GetSemanticModel( intermediateSyntaxTree )
                    .GetDeclaredSymbol( symbolSyntax )
                    .AssertNotNull();
            }

            ISymbol? GetOverrideTargetSymbol( IDeclaration overrideTarget )
            {
                if ( overrideTarget is Declaration originalDeclaration )
                {
                    return this._intermediateCompilation.CompilationContext.SymbolTranslator.Translate( originalDeclaration.GetSymbol().AssertNotNull(), true );
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
                    var introducedBuilder = builderToInjectedMemberMap[builder];
                    var sourceSyntaxTree = ((IDeclarationImpl) builder).PrimarySyntaxTree.AssertNotNull();
                    var intermediateSyntaxTree = this._transformedSyntaxTreeMap[sourceSyntaxTree];
                    var intermediateNode = intermediateSyntaxTree.GetRoot().GetCurrentNode( introducedBuilder.Syntax );

                    var intermediateSemanticModel =
                        this._intermediateCompilation.CompilationContext.SemanticModelProvider.GetSemanticModel( intermediateSyntaxTree );

                    var symbolNode = intermediateNode.AssertNotNull() switch
                    {
                        EventFieldDeclarationSyntax eventFieldNode => (SyntaxNode) eventFieldNode.Declaration.Variables.First(),
                        _ => intermediateNode!
                    };

                    return intermediateSemanticModel.GetDeclaredSymbol( symbolNode );

                }
            }
        }

        /// <summary>
        /// Gets introduced members representing overrides of a symbol.
        /// </summary>
        /// <param name="referencedSymbol">Symbol.</param>
        /// <returns>List of introduced members.</returns>
        public IReadOnlyList<ISymbol> GetOverridesForSymbol( ISymbol referencedSymbol )
        {
            if (this._overrideTargetToOverrideListMap.TryGetValue(referencedSymbol, out var overrideSymbolList))
            {
                return overrideSymbolList;
            }
            else
            {
                return Array.Empty<ISymbol>();
            }
        }

        private ISymbol? GetOverrideTarget( LinkerInjectedMember overrideInjectedMember )
        {
            if ( !this._overrideToOverrideTargetMap.TryGetValue( this._injectedMemberToSymbolMap[overrideInjectedMember], out var overrideTarget ) )
            {
                // Coverage: ignore (coverage is irrelevant, needed for correctness)
                return null;
            }

            return overrideTarget;
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

            if (this._symbolToInjectedMemberMap.TryGetValue( symbol, out var injectedMember))
            {
                return injectedMember;
            }
            else
            {
                return null;
            }
        }

        public IIntroduceDeclarationTransformation? GetTransformationForBuilder( IDeclarationBuilder builder )
        {
            if ( this._builderToInjectedMemberMap.TryGetValue( builder, out var injectedMember ) )
            {
                // Builder that was removed.
                return (IIntroduceDeclarationTransformation) injectedMember.Transformation;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a symbol in intermediate compilation that represents a declaration introduced by the introduced member.
        /// </summary>
        /// <param name="injectedMember"></param>
        /// <returns></returns>
        public ISymbol GetSymbolForInjectedMember( LinkerInjectedMember injectedMember )
        {
            return this._injectedMemberToSymbolMap[injectedMember];
        }

        /// <summary>
        /// Gets introduced members for all transformations.
        /// </summary>
        /// <returns>Enumeration of introduced members.</returns>
        public IEnumerable<LinkerInjectedMember> GetInjectedMembers()
        {
            return this._injectedMembers;
        }

        /// <summary>
        /// Gets all symbols for overridden members.
        /// </summary>
        /// <returns>Enumeration of symbols.</returns>
        public IReadOnlyList<ISymbol> GetOverriddenMembers()
        {
            return this._overrideTargets;
        }

        public ISymbol? GetOverrideTarget( ISymbol overrideSymbol )
        {
            if (this._overrideToOverrideTargetMap.TryGetValue(overrideSymbol, out var overrideTargetSymbol))
            {
                return overrideTargetSymbol;
            }
            else
            {
                return null;
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
                    var overrides = this.GetOverridesForSymbol( symbol );
                    var lastOverride = overrides.Count > 0 ? overrides[overrides.Count - 1] : null;

                    if ( lastOverride == null )
                    {
                        // Coverage: ignore (coverage is irrelevant, needed for correctness).
                        return symbol;
                    }

                    return lastOverride;
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
        /// Determines whether the symbol was introduced.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns><c>True</c> if the method is introduced, otherwise <c>false</c>.</returns>
        public bool IsIntroduced( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol
                {
                    MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove
                } methodSymbol )
            {
                return this.IsIntroduced( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            var injectedMember = this.GetInjectedMemberForSymbol( symbol );

            if ( injectedMember == null )
            {
                return false;
            }

            return injectedMember.Semantic == InjectedMemberSemantic.Introduction;
        }

        /// <summary>
        /// Determines whether the symbol represents override target.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns><c>True</c> if the method is override target, otherwise <c>false</c>.</returns>
        public bool IsOverrideTarget( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol
                {
                    MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove
                } methodSymbol )
            {
                return this.IsOverrideTarget( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            return this._overrideTargetToOverrideListMap.ContainsKey( symbol );
        }

        /// <summary>
        /// Determines whether the symbol represents an override method.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public bool IsOverride( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol
                {
                    MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove
                } methodSymbol )
            {
                return this.IsOverride( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            return this._overrideTargetToOverrideListMap.ContainsKey( symbol );
        }

        // Resharper disable once UnusedMember.Global
        public bool IsLastOverride( ISymbol symbol )
        {
            return 
                this.IsOverride( symbol ) && 
                this._intermediateCompilation.CompilationContext.SymbolComparer.Equals(
                    symbol,
                    this.GetLastOverride( this.GetOverrideTarget( symbol ).AssertNotNull() ) );
        }

        public IAspectClass? GetSourceAspect( ISymbol symbol )
        {
            var injectedMember = this.GetInjectedMemberForSymbol( symbol );

            return injectedMember?.Transformation.ParentAdvice.Aspect.AspectClass;
        }
    }
}