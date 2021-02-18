﻿using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Introductions.Methods.DeclarativeNonVoid
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
        }

        [IntroduceMethod]
        public int IntroducedMethod()
        {
            Console.WriteLine( "This is introduced method." );
            return proceed();
        }
    }

    #region Target
    [Introduction]
    internal class TargetClass
    {
    }
    #endregion
}
