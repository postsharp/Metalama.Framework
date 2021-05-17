// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
        static DiagnosticDefinition<ICodeElement> myWarning = new( "MY001", Severity.Warning, "Hello, {0} v22." );
        public override void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            base.Initialize( aspectBuilder );

            aspectBuilder.Diagnostics.Report( myWarning, aspectBuilder.TargetDeclaration );
        }
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return meta.Proceed();
        }
    }
}
