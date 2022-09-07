using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Compilation;

[assembly: MyAspect]
[assembly:global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Compilation.MyAttribute]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Compilation;

public class MyAttribute : Attribute { }


#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 
public class MyAspect : CompilationAspect
{
    public override void BuildAspect(IAspectBuilder<ICompilation> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823 
