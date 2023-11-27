internal interface Interface
{
  [Override]
  public static virtual Interface operator +(Interface a, Interface b)
  {
    global::System.Console.WriteLine("Override.");
    return a;
  }
}