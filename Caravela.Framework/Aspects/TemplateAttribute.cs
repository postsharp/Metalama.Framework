using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// The base class for all custom attributes that mark a declaration as a template.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public abstract class TemplateAttribute : Attribute
    {
        // Prevents instantiation by users.
        internal TemplateAttribute()
        {
            
        }
    }
}