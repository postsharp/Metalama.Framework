using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.TemplateProvider_CrossAssembly;

[CompileTime]
public class Templates : ITemplateProvider
{
    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine($"static template i={i}");
    }
}