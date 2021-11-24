﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.TestApp.Aspects;

namespace Caravela.Framework.TestApp
{
    internal class PrintDebugInfoAspect : OverrideMethodAspect
    {
        static DiagnosticDefinition<IDeclaration> myWarning = new( "MY001", Severity.Warning, "Hello, {0} v23." );

        public override void BuildAspect(IAspectBuilder<IMethod> aspectBuilder )
        {
            base.BuildAspect( aspectBuilder );

            aspectBuilder.Diagnostics.Report( aspectBuilder.Target, myWarning, aspectBuilder.Target );
        }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return meta.Proceed();
        }
    }
}
