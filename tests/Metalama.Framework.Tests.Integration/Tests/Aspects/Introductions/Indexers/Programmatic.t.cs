[Introduction]
internal class TargetClass
{
  public dynamic? this[global::System.Int32 x]
  {
    get
    {
      global::System.Console.WriteLine("Introduced");
      return default(dynamic? );
    }
    set
    {
      global::System.Console.WriteLine("Introduced");
    }
  }
}