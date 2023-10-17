// --- RoslynAllowed.cs ---
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RoslynAllowed;
class Dummy
{
}
// --- _Roslyn.cs ---
public class TargetClass
{
  [Log]
  public static void TargetMethod()
  {
    global::System.Console.WriteLine("Starting TargetMethod, doc ID M:Metalama.Framework.Tests.Integration.Tests.Licensing.Roslyn.TargetClass.TargetMethod.");
    return;
  }
}