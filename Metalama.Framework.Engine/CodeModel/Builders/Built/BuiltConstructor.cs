﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal sealed class BuiltConstructor : BuiltMethodBase, IConstructorImpl
{
    private readonly ConstructorBuilder _constructorBuilder;

    public BuiltConstructor( ConstructorBuilder constructorBuilder, CompilationModel compilation, IGenericContext genericContext ) : base(
        compilation,
        genericContext )
    {
        this._constructorBuilder = constructorBuilder;
    }

    public override DeclarationBuilder Builder => this._constructorBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this._constructorBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._constructorBuilder;

    protected override MemberBuilder MemberBuilder => this._constructorBuilder;

    protected override MethodBaseBuilder MethodBaseBuilder => this._constructorBuilder;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();

    [Memo]
    private IRef<IConstructor> Ref
        => (IRef<IConstructor>?) ((ICompilationBoundRefImpl?) this._constructorBuilder.ReplacedImplicitConstructor?.ToRef())?.WithGenericContext(
               this.GenericContext )
           ?? this.RefFactory.FromBuilt<IConstructor>( this );

    public IRef<IConstructor> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public ConstructorInitializerKind InitializerKind => this._constructorBuilder.InitializerKind;

    bool IConstructor.IsPrimary => false;

    public ConstructorInfo ToConstructorInfo() => this._constructorBuilder.ToConstructorInfo();

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