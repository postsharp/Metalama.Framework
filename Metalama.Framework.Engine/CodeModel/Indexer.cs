// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class Indexer : PropertyOrIndexer, IIndexerImpl
{
    public Indexer( IPropertySymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.PropertySymbol.Parameters.Select( p => new Ref<IParameter>( p, this.Compilation.CompilationContext ) ).ToList() );

    public IIndexer? OverriddenIndexer
    {
        get
        {
            var overriddenProperty = this.PropertySymbol.OverriddenProperty;

            if ( overriddenProperty != null )
            {
                return this.Compilation.Factory.GetIndexer( overriddenProperty );
            }
            else
            {
                return null;
            }
        }
    }

    public IMember? OverriddenMember => this.OverriddenIndexer;

    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetIndexer( p ) ).ToList();

    [Memo]
    public IInvokerFactory<IIndexerInvoker> Invokers => new InvokerFactory<IIndexerInvoker>( ( order, _ ) => new IndexerInvoker( this, order ) );

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;
}