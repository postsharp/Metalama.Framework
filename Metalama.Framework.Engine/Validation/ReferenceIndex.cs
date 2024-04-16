// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceIndex
{
    private readonly ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, List<ReferencingNode>>> _explicitReferences;

    internal ReferenceIndex( ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, List<ReferencingNode>>> explicitReferences )
    {
        this._explicitReferences = explicitReferences;
    }

    public IEnumerable<ReferencedSymbolInfo> ReferencedSymbols
        => this._explicitReferences.SelectAsReadOnlyCollection( x => new ReferencedSymbolInfo( x.Key, x.Value ) );

    public readonly struct ReferencedSymbolInfo
    {
        private readonly ConcurrentDictionary<ISymbol, List<ReferencingNode>> _references;

        public ISymbol ReferencedSymbol { get; }

        internal ReferencedSymbolInfo( ISymbol referencedSymbol, ConcurrentDictionary<ISymbol, List<ReferencingNode>> references )
        {
            this._references = references;
            this.ReferencedSymbol = referencedSymbol;
            this.AllReferenceKinds = this.ComputeAllReferenceKinds();
        }

        public IEnumerable<ReferencingSymbolInfo> References => this._references.SelectAsReadOnlyCollection( x => new ReferencingSymbolInfo( x.Key, x.Value ) );

        public ReferenceKinds AllReferenceKinds { get; }

        private ReferenceKinds ComputeAllReferenceKinds()
        {
            var referenceKinds = ReferenceKinds.None;

            foreach ( var reference in this._references )
            {
                foreach ( var node in reference.Value )
                {
                    referenceKinds |= node.ReferenceKinds;
                }
            }

            return referenceKinds;
        }
    }

    public readonly struct ReferencingSymbolInfo
    {
        internal ReferencingSymbolInfo( ISymbol referencingSymbol, IReadOnlyList<ReferencingNode> nodes )
        {
            this.ReferencingSymbol = referencingSymbol;
            this.Nodes = nodes;
        }

        public ISymbol ReferencingSymbol { get; }

        public IReadOnlyList<ReferencingNode> Nodes { get; }
    }
}