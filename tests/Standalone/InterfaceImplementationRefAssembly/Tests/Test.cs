// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Dependency;
using Xunit;

namespace Tests;

public partial class Test
{
    [Fact]
    public void IntroduceInterfaceTarget_ImplementsInterface()
    {
        var test = new IntroduceInterfaceTarget();

        Assert.True( test is IInterface );
    }

    [Fact]
    public void IntroduceInterfaceWithPrivateTemplatesTarget_ImplementsInterface()
    {
        var test = new IntroduceInterfaceWithPrivateTemplatesTarget();

        Assert.True( test is IInterface );
    }
}

[IntroduceInterface]
public class IntroduceInterfaceTarget
{

}

[IntroduceInterfaceWithPrivateTemplates]
public class IntroduceInterfaceWithPrivateTemplatesTarget
{

}