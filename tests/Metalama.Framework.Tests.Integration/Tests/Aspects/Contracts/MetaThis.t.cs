internal class Target
{
  private int MaybeBuff([ActionSpeed] int speed)
  {
    if (this is global::Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.MetaThis.IBuffable)
    {
      speed = speed * 2;
    }
    return speed;
  }
}