// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeExceptionHandler
    {
        internal static void ReportException( Exception e )
        {
            if ( e is not OperationCanceledException )
            {
                Logger.Instance?.Write( e.ToString() );   
            }
        }
    }
}