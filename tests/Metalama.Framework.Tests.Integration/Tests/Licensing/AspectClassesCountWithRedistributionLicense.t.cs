// --- AspectClassesCountWithRedistributionLicense.cs ---
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectClassesCountWithRedistributionLicense;
class Dummy
{
} // --- _Redistribution.cs ---
using  Metalama . Framework . Tests . Integration . Tests . Licensing . Redistribution . Dependency ;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution;
class RedistributionTargetClass
{
  [RedistributionAspect1]
  [RedistributionAspect2]
  [RedistributionAspect3]
  [RedistributionAspect4]
  void RedistributionTargetMethod()
  {
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect1");
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect2");
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect3");
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod() enhanced by RedistributionAspect4");
    return;
  }
}