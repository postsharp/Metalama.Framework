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
            this.PropertySymbol.Parameters.Select( p => new Ref<IParameter>( p, this.Compilation.CompilationContext ) ).ToReadOnlyList() );

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

    [Memo]
    public IIndexer Definition
        => this.PropertySymbol == this.PropertySymbol.OriginalDefinition ? this : this.Compilation.Factory.GetIndexer( this.PropertySymbol.OriginalDefinition );

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public IIndexerInvoker With( InvokerOptions options ) => new IndexerInvoker( this, options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => new IndexerInvoker( this, options, target );

    public object GetValue( params object?[] args ) => new IndexerInvoker( this ).GetValue( args );

    public object SetValue( object? value, params object?[] args ) => new IndexerInvoker( this ).SetValue( value, args );

    public IMember? OverriddenMember => this.OverriddenIndexer;

    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this.PropertySymbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetIndexer( p ) ).ToReadOnlyList();

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;

    [Memo]
    private BoxedRef<IIndexer> BoxedRef => new BoxedRef<IIndexer>( this.ToValueTypedRef() );

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.BoxedRef;

    IRef<IIndexer> IIndexer.ToRef() => this.BoxedRef;

    protected override IRef<IPropertyOrIndexer> ToPropertyOrIndexerRef() => this.BoxedRef;

    protected override IRef<IFieldOrPropertyOrIndexer> ToFieldOrPropertyOrIndexerRef() => this.BoxedRef;

    protected override IRef<IMember> ToMemberRef() => this.BoxedRef;

    protected override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.BoxedRef;
}