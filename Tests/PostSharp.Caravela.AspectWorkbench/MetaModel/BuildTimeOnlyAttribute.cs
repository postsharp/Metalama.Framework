using System;

namespace Caravela.AspectWorkbench
{
    [AttributeUsage(AttributeTargets.All)]
    public class BuildTimeOnlyAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class ProceedAttribute : Attribute
    {
    }
}