// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltPropertyOrIndexer : BuiltMember, IPropertyOrIndexerImpl
{
    protected BuiltPropertyOrIndexer( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract PropertyOrIndexerBuilder PropertyOrIndexerBuilder { get; }

    public RefKind RefKind => this.PropertyOrIndexerBuilder.RefKind;

    public Writeability Writeability => this.PropertyOrIndexerBuilder.Writeability;

    [Memo]
    public IType Type => this.Compilation.Factory.Translate( this.PropertyOrIndexerBuilder.Type );

    [Memo]
    public IMethod? GetMethod
        => this.PropertyOrIndexerBuilder.GetMethod != null
            ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyOrIndexerBuilder.GetMethod, this.GenericContext )
            : null;

    [Memo]
    public IMethod? SetMethod
        => this.PropertyOrIndexerBuilder.SetMethod != null
            ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyOrIndexerBuilder.SetMethod, this.GenericContext )
            : null;

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.PropertyOrIndexerBuilder.ToPropertyOrIndexerRef();

    public PropertyInfo ToPropertyInfo() => this.PropertyOrIndexerBuilder.ToPropertyInfo();

    IRef<IPropertyOrIndexer> IPropertyOrIndexer.ToRef() => this.PropertyOrIndexerBuilder.ToPropertyOrIndexerRef();

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors
        => this.PropertyOrIndexerBuilder.Accessors.Select( a => this.Compilation.Factory.Translate( a, genericContext: this.GenericContext ) ).WhereNotNull();
}