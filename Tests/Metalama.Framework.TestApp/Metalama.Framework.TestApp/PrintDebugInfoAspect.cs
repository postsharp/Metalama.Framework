// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.TestApp.Aspects;

namespace Metalama.Framework.TestApp
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
