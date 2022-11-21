[Introduction]
[Override]
internal class TargetClass
{
  private dynamic this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Introduced");
      return default(dynamic);
    }
    set
    {
      global::System.Console.WriteLine("Introduced");
    }
  }
  private dynamic this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Introduced");
      return default(dynamic);
    }
    set
    {
      global::System.Console.WriteLine("Introduced");
    }
  }
  private dynamic this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override first");
      var x_1 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override first");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)] = value;
    }
  }
  private dynamic this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override second");
      var x_2 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override second");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)] = value;
    }
  }
  private dynamic this[global::System.Int32 x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override third");
      var x_3 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
    }
    set
    {
      global::System.Console.WriteLine("Override third");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
    }
  }
  private dynamic this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override first");
      var x_1 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override first");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.IntroductionAttribute>)] = value;
    }
  }
  private dynamic this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override second");
      var x_2 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)];
    }
    set
    {
      global::System.Console.WriteLine("Override second");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute>)] = value;
    }
  }
  private dynamic this[global::System.String x, global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2> __linker_param]
  {
    get
    {
      global::System.Console.WriteLine("Override third");
      var x_3 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)];
    }
    set
    {
      global::System.Console.WriteLine("Override third");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._1>)] = value;
    }
  }
  public dynamic this[global::System.Int32 x]
  {
    get
    {
      global::System.Console.WriteLine("Override fourth");
      var x_4 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
    }
    set
    {
      global::System.Console.WriteLine("Override fourth");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
    }
  }
  public dynamic this[global::System.String x]
  {
    get
    {
      global::System.Console.WriteLine("Override fourth");
      var x_4 = this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
      return this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)];
    }
    set
    {
      global::System.Console.WriteLine("Override fourth");
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
      this[x, default(global::Metalama.Framework.RunTime.OverriddenBy<global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineableIntroduced.OverrideAttribute, global::Metalama.Framework.RunTime.OverrideOrdinal._2>)] = value;
    }
  }
}