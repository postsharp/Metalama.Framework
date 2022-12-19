// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Linking.Substitution;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Contains information collected during analysis of the intermediate assembly and provides methods querying this information.
    /// </summary>
    internal sealed class LinkerAnalysisRegistry
    {
        private readonly HashSet<IntermediateSymbolSemantic> _reachableSemantics;
        private readonly HashSet<IntermediateSymbolSemantic> _inlinedSemantics;
        private readonly IReadOnlyDictionary<InliningContextIdentifier, IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>> _substitutions;

        public LinkerAnalysisRegistry(
            IReadOnlyList<IntermediateSymbolSemantic> reachableSemantics,
            IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics,
            IReadOnlyDictionary<InliningContextIdentifier, IReadOnlyList<SyntaxNodeSubstitution>> substitutions )
        {
            this._reachableSemantics = new HashSet<IntermediateSymbolSemantic>( reachableSemantics );
            this._inlinedSemantics = new HashSet<IntermediateSymbolSemantic>( inlinedSemantics );

            this._substitutions =
                substitutions.ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>) x.Value.ToDictionary( y => y.TargetNode, y => y ) );
        }

        public bool IsReachable( IntermediateSymbolSemantic semantic ) => this._reachableSemantics.Contains( semantic );

        public bool IsInlined( IntermediateSymbolSemantic semantic ) => this._inlinedSemantics.Contains( semantic );

        public IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>? GetSubstitutions( InliningContextIdentifier contextId )
        {
            if ( !this._substitutions.TryGetValue( contextId, out var substitutions ) )
            {
                return null;
            }

            return substitutions;
        }

        public bool HasAnyRedirectionSubstitutions( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    var semantic = methodSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    var rootContextId = new InliningContextIdentifier( semantic );

                    if ( this._substitutions.TryGetValue( rootContextId, out var substitutions ) )
                    {
                        return substitutions.Values.Any( x => x is RedirectionSubstitution );
                    }
                    else
                    {
                        return false;
                    }

                case IPropertySymbol propertySymbol:
                    if ( (propertySymbol.GetMethod != null && this.HasAnyRedirectionSubstitutions( propertySymbol.GetMethod ))
                         || (propertySymbol.SetMethod != null && this.HasAnyRedirectionSubstitutions( propertySymbol.SetMethod )) )
                    {
                        return true;
                    }

                    return false;

                case IEventSymbol eventSymbol:
                    if ( (eventSymbol.AddMethod != null && this.HasAnyRedirectionSubstitutions( eventSymbol.AddMethod ))
                         || (eventSymbol.RemoveMethod != null && this.HasAnyRedirectionSubstitutions( eventSymbol.RemoveMethod )) )
                    {
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        public bool HasAnyForcefullyInitializedFields( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    var semantic = methodSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    var rootContextId = new InliningContextIdentifier( semantic );

                    if ( this._substitutions.TryGetValue( rootContextId, out var substitutions ) )
                    {
                        return substitutions.Values.Any( x => x is ForcedInitializationSubstitution );
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        public bool HasAnyEventFieldRaiseSubstitution( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    var semantic = methodSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    var rootContextId = new InliningContextIdentifier( semantic );

                    if ( this._substitutions.TryGetValue( rootContextId, out var substitutions ) )
                    {
                        return substitutions.Values.Any( x => x is EventFieldRaiseSubstitution );
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }
    }
}