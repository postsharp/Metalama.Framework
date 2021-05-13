﻿// @IncludeAllSeverities

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromInitialize
{
    public class ErrorAttribute : OverrideMethodAspect
    {
        public override void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.Diagnostics.Report(Caravela.Framework.Diagnostics.Severity.Error, "MY001", "Error");
            aspectBuilder.Diagnostics.Report(Caravela.Framework.Diagnostics.Severity.Warning, "MY002", "Warning");
            aspectBuilder.Diagnostics.Report(Caravela.Framework.Diagnostics.Severity.Info, "MY003", "Info");
            aspectBuilder.Diagnostics.Report(Caravela.Framework.Diagnostics.Severity.Hidden, "MY004", "Hidden");
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Error]
        public static int Add(int a, int b)
        {
            if (a == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            return a + b;
        }
    }
}
