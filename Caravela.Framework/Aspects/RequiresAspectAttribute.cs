using System;

namespace Caravela.Framework.Aspects
{
    public sealed class RequiresAspectAttribute : Attribute
    {
        public RequiresAspectAttribute( params Type[] requiredAspectTypes )
        {
        }
    }
}