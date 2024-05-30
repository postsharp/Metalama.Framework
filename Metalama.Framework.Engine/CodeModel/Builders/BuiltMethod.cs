// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltMethod : BuiltMethodBase, IMethodImpl
{
    private readonly MethodBuilder _methodBuilder;

    public BuiltMethod( MethodBuilder builder, CompilationModel compilation ) : base( builder, compilation )
    {
        this._methodBuilder = builder;
    }

    protected override MemberBuilder MemberBuilder => this._methodBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._methodBuilder;

    protected override MethodBaseBuilder MethodBaseBuilder => this._methodBuilder;

    public MethodKind MethodKind => this._methodBuilder.MethodKind;

    public OperatorKind OperatorKind => this._methodBuilder.OperatorKind;

    public bool IsReadOnly => this._methodBuilder.IsReadOnly;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this._methodBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

    public MethodInfo ToMethodInfo() => this._methodBuilder.ToMethodInfo();

    IHasAccessors? IMethod.DeclaringMember => null;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToMethodInfo();

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this._methodBuilder.ReturnParameter, this.Compilation );

    [Memo]
    public IType ReturnType => this.Compilation.Factory.GetIType( this._methodBuilder.ReturnParameter.Type );

    [Memo]
    public IGenericParameterList TypeParameters
        => new TypeParameterList(
            this,
            this._methodBuilder.TypeParameters.AsBuilderList.Select( Ref.FromBuilder<ITypeParameter> ).ToReadOnlyList() );

    public IReadOnlyList<IType> TypeArguments => this.TypeParameters;

    public bool IsGeneric => this._methodBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => throw new NotImplementedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    [Memo]
    public IMethod? OverriddenMethod => this.Compilation.Factory.GetDeclaration( this._methodBuilder.OverriddenMethod );

    IMethod IMethod.Definition => this;

    bool IMethod.IsPartial => false;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => this._methodBuilder.With( options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => this._methodBuilder.With( target, options );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => this._methodBuilder.With( target, options );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => this._methodBuilder.CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => this._methodBuilder.Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => this._methodBuilder.Invoke( args );

    public bool? IsIteratorMethod => this._methodBuilder.IsIteratorMethod;
}