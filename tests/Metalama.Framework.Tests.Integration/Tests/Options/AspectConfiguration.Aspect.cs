// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Options;
using System;

namespace Doc.AspectConfiguration
{
    // The aspect itself, consuming the configuration.
    public class LogAttribute : OverrideMethodAspect, IHierarchicalOptionsProvider<LoggingOptions>
    {
        public string? Category { get; init; }

        public override dynamic? OverrideMethod()
        {
            var options = meta.AspectInstance.GetOptions<LoggingOptions>();

            var message = $"{options.Category}: Executing {meta.Target.Method}.";
            Console.WriteLine( message );
           
            return meta.Proceed();
        }

        public LoggingOptions GetOptions() => new() { Category = this.Category };
    }
}