using System;

namespace Caravela.Framework.Project
{
    [AttributeUsage(AttributeTargets.All)]
    public class CompileTimeOnlyAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class ProceedAttribute : Attribute
    {
    }
}