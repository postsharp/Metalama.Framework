using System;

namespace Caravela.Framework.Project
{
    
    /// <summary>
    /// Marks members that must be highlighted as "template keywords" in the IDE.
    /// </summary>
    public class TemplateKeywordAttribute : Attribute
    {
       
        // TODO: This attribute and the Proceed attribute could be merged into one, if this attribute has a parameter
        // that accepts the category, and Proceeds is its own category.    
    }
}