// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Reflection;

// ReSharper disable ClassCanBeSealed.Global

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// An implementation of <see cref="CompileTimeTypeResolver"/> that cannot be used for user-code attributes.
/// </summary>
internal class SystemTypeResolver : CurrentAppDomainTypeResolver
{
    // Avoid initializing from a static member because it is more difficult to debug.
    private readonly Assembly _netStandardAssembly = Assembly.Load( "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" );

    private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

    public SystemTypeResolver( in ProjectServiceProvider serviceProvider, CompilationContext compilationContext ) : base( serviceProvider, compilationContext )
    {
        this._referenceAssemblyLocator = serviceProvider.GetReferenceAssemblyLocator();
    }

    protected override bool CanLoadTypeFromAssembly( AssemblyName assemblyName )
        => AppDomainUtility.HasAnyLoadedAssembly( a => AssemblyName.ReferenceMatchesDefinition( assemblyName, a.GetName() ) );

    protected override bool IsSupportedAssembly( string assemblyName ) => this._referenceAssemblyLocator.IsStandardAssemblyName( assemblyName );

    protected override Type? GetWellKnownType( string typeName )
    {
        // Check if this is a system type. If yes, it does not need to be in the same assembly.
        var systemType = this._netStandardAssembly.GetType( typeName, false );

        if ( systemType != null )
        {
            return systemType;
        }
        else
        {
            return null;
        }
    }

    public new class Provider : CompilationServiceProvider<CompileTimeTypeResolver>
    {
        public Provider( in ProjectServiceProvider serviceProvider ) : base( serviceProvider ) { }

        protected override CompileTimeTypeResolver Create( CompilationContext compilationContext )
            => new SystemTypeResolver( this.ServiceProvider, compilationContext );
    }
}