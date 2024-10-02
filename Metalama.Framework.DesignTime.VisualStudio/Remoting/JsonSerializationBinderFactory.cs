// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Rpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal static class JsonSerializationBinderFactory
{
    public static JsonSerializationBinder Instance { get; } = new(
        configuration =>
        {
            configuration.AddAssemblyOfType( typeof(IAspect) );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.0.1" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.4.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.8.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.12.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.0.1" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.4.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.8.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.12.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.0.1" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.4.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.8.0" );
            configuration.AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.12.0" );
        } );
}