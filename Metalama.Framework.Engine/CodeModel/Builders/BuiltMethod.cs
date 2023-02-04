// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltMethod : BuiltMember, IMethodImpl
{
    private readonly MethodBuilder _methodBuilder;

    public BuiltMethod( MethodBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this._methodBuilder = builder;
    }

    protected override MemberBuilder MemberBuilder => this._methodBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._methodBuilder;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this._methodBuilder.ToTypedRef<IHasParameters>() ) );

    public MethodKind MethodKind => this._methodBuilder.MethodKind;

    public OperatorKind OperatorKind => this._methodBuilder.OperatorKind;

    public bool IsReadOnly => this._methodBuilder.IsReadOnly;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this._methodBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

    public MethodInfo ToMethodInfo() => this._methodBuilder.ToMethodInfo();

    IMemberWithAccessors? IMethod.DeclaringMember => null;

    System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this._methodBuilder.ReturnParameter, this.Compilation );

    [Memo]
    public IType ReturnType => this.Compilation.Factory.GetIType( this._methodBuilder.ReturnParameter.Type );

    [Memo]
    public IGenericParameterList TypeParameters
        => new TypeParameterList(
            this,
            this._methodBuilder.TypeParameters.AsBuilderList.Select( Ref.FromBuilder<ITypeParameter, TypeParameterBuilder> ).ToList() );

    public IReadOnlyList<IType> TypeArguments => throw new NotImplementedException();

    public bool IsGeneric => this._methodBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => throw new NotImplementedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    [Obsolete]
    IInvokerFactory<IMethodInvoker> IMethod.Invokers => throw new NotSupportedException();

    [Memo]
    public IMethod? OverriddenMethod => this.Compilation.Factory.GetDeclaration( this._methodBuilder.OverriddenMethod );

    IMethod IMethod.MethodDefinition => this;

    bool IMethod.IsExtern => false;

    public object? Invoke( object? target, params object?[] args ) => TemplateExpansionContext.CurrentInvocationApi.Invoke( this, target, args );

    public bool? IsIteratorMethod => this._methodBuilder.IsIteratorMethod;
}