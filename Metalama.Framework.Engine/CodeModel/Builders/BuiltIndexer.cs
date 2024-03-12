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
    public BuiltIndexer( IndexerBuilder builder, CompilationModel compilation ) : base( builder, compilation )
    {
    }

    public IndexerBuilder IndexerBuilder => (IndexerBuilder) this.MemberBuilder;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this.IndexerBuilder.ToTypedRef<IHasParameters>() ) );

    [Memo]
    public IIndexer? OverriddenIndexer => this.Compilation.Factory.GetDeclaration( this.IndexerBuilder.OverriddenIndexer );

    IIndexer IIndexer.Definition => this;

    public IIndexerInvoker With( InvokerOptions options ) => this.IndexerBuilder.With( options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => this.IndexerBuilder.With( target, options );

    public object GetValue( params object?[] args ) => this.IndexerBuilder.With( args );

    public object SetValue( object? value, params object?[] args ) => this.IndexerBuilder.SetValue( value, args );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this.IndexerBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );
}