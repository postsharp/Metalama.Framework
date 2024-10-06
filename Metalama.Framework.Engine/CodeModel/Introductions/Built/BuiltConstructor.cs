// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltConstructor : BuiltMember, IConstructorImpl
{
    private readonly ConstructorBuilderData _constructorBuilder;

    public BuiltConstructor( ConstructorBuilderData constructorBuilder, CompilationModel compilation, IGenericContext genericContext ) : base(
        compilation,
        genericContext )
    {
        this._constructorBuilder = constructorBuilder;
    }

    public override DeclarationBuilderData BuilderData => this._constructorBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this._constructorBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this._constructorBuilder;

    protected override MemberBuilderData MemberBuilder => this._constructorBuilder;

    public override bool IsExplicitInterfaceImplementation => throw new NotImplementedException();

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.Compilation.GetParameterCollection( this._constructorBuilder.ToRef() ) );

    public MethodBase ToMethodBase() => this.ToConstructorInfo();

    IRef<IMethodBase> IMethodBase.ToRef() => this.ToRef();

    [Memo]
    private IRef<IConstructor> Ref
        => (IRef<IConstructor>?) ((ICompilationBoundRefImpl?) this._constructorBuilder.ReplacedImplicitConstructor)?.WithGenericContext( this.GenericContext )
           ?? this.RefFactory.FromBuilt<IConstructor>( this );

    public IRef<IConstructor> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public ConstructorInitializerKind InitializerKind => this._constructorBuilder.InitializerKind;

    bool IConstructor.IsPrimary => false;

    public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

    [Memo]
    public IConstructor Definition => this.Compilation.Factory.GetConstructor( this._constructorBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public IConstructor? GetBaseConstructor()
        =>

            // Currently ConstructorBuilder is used to represent a default constructor, the base constructor is always
            // the default constructor of the base class.
            this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );

    public object Invoke( params object?[] args ) => new ConstructorInvoker( this ).Invoke( args );

    public object Invoke( IEnumerable<IExpression> args ) => new ConstructorInvoker( this ).Invoke( args );

    public IObjectCreationExpression CreateInvokeExpression() => new ConstructorInvoker( this ).CreateInvokeExpression();

    public IObjectCreationExpression CreateInvokeExpression( params object?[] args ) => new ConstructorInvoker( this ).CreateInvokeExpression( args );

    public IObjectCreationExpression CreateInvokeExpression( params IExpression[] args ) => new ConstructorInvoker( this ).CreateInvokeExpression( args );

    public IObjectCreationExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new ConstructorInvoker( this ).CreateInvokeExpression( args );
}