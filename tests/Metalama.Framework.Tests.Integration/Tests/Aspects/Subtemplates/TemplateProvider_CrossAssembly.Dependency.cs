using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.TemplateProvider_CrossAssembly;

[CompileTime]
[TemplateProvider]
public class Templates
{
    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine($"static template i={i}");
    }
}