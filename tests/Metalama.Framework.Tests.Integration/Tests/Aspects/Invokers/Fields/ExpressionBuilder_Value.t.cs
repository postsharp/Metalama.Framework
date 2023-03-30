internal class TargetClass
{
  public int F;
  [Test]
  public void Map(TargetClass source, TargetClass target)
  {
    target.F = ((global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.ExpressionBuilder_Value.TargetClass)source).F;
    return;
  }
}