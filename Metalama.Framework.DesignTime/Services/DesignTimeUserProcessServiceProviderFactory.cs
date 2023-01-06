// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;

namespace Metalama.Framework.DesignTime.Services;

public class DesignTimeUserProcessServiceProviderFactory : DesignTimeServiceProviderFactory
{
    public DesignTimeUserProcessServiceProviderFactory() : this( null ) { }

    public DesignTimeUserProcessServiceProviderFactory( DesignTimeEntryPointManager? entryPointManager ) : base( entryPointManager ) { }
}