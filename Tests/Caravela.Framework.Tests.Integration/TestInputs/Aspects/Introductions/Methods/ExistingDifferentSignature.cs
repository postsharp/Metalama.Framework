using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingDifferentSignature
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
        public int ExistingMethod(int x)
        {
            return x;
        }
    }
    #endregion
}
