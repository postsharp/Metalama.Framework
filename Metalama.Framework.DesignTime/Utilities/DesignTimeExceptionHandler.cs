// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.DesignTime.Utilities
{
    public static class DesignTimeExceptionHandler
    {
        // It is critical that OperationCanceledException is NOT handled, i.e. this exception should flow to the caller, otherwise VS will be satisfied
        // with the incomplete results it received, and cache them. 
        internal static bool MustHandle( Exception e )
            => e switch
            {
                OperationCanceledException => false,
                AggregateException { InnerException: not null } => MustHandle( e.InnerException ),
                _ => true
            };

        public static void ReportException( Exception e, IExceptionReporter? exceptionReporter, ILogger? logger = null )
        {
            logger ??= Logger.DesignTime;

            if ( MustHandle( e ) )
            {
                logger.Error?.Log( e.ToString() );

                exceptionReporter ??= BackstageServiceFactory.ServiceProvider.GetBackstageService<IExceptionReporter>();

                exceptionReporter?.ReportException( e );
            }
            else
            {
                logger.Warning?.Log( $"Got an acceptable exception {e.GetType().Name}." );
            }
        }
    }
}