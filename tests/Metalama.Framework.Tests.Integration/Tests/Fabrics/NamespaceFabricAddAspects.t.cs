using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspects
{
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
  internal class Fabric : NamespaceFabric
  {
    public override void AmendNamespace(INamespaceAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
  internal class Aspect : OverrideMethodAspect
  {
    public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
  internal class TargetCode
  {
    private int Method1(int a) => a;
    private string Method2(string s)
    {
      global::System.Console.WriteLine("overridden");
      return s;
    }
  }
  namespace Sub
  {
    internal class AnotherClass
    {
      private int Method1(int a) => a;
      private string Method2(string s)
      {
        global::System.Console.WriteLine("overridden");
        return s;
      }
    }
  }
}