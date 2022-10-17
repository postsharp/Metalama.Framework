internal class TargetClass
{
  private int _property = 42;
  [Override]
  public int Property
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this._property;
    }
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this._property = value;
    }
  }
  private static int _staticProperty = 42;
  [Override]
  public static int StaticProperty
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_Initializers.TargetClass._staticProperty;
    }
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_Initializers.TargetClass._staticProperty = value;
    }
  }
}