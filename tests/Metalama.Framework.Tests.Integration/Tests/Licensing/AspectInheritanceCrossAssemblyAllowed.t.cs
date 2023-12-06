// --- AspectInheritanceCrossAssemblyAllowed.cs ---
// Error LAMA0810 on ``: `Accessing the Roslyn API via Metalama.Framework.Sdk package is not covered by your license.`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssemblyAllowed;
class Dummy
{
} // --- _AspectInheritanceCrossAssembly.cs ---
// Error LAMA0810 on ``: `Accessing the Roslyn API via Metalama.Framework.Sdk package is not covered by your license.`
using  Metalama . Framework . Aspects ;  using  Metalama . Framework . Code ;  using  Metalama . Framework . Tests . Integration . Tests . Licensing . AspectInheritanceCrossAssembly . Dependency ;  using  System ;
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