// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltPropertyOrIndexer : BuiltMember, IPropertyOrIndexerImpl
{
    private readonly PropertyOrIndexerBuilder _propertyOrIndexerBuilder;

    public BuiltPropertyOrIndexer( PropertyOrIndexerBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this._propertyOrIndexerBuilder = builder;
    }

    protected override MemberBuilder MemberBuilder => this._propertyOrIndexerBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._propertyOrIndexerBuilder;

    public RefKind RefKind => this._propertyOrIndexerBuilder.RefKind;

    public Writeability Writeability => this._propertyOrIndexerBuilder.Writeability;

    [Memo]
    public IType Type => this.Compilation.Factory.GetIType( this._propertyOrIndexerBuilder.Type );

    [Memo]
    public IMethod? GetMethod
        => this._propertyOrIndexerBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._propertyOrIndexerBuilder.GetMethod ) : null;

    [Memo]
    public IMethod? SetMethod
        => this._propertyOrIndexerBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this._propertyOrIndexerBuilder.SetMethod ) : null;

    public PropertyInfo ToPropertyInfo() => this._propertyOrIndexerBuilder.ToPropertyInfo();

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors => this._propertyOrIndexerBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );
}