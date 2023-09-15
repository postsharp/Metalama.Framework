// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltIndexer : BuiltMember, IIndexerImpl
{
    private readonly IndexerBuilder _indexerBuilder;

    public BuiltIndexer( IndexerBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this._indexerBuilder = builder;
    }

    protected override MemberBuilder MemberBuilder => this._indexerBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._indexerBuilder;

    public RefKind RefKind => this._indexerBuilder.RefKind;

    public Writeability Writeability => this._indexerBuilder.Writeability;

    [Memo]
    public IType Type => this.Compilation.Factory.GetIType( this._indexerBuilder.Type );

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this._indexerBuilder.ToTypedRef<IHasParameters>() ) );

    [Memo]
    public IMethod? GetMethod => this._indexerBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._indexerBuilder.GetMethod ) : null;

    [Memo]
    public IMethod? SetMethod => this._indexerBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._indexerBuilder.SetMethod ) : null;

    [Memo]
    public IIndexer? OverriddenIndexer => this.Compilation.Factory.GetDeclaration( this._indexerBuilder.OverriddenIndexer );

    IIndexer IIndexer.IndexerDefinition => this;

    public IIndexerInvoker With( InvokerOptions options ) => this._indexerBuilder.With( options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => this._indexerBuilder.With( target, options );

    public object GetValue( params object?[] args ) => this._indexerBuilder.With( args );

    public object SetValue( object? value, params object?[] args ) => this._indexerBuilder.SetValue( value, args );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this._indexerBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

    public PropertyInfo ToPropertyInfo() => this._indexerBuilder.ToPropertyInfo();

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors => this._indexerBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );
}