using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.AutomaticPropertyTemplateUnexpected_CrossProject;

// <target>
internal class C
{
    [TheAspect]
    private int P { get; set; }
}