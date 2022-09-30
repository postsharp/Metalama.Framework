using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.ParamsConstructor;

[AttributeUsage( AttributeTargets.All, AllowMultiple = true )]
public class MyAttribute : Attribute
{
    public MyAttribute( params int[] x ) { }
}

public class MyAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create( typeof(MyAttribute) ) );

        builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create(
                typeof(MyAttribute),
                constructorArguments: new object[] { 1 } ),
            whenExists: OverrideStrategy.New );

        builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create(
                typeof(MyAttribute),
                constructorArguments: new object[] { 1, 2 } ),
            whenExists: OverrideStrategy.New );
    }
}

// <target>
internal class C
{
    [MyAspect]
    private void M() { }
}