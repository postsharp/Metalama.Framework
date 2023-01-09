// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltMethod : BuiltMember, IMethodImpl
{
    public BuiltMethod( MethodBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this.MethodBuilder = builder;
    }

    public MethodBuilder MethodBuilder { get; }

    public override MemberBuilder MemberBuilder => this.MethodBuilder;

    public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.MethodBuilder;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this.MethodBuilder.ToTypedRef<IHasParameters>() ) );

    public MethodKind MethodKind => this.MethodBuilder.MethodKind;

    public OperatorKind OperatorKind => this.MethodBuilder.OperatorKind;

    public bool IsReadOnly => this.MethodBuilder.IsReadOnly;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this.MethodBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

    public MethodInfo ToMethodInfo() => this.MethodBuilder.ToMethodInfo();

    IMemberWithAccessors? IMethod.DeclaringMember => null;

    System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this.MethodBuilder.ReturnParameter, this.Compilation );

    [Memo]
    public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodBuilder.ReturnParameter.Type );

    [Memo]
    public IGenericParameterList TypeParameters
        => new TypeParameterList(
            this,
            this.MethodBuilder.TypeParameters.AsBuilderList.Select( Ref.FromBuilder<ITypeParameter, TypeParameterBuilder> ).ToList() );

    public IReadOnlyList<IType> TypeArguments => throw new NotImplementedException();

    public bool IsGeneric => this.MethodBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => throw new NotImplementedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    [Memo]
    public IInvokerFactory<IMethodInvoker> Invokers
        => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ) );

    [Memo]
    public IMethod? OverriddenMethod => this.Compilation.Factory.GetDeclaration( this.MethodBuilder.OverriddenMethod );

    IMethod IMethod.MethodDefinition => this;

    bool IMethod.IsExtern => false;

    public bool? IsIteratorMethod => this.MethodBuilder.IsIteratorMethod;
}