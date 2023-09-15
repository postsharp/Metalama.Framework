[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Project")]
public class C1
{
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Project")]
    public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Project")] int p)
    {
    }
}

[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
public class C2
{
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
    public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")] int p)
    {
    }

    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("M2")]
    public void M2([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("M2")] int p)
    {
    }

    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
    public int P {[OptionsAspect]
        [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
        get; [OptionsAspect]
        [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
        set; }

    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
    public int F;
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
    public event EventHandler? E;
    public class N
    {
        [OptionsAspect]
        [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")]
        public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("C2")] int p)
        {
        }
    }
}
