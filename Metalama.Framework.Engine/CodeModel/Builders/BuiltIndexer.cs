// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltIndexer : BuiltPropertyOrIndexer, IIndexerImpl
{
    private readonly IndexerBuilder _indexerBuilder;

    public BuiltIndexer( CompilationModel compilation, IndexerBuilder builder ) : base( compilation ) 
    {
        this._indexerBuilder = builder;
    }

    public override DeclarationBuilder Builder => this._indexerBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this._indexerBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._indexerBuilder;

    protected override MemberBuilder MemberBuilder => this._indexerBuilder;

    protected override PropertyOrIndexerBuilder PropertyOrIndexerBuilder => this._indexerBuilder;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this._indexerBuilder.ToTypedRef<IHasParameters>() ) );

    [Memo]
    public IIndexer? OverriddenIndexer => this.Compilation.Factory.GetDeclaration( this._indexerBuilder.OverriddenIndexer );

    IIndexer IIndexer.Definition => this;

    public IIndexerInvoker With( InvokerOptions options ) => this._indexerBuilder.With( options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => this._indexerBuilder.With( target, options );

    public object GetValue( params object?[] args ) => this._indexerBuilder.With( args );

    public object SetValue( object? value, params object?[] args ) => this._indexerBuilder.SetValue( value, args );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this._indexerBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );
}