using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Marks the declaration (and all children declarations) as compile-time for the template compiler.
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public class CompileTimeAttribute : Attribute
    {
    }
}