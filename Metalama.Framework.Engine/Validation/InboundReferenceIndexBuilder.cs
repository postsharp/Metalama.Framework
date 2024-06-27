﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

internal sealed class InboundReferenceIndexBuilder : ReferenceIndexBuilder
{
    private readonly ConcurrentDictionary<ISymbol, ReferencedSymbolInfo> _references;
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly ReferenceIndexerOptions _options;
    private bool _frozen;

    public InboundReferenceIndexBuilder( ProjectServiceProvider serviceProvider, ReferenceIndexerOptions options, IEqualityComparer<ISymbol> symbolComparer )
    {
        this._serviceProvider = serviceProvider;
        this._options = options;
        this._references = new ConcurrentDictionary<ISymbol, ReferencedSymbolInfo>( symbolComparer );
    }

    protected override void AddReferenceCore( ISymbol referencedSymbol, ISymbol referencingSymbol, SyntaxNodeOrToken node, ReferenceKinds referenceKind )
    {
        if ( !this._options.MustIndexReferenceKind( referenceKind ) )
        {
            return;
        }

        // Index the explicit reference.
        var referencedSymbolInfo = this._references.GetOrAdd( referencedSymbol, static s => new ReferencedSymbolInfo( s ) );
        referencedSymbolInfo.AddReference( referencingSymbol, node, referenceKind );

        // Create indirect references (containing declarations, base types).
        this.AddToParents( referencedSymbolInfo );
    }

    private void AddToParents( ReferencedSymbolInfo child )
    {
        // Recurse on the base type.
        var referencedSymbol = child.ReferencedSymbol;

        if ( referencedSymbol is INamedTypeSymbol { BaseType: { } baseType }
             && this._options.MustDescendIntoReferencedBaseTypes( ReferenceKinds.All ) )
        {
            this.AddToParent( child, baseType, ChildKinds.DerivedType );
        }

        // Descent into the containing type.
        if ( referencedSymbol.ContainingType != null )
        {
            if ( this._options.MustDescendIntoReferencedDeclaringType( ReferenceKinds.All ) )
            {
                this.AddToParent( child, referencedSymbol.ContainingType, ChildKinds.ContainingDeclaration );

                // There is no need to continue the descent since it was done when processing the containing type.
                return;
            }
        }

        // Descent into the containing namespace.
        if ( referencedSymbol.ContainingNamespace is { IsGlobalNamespace: false } )
        {
            if ( this._options.MustDescendIntoReferencedNamespace( ReferenceKinds.All ) )
            {
                this.AddToParent( child, referencedSymbol.ContainingNamespace, ChildKinds.ContainingDeclaration );

                // There is no need to continue the descent since it was done when processing the containing type.
                return;
            }
        }

        // Descent into the containing assembly.
        if ( referencedSymbol.ContainingAssembly != null )
        {
            if ( this._options.MustDescendIntoReferencedAssembly( ReferenceKinds.All ) )
            {
                this.AddToParent( child, referencedSymbol.ContainingAssembly, ChildKinds.ContainingDeclaration );
            }
        }
    }

    private void AddToParent( ReferencedSymbolInfo child, ISymbol parent, ChildKinds childKind )
    {
        if ( !this._references.TryGetValue( parent, out var parentNode ) )
        {
            parentNode = new ReferencedSymbolInfo( parent );

            if ( this._references.GetOrAdd( parent, parentNode ) == parentNode )
            {
                this.AddToParents( parentNode );
            }
        }

        parentNode.AddChild( child, childKind );
    }

    internal void IndexSemanticModel( SemanticModel semanticModel, CancellationToken cancellationToken )
    {
        if ( this._frozen )
        {
            throw new InvalidOperationException();
        }

        var visitor = new ReferenceIndexWalker( this._serviceProvider, cancellationToken, this, this._options );
        visitor.Visit( semanticModel );
    }

    internal void IndexSyntaxTree( SyntaxTree syntaxTree, SemanticModelProvider semanticModelProvider, CancellationToken cancellationToken = default )
    {
        if ( this._frozen )
        {
            throw new InvalidOperationException();
        }

        var visitor = new ReferenceIndexWalker( this._serviceProvider, cancellationToken, this, this._options, semanticModelProvider );
        visitor.Visit( syntaxTree );
    }

    public InboundReferenceIndex ToReadOnly()
    {
        this._frozen = true;

        return new InboundReferenceIndex( this._references );
    }
}