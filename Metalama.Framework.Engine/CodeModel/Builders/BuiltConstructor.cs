﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltConstructor : BuiltMethodBase, IConstructorImpl
{
    private readonly ConstructorBuilder _constructorBuilder;

    public BuiltConstructor( CompilationModel compilation, ConstructorBuilder constructorBuilder ) : base( compilation )
    {
        this._constructorBuilder = constructorBuilder;
    }

    public override DeclarationBuilder Builder => this._constructorBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this._constructorBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this._constructorBuilder;

    protected override MemberBuilder MemberBuilder => this._constructorBuilder;

    protected override MethodBaseBuilder MethodBaseBuilder => this._constructorBuilder;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();

    public ConstructorInitializerKind InitializerKind => this._constructorBuilder.InitializerKind;

    bool IConstructor.IsPrimary => false;

    public ConstructorInfo ToConstructorInfo() => this._constructorBuilder.ToConstructorInfo();

    IConstructor IConstructor.Definition => this;

    public IConstructor? GetBaseConstructor()
        =>

            // Currently ConstructorBuilder is used to represent a default constructor, the base constructor is always
            // the default constructor of the base class.
            this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );
}