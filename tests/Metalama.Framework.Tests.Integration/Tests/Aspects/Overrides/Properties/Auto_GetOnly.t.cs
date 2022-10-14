internal class TargetClass : BaseClass
{
  private readonly int _property;
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
  private static int _staticProperty;
  [Override]
  public static int StaticProperty
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty;
    }
    private set
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty = value;
    }
  }
  private readonly int _initializerProperty = 42;
  [Override]
  public int InitializerProperty
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this._initializerProperty;
    }
    private init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this._initializerProperty = value;
    }
  }
  private static int _staticInitializerProperty = 42;
  [Override]
  public static int StaticInitializerProperty
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializerProperty;
    }
    private set
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializerProperty = value;
    }
  }
  [Override]
  public override int AbstractBaseProperty
  {
    get
    {
      return this.AbstractBaseProperty_Override;
    }
  }
  private int AbstractBaseProperty_Source { get; init; }
  private global::System.Int32 AbstractBaseProperty_Override
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this.AbstractBaseProperty_Source;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this.AbstractBaseProperty_Source = value;
    }
  }
  [Override]
  public override int AbstractBaseInitializerProperty
  {
    get
    {
      return this.AbstractBaseInitializerProperty_Override;
    }
  }
  private int AbstractBaseInitializerProperty_Source { get; init; } = 42;
  private global::System.Int32 AbstractBaseInitializerProperty_Override
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this.AbstractBaseInitializerProperty_Source;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this.AbstractBaseInitializerProperty_Source = value;
    }
  }
  [Override]
  public override int VirtualBaseProperty
  {
    get
    {
      return this.VirtualBaseProperty_Override;
    }
  }
  private int VirtualBaseProperty_Source { get; init; }
  private global::System.Int32 VirtualBaseProperty_Override
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this.VirtualBaseProperty_Source;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this.VirtualBaseProperty_Source = value;
    }
  }
  [Override]
  public override int VirtualBaseInitializerProperty
  {
    get
    {
      return this.VirtualBaseInitializerProperty_Override;
    }
  }
  private int VirtualBaseInitializerProperty_Source { get; init; } = 42;
  private global::System.Int32 VirtualBaseInitializerProperty_Override
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      return this.VirtualBaseInitializerProperty_Source;
    }
    init
    {
      global::System.Console.WriteLine($"This is the overridden setter.");
      this.VirtualBaseInitializerProperty_Source = value;
    }
  }
  public TargetClass()
  {
    this.Property = 27;
    this.InitializerProperty = 27;
    this.AbstractBaseProperty_Override = 27;
    this.AbstractBaseInitializerProperty_Override = 27;
    this.VirtualBaseProperty_Override = 27;
    this.VirtualBaseInitializerProperty_Override = 27;
  }
  static TargetClass()
  {
    StaticProperty = 27;
    StaticInitializerProperty = 27;
  }
}