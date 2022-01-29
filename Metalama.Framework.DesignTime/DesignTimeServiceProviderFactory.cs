// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime;

public static class DesignTimeServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider? _serviceProvider;

    public static ServiceProvider GetServiceProvider()
    {
        if ( _serviceProvider == null )
        {
            lock ( _initializeSync )
            {
                if ( _serviceProvider == null )
                {
                    _serviceProvider = ServiceProviderFactory.GetServiceProvider();

                    _serviceProvider = _serviceProvider
                        .WithService( new DesignTimeAspectPipelineFactory( _serviceProvider, new CompileTimeDomain() ) );
                }
            }
        }

        return _serviceProvider;
    }
}