﻿using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.TestApp
{
    class PrintDebugInfoAspect : OverrideMethodAspect
    {
        public override dynamic Template()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return TemplateContext.proceed();
        }
    }
}
