// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltAccessor : BuiltDeclaration, IMethodImpl
{
    private readonly BuiltMember _builtMember;
    private readonly AccessorBuilder _accessorBuilder;

    public BuiltAccessor( BuiltMember builtMember, AccessorBuilder builder ) : base( builtMember.Compilation, builtMember.GenericContext )
    {
        this._builtMember = builtMember;
        this._accessorBuilder = builder;
    }

    public override DeclarationBuilder Builder => this._accessorBuilder;

    public Accessibility Accessibility => this._accessorBuilder.Accessibility;

    public string Name => this._accessorBuilder.Name;

    public bool IsPartial => this._accessorBuilder.IsPartial;

    public bool HasImplementation => this._accessorBuilder.HasImplementation;

    public bool IsAbstract => this._accessorBuilder.IsAbstract;

    public bool IsStatic => this._accessorBuilder.IsStatic;

    public bool IsVirtual => this._accessorBuilder.IsVirtual;

    public bool IsSealed => this._accessorBuilder.IsSealed;

    public bool IsReadOnly => this._accessorBuilder.IsReadOnly;

    public bool IsOverride => this._accessorBuilder.IsOverride;

    public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public bool IsNew => this._accessorBuilder.IsNew;

    public bool? HasNewKeyword => false;

    public bool IsAsync => this._accessorBuilder.IsAsync;

    public override bool IsImplicitlyDeclared
        => this is { MethodKind: MethodKind.PropertySet, ContainingDeclaration: IProperty { Writeability: Writeability.ConstructorOnly } };

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.Compilation.GetParameterCollection( this.Ref ) );

    public MethodKind MethodKind => this._accessorBuilder.MethodKind;

    public OperatorKind OperatorKind => this._accessorBuilder.OperatorKind;

    [Memo]
    public IMethod Definition => this.Compilation.Factory.GetAccessor( this._accessorBuilder );

    [Memo]
    private IRef<IMethod> Ref => this.RefFactory.FromBuilt<IMethod>( this );

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    IRef<IMember> IMember.ToRef() => this.Ref;

    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => this.Ref;

    public IRef<IMethod> ToRef() => this.Ref;

    IRef<IMethodBase> IMethodBase.ToRef() => this.Ref;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.Definition;

    IMember IMember.Definition => this;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => this._accessorBuilder.With( options );

    public IMethodInvoker With( object? target, InvokerOptions options ) => this._accessorBuilder.With( target, options );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => this._accessorBuilder.With( target, options );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => this._accessorBuilder.CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => this._accessorBuilder.Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => this._accessorBuilder.Invoke( args );

    [Memo]
    public IParameter ReturnParameter
        => new BuiltParameter( (BaseParameterBuilder) this._accessorBuilder.ReturnParameter, this.Compilation, this.GenericContext );

    [Memo]
    public IType ReturnType => this.MapType( this._accessorBuilder.ReturnParameter.Type );

    public IGenericParameterList TypeParameters => TypeParameterList.Empty;

    IReadOnlyList<IType> IGeneric.TypeArguments => [];

    public bool IsGeneric => this._accessorBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => this.DeclaringType.IsCanonicalGenericInstance;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => this._accessorBuilder.ConstructGenericInstance( typeArguments );

    public IMethod? OverriddenMethod => this._accessorBuilder.OverriddenMethod;

    public INamedType DeclaringType => this._builtMember.DeclaringType;

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this._accessorBuilder.ExplicitInterfaceImplementations;

    public MethodInfo ToMethodInfo() => this._accessorBuilder.ToMethodInfo();

    IHasAccessors IMethod.DeclaringMember => (IHasAccessors) this._builtMember;

    public System.Reflection.MethodBase ToMethodBase() => this._accessorBuilder.ToMethodBase();

    public MemberInfo ToMemberInfo() => this._accessorBuilder.ToMemberInfo();

    ExecutionScope IMemberOrNamedType.ExecutionScope => ExecutionScope.RunTime;

    [Memo]
    public IMember? OverriddenMember => this.Compilation.Factory.Translate( this._accessorBuilder.OverriddenMember, genericContext: this.GenericContext );

    public bool? IsIteratorMethod => this._accessorBuilder.IsIteratorMethod;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
    {
        if ( !this.CanBeInherited )
        {
            return Enumerable.Empty<IDeclaration>();
        }
        else
        {
            return Member.GetDerivedDeclarationsCore( this, options );
        }
    }
}