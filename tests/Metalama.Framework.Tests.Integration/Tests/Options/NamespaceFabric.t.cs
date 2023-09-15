[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Namespace")]
public class C1
{
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Namespace")]
    public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Namespace")] int p)
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
}

[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute(default(global::System.String))]
public class C3
{
    [OptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute(default(global::System.String))]
    public void M([OptionsAspect][global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute(default(global::System.String))] int p)
    {
    }
}
