using System;

namespace Caravela.Framework.Tests.Integration.Tests.Linker
{
    [AttributeUsage(AttributeTargets.All)]
    public class PseudoReplacement : Attribute
    {
        public PseudoReplacement(string targetMember)
        {
        }
    }
}
