abstract class Base
{
  public abstract void M([NotNull] string m);
}
class Target : Base
{
  public override void M(string m)
  {
    if (m == null)
    {
      throw new global::System.ArgumentNullException("m");
    }
  }
}