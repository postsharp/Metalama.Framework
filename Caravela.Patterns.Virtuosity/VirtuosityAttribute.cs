using System;
using Caravela.Framework.Aspects;

namespace Caravela.Patterns.Virtuosity
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class VirtuosityAttribute : Attribute, IAspect { }
}
