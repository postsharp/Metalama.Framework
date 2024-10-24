using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibtyForIntroductions_CrossAssembly;

public class MyAspect : TypeAspect
{
    [Introduce]
    private void Private() { }

    [Introduce]
    protected void Protected() { }

    [Introduce]
    internal void Internal() { }

    [Introduce]
    public void Public() { }

    [Introduce]
    private protected void PrivateProtected() { }

    [Introduce]
    protected internal void ProtectedInternal() { }
}