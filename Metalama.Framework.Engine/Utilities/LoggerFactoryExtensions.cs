// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.Engine.Utilities;

internal static class LoggerFactoryExtensions
{
    public static ILogger DesignTime( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "DesignTime" );

    public static ILogger CompileTime( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "CompileTime" );
}