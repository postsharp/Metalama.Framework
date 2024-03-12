// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltConstructor : BuiltMethodBase, IConstructorImpl
{
    public BuiltConstructor( ConstructorBuilder constructorBuilder, CompilationModel compilation ) : base( constructorBuilder, compilation )
    {
        this.ConstructorBuilder = constructorBuilder;
    }

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.ConstructorBuilder;

    protected override MemberBuilder MemberBuilder => this.ConstructorBuilder;

    protected override MethodBaseBuilder MethodBaseBuilder => this.ConstructorBuilder;

    public override IParameterList Parameters => ParameterList.Empty;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();

    public ConstructorInitializerKind InitializerKind => this.ConstructorBuilder.InitializerKind;

    bool IConstructor.IsPrimary => false;

    public ConstructorBuilder ConstructorBuilder { get; }

    public ConstructorInfo ToConstructorInfo() => this.ConstructorBuilder.ToConstructorInfo();

    IConstructor IConstructor.Definition => this;

    public IConstructor? GetBaseConstructor()
    {
        // Currently ConstructorBuilder is used to represent a default constructor, the base constructor is always
        // the default constructor of the base class.
        return this.DeclaringType.BaseType?.Constructors.SingleOrDefault( c => c.Parameters.Count == 0 );
    }
}