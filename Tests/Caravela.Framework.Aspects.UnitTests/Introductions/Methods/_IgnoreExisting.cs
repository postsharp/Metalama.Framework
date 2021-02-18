using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Introductions.Methods.IgnoreExisting
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
        }

        [IntroduceMethod]
        public int ExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );
            return 42;
        }
    }

    #region Target
    [Introduction]
    internal class TargetClass
    {
        public int ExistingMethod()
        {
            return 13;
        }
    }
    #endregion
}
