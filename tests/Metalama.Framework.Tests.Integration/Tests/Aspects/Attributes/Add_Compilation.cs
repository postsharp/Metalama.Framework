using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Compilation;

[assembly: MyAspect]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Compilation;

public class MyAttribute : Attribute { }

public class MyAspect : CompilationAspect
{
    public override void BuildAspect( IAspectBuilder<ICompilation> builder )
    {
        builder.Advice.AddAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );
    }
}