using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.DeclarativeAdvice;


#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 
public class C
{
    
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 
private class F : TypeFabric
    {
        [Introduce]
[global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility=global::Metalama.Framework.Code.Accessibility.Private)]
public void M() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 



private void M()
{
}}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 
