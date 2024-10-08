// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodBase = System.Reflection.MethodBase;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltAccessor : BuiltDeclaration, IMethodImpl
{
    private readonly BuiltMember _builtMember;
    private readonly MethodBuilderData _accessorBuilder;

    public BuiltAccessor( BuiltMember builtMember, MethodBuilderData builder ) : base( builtMember.Compilation, builtMember.GenericContext )
    {
        this._builtMember = builtMember;
        this._accessorBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._accessorBuilder;

    public Accessibility Accessibility => this._accessorBuilder.Accessibility;

    public string Name => this._accessorBuilder.Name;

    public bool IsPartial => this._accessorBuilder.IsPartial;

    public bool HasImplementation => !this._builtMember.IsAbstract;

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
    private IFullRef<IMethod> Ref => this.RefFactory.FromBuilt<IMethod>( this );

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    IRef<IMember> IMember.ToRef() => this.Ref;

    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => this.Ref;

    public IRef<IMethod> ToRef() => this.Ref;

    IRef<IMethodBase> IMethodBase.ToRef() => this.Ref;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.Definition;

    IMember IMember.Definition => this.Definition;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options ) => new MethodInvoker( this, options, target );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new MethodInvoker( this ).CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this._accessorBuilder.ReturnParameter, this.Compilation, this.GenericContext, this );

    [Memo]
    public IType ReturnType => this.MapType( this._accessorBuilder.ReturnParameter.Type );

    public ITypeParameterList TypeParameters => TypeParameterList.Empty;

    IReadOnlyList<IType> IGeneric.TypeArguments => [];

    public bool IsGeneric => false;

    public bool IsCanonicalGenericInstance => this.DeclaringType.IsCanonicalGenericInstance;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
        => throw new NotSupportedException( "Cannot add generic parameters to accessors." );

    [Memo]
    public IMethod? OverriddenMethod => this.MapDeclaration( this._accessorBuilder.OverriddenMethod );

    public INamedType DeclaringType => this._builtMember.DeclaringType;

    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this.MapDeclarationList( this._accessorBuilder.ExplicitInterfaceImplementations );

    public MethodInfo ToMethodInfo() => throw new NotImplementedException();

    IHasAccessors IMethod.DeclaringMember => (IHasAccessors) this._builtMember;

    public override IDeclaration ContainingDeclaration => this._builtMember;

    public MethodBase ToMethodBase() => CompileTimeMethodInfo.Create( this );

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    ExecutionScope IMemberOrNamedType.ExecutionScope => ExecutionScope.RunTime;

    [Memo]
    public IMember? OverriddenMember => this.MapDeclaration( this._accessorBuilder.OverriddenMember );

    public bool? IsIteratorMethod => this._accessorBuilder.IsIteratorMethod;

    public override bool CanBeInherited => this._builtMember.CanBeInherited;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
    {
        if ( !this.CanBeInherited )
        {
            return [];
        }
        else
        {
            return Member.GetDerivedDeclarationsCore( this, options );
        }
    }
}