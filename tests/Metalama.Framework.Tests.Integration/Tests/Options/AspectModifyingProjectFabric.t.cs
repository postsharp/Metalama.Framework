[OptionsAspect]
[ModifyOptionsAspect("FromAspect.C1")]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.Project->FromAspect.C1")]
public class C1
{
  [OptionsAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.Project->FromAspect.C1")]
  public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.Project->FromAspect.C1")] int p)
  {
  }
}
[OptionsAspect]
[ModifyOptionsAspect("FromAspect.C2")]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
public class C2
{
  [OptionsAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
  public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")] int p)
  {
  }
  [OptionsAspect]
  [ModifyOptionsAspect("FromAspect.P")]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.M2")]
  public void M2([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.M2")] int p)
  {
  }
  [OptionsAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
  public int P {[OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
    get; [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
    set; }
  [OptionsAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
  public int F;
  [OptionsAspect]
  [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
  public event EventHandler? E;
  public class N
  {
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")]
    public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("FromFabric.C2")] int p)
    {
    }
  }
}