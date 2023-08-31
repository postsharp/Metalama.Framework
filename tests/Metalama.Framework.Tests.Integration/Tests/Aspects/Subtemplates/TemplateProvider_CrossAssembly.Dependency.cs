using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.TemplateProvider_CrossAssembly;

[CompileTime]
public class Templates : ITemplateProvider
{
    [Template]
    public void Template( [CompileTime] int i )
    {
        Console.WriteLine( $"static template i={i}" );
    }
}