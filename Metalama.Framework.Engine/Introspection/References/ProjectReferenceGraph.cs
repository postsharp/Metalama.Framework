// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection.References;

internal sealed class ProjectReferenceGraph( CompilationModel compilation, ReferenceIndex referenceIndex ) : IReferenceGraph
{
    private WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>? _includeDerivedTypesCache;
    private WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>? _includeContainedDeclarationsCache;
    private WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>? _includeAllCache;
    private WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>? _includeNoChildrenCache;

    public IReadOnlyCollection<IDeclarationReference> GetIncomingReferences(
        IDeclaration destination,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration )
    {
        var cache = childKinds switch
        {
            ReferenceGraphChildKinds.None => this._includeNoChildrenCache ??= new WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>(),
            ReferenceGraphChildKinds.DerivedType => this._includeDerivedTypesCache ??=
                new WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>(),
            ReferenceGraphChildKinds.ContainingDeclaration => this._includeContainedDeclarationsCache ??=
                new WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>(),
            ReferenceGraphChildKinds.All => this._includeAllCache ??= new WeakCache<IDeclaration, IReadOnlyCollection<IDeclarationReference>>(),
            _ => throw new ArgumentOutOfRangeException( nameof(childKinds), childKinds, null )
        };

        // Cache fast path.
        if ( cache.TryGetValue( destination, out var result ) )
        {
            return result;
        }

        return cache.GetOrAdd( destination, _ => this.GetIncomingReferencesCore( destination, childKinds ) );
    }

    private IReadOnlyCollection<IDeclarationReference> GetIncomingReferencesCore( IDeclaration destination, ReferenceGraphChildKinds childKinds )
    {
        var symbol = destination.GetSymbol();

        if ( symbol == null )
        {
            return [];
        }
        else if ( referenceIndex.TryGetIncomingReferences( symbol, out var referencedSymbolInfo ) )
        {
            var descendants = referencedSymbolInfo.DescendantsAndSelf( ChildKindHelper.ToChildKinds( childKinds ) );

            return descendants
                .SelectMany( d => d.References.Select( r => (d.ReferencedSymbol, ReferencingSymbolInfo: r) ) )
                .Select( x => new DeclarationReference( x.ReferencedSymbol, x.ReferencingSymbolInfo, compilation ) )
                .ToReadOnlyList();
        }
        else
        {
            return [];
        }
    }
}