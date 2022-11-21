[Test]
internal class TargetClass
{
  public int this[int x]
  {
    get
    {
      global::System.Console.WriteLine("Override fourth");
      var x_4 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
    }
    set
    {
      global::System.Console.WriteLine("Override fourth");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
    }
  }
  private int this[int x, global::Metalama.Framework.RunTime.Source __linker_param]
  {
    get
    {
      Console.WriteLine("Original");
      return x;
    }
    set
    {
      Console.WriteLine("Original");
    }
  }
  private global::System.Int32 this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override first");
      var x_1 = this[x, default(global::Metalama.Framework.RunTime.Source)];
      return this[x, default(global::Metalama.Framework.RunTime.Source)];
    }
    set
    {
      global::System.Console.WriteLine("Override first");
      this[x, default(global::Metalama.Framework.RunTime.Source)] = value;
      this[x, default(global::Metalama.Framework.RunTime.Source)] = value;
    }
  }
  private global::System.Int32 this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override second");
      var x_2 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override second");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)] = value;
    }
  }
  private global::System.Int32 this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override third");
      var x_3 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
    }
    set
    {
      global::System.Console.WriteLine("Override third");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
    }
  }
  public string this[string x]
  {
    get
    {
      global::System.Console.WriteLine("Override fourth");
      var x_4 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
    }
    set
    {
      global::System.Console.WriteLine("Override fourth");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
    }
  }
  private string this[string x, global::Metalama.Framework.RunTime.Source __linker_param]
  {
    get
    {
      Console.WriteLine("Original");
      return x;
    }
    set
    {
      Console.WriteLine("Original");
    }
  }
  private global::System.String this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override first");
      var x_1 = this[x, default(global::Metalama.Framework.RunTime.Source)];
      return this[x, default(global::Metalama.Framework.RunTime.Source)];
    }
    set
    {
      global::System.Console.WriteLine("Override first");
      this[x, default(global::Metalama.Framework.RunTime.Source)] = value;
      this[x, default(global::Metalama.Framework.RunTime.Source)] = value;
    }
  }
  private global::System.String this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override second");
      var x_2 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override second");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute>)] = value;
    }
  }
  private global::System.String this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override third");
      var x_3 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
    }
    set
    {
      global::System.Console.WriteLine("Override third");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable.TestAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
    }
  }
}