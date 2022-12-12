// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Metalama.DotNetTools
{
    internal static class Program
    {
        private static async Task<int> Main( string[] args )
        {
            var servicesFactory = new CommandServiceProvider();

            try
            {
                var root = new TheRootCommand( servicesFactory );

                return await root.InvokeAsync( args );
            }
            catch ( Exception e )
            {
                try
                {
                    servicesFactory.ServiceProvider.GetService<IExceptionReporter>()?.ReportException( e );
                }
                catch ( Exception reporterException )
                {
                    throw new AggregateException( e, reporterException );
                }

                return -1;
            }
            finally
            {
                try
                {
                    // Report usage.
                    servicesFactory.ServiceProvider.GetService<IUsageReporter>()?.StopSession();

                    // Close logs.
                    // Logging has to be disposed as the last one, so it could be used until now.
                    servicesFactory.ServiceProvider.GetLoggerFactory().Dispose();
                }
                catch ( Exception e )
                {
                    try
                    {
                        servicesFactory.ServiceProvider.GetService<IExceptionReporter>()?.ReportException( e );
                    }
                    catch
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't want failing telemetry to disturb users.
                }
            }
        }
    }
}