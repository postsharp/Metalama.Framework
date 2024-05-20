// --- _AspectInheritanceCrossAssembly.cs ---
using Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssembly.Dependency;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssembly;
internal class ImplementingClass : IInterfaceWithAspects
{
  public void TargetMethod()
  {
    global::System.Console.WriteLine("InheritableAspect1");
    global::System.Console.WriteLine("InheritableAspect2");
    global::System.Console.WriteLine("InheritableAspect3");
    global::System.Console.WriteLine("InheritableAspect4");
    return;
  }
}
// --- AspectInheritanceCrossAssemblyAllowed.cs ---
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssemblyAllowed;
class Dummy
{
}