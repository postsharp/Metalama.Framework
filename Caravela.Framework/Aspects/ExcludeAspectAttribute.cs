using System;

namespace Caravela.Framework.Aspects
{
    public class ExcludeAspectAttribute : Attribute
    {
        public ExcludeAspectAttribute( params Type[] exludedAspectTypes )
        {
        }
    }
}