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
    
    // TODO: Needs to be moved out of the public API.
    /// <summary>
    /// Can be used by the tests when they want to bypassing the aspect framework. 
    /// </summary>
    public sealed class TestTemplateAttribute : TemplateAttribute
    {
        
    }
}