// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
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
                AggregateException { InnerExceptions.Count: 0 } when e.InnerException != null => MustHandle( e.InnerException ),
                _ => true
            };

        public static void ReportException( Exception e, ILogger? logger = null )
        {
            logger ??= Logger.DesignTime;

            if ( MustHandle( e ) )
            {
                logger.Error?.Log( e.ToString() );

                BackstageServiceFactory.ServiceProvider.GetBackstageService<IExceptionReporter>()?.ReportException( e );
            }
            else
            {
                logger.Warning?.Log( $"Got an acceptable exception {e.GetType().Name}." );
            }
        }
    }
}