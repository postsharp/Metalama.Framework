// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.DesignTime
{
    internal static class DesignTimeExceptionHandler
    {
        // It is critical that OperationCanceledException is NOT handled, i.e. this exception should flow to the caller, otherwise VS will be satisfied
        // with the incomplete results it received, and cache them. 
        internal static bool MustHandle( Exception e )
            => e switch
            {
                OperationCanceledException => false,
                AggregateException aggregate when aggregate.InnerExceptions.Count == 0 && e.InnerException != null => MustHandle( e.InnerException ),
                _ => true
            };

        internal static void ReportException( Exception e )
        {
            if ( MustHandle( e ) )
            {
                Logger.Instance?.Write( e.ToString() );
            }
        }
    }
}