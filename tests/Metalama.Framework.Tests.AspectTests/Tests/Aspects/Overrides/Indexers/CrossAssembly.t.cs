[Override]
[Introduction]
internal class TargetClass
{
  public string this[string x]
  {
    get
    {
      global::System.Console.WriteLine("Override");
      Console.WriteLine("Original");
      return x;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      Console.WriteLine("Original");
      x = value;
      return;
    }
  }
  public dynamic? this[global::System.Int32 index]
  {
    get
    {
      global::System.Console.WriteLine("Override");
      global::System.Console.WriteLine("Introduced.");
      return default(dynamic? );
    }
    set
    {
      global::System.Console.WriteLine("Override");
      global::System.Console.WriteLine("Introduced.");
      return;
    }
  }
}