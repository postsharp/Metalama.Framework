using System.Linq;
using System.Reflection;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.TestFramework.GlobalAttributes;

internal class TheAspect : TypeAspect
{
    [Introduce]
    public string Product
        => meta.Target.Compilation.Attributes.OfAttributeType( typeof(AssemblyProductAttribute) ).First().ConstructorArguments.First().Value.ToString();
}

// <target>
[TheAspect]
internal class C { }