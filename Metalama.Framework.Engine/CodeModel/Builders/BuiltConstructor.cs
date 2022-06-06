// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class BuiltConstructor : BuiltMember, IConstructorImpl
{
    public ConstructorBuilder ConstructorBuilder { get; }

    public BuiltConstructor( ConstructorBuilder constructorBuilder, CompilationModel compilation ) : base( compilation )
    {
        this.ConstructorBuilder = constructorBuilder;
    }

    public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.ConstructorBuilder;

    public override MemberBuilder MemberBuilder => this.ConstructorBuilder;

    public IParameterList Parameters => ParameterList.Empty;

    public MethodKind MethodKind => this.ConstructorBuilder.MethodKind;

    public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

    public ConstructorInitializerKind InitializerKind => this.ConstructorBuilder.InitializerKind;

    public ConstructorInfo ToConstructorInfo() => throw new NotImplementedException();

    public IConstructor? GetBaseConstructor()
    {
        // Currently ConstructorBuilder is used to represent a default constructor, the base constructor is always
        // the default constructor of the base class.
        return this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );
    }
}