internal record struct TargetRecordStruct
{
  private int Method1(int a)
  {
    global::System.Console.WriteLine("overridden");
    return a;
  }
  private string Method2(string s)
  {
    global::System.Console.WriteLine("overridden");
    return s;
  }
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
  private class Fabric : TypeFabric
  {
    public override void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
    [Template]
    [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility = global::Metalama.Framework.Code.Accessibility.Private)]
    public dynamic? Template() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823
}