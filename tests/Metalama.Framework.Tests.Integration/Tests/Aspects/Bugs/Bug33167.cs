using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33167;

internal class TestContract : ContractAspect
{
    public override void Validate( dynamic? value )
    {
        ((IMethod)meta.Target.Parameter.ContainingDeclaration!).Invoke(value);
    }
}

// <target>
internal class TestClass
{
    public void Method1( [TestContract] string nonNullableString )
    {
    }
}