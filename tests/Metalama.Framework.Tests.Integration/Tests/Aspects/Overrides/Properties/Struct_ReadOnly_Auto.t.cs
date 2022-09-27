internal readonly struct TargetStruct
{
  public TargetStruct()
  {
  }
  private readonly int _property = default;
  [Override]
  public int Property
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this._property;
    }
    private init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this._property = value;
    }
  }
  private static int _staticProperty = default;
  [Override]
  public static int StaticProperty
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Struct_ReadOnly_Auto.TargetStruct._staticProperty;
    }
    set
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Struct_ReadOnly_Auto.TargetStruct._staticProperty = value;
    }
  }
  private readonly int _propertyInitOnly = default;
  [Override]
  public int PropertyInitOnly
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this._propertyInitOnly;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this._propertyInitOnly = value;
    }
  }
  private readonly int _staticPropertyInitOnly = default;
  [Override]
  public int StaticPropertyInitOnly
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this._staticPropertyInitOnly;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this._staticPropertyInitOnly = value;
    }
  }
}