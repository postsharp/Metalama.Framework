// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime
{
    internal interface ISystemTypeResolverFactory : IService
    {
        SystemTypeResolver Create( CompilationServices compilationServices );
    }
    
    internal class SystemTypeResolverFactory : ISystemTypeResolverFactory
    {
        public virtual SystemTypeResolver Create( CompilationServices compilationServices ) => new SystemTypeResolver( compilationServices );
    }
    
    /// <summary>
    /// An implementation of <see cref="CompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : CurrentAppDomainTypeResolver
    {
        // Avoid initializing from a static member because it is more difficult to debug.
        private readonly Assembly _netStandardAssembly = Assembly.Load( "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" );

        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SystemTypeResolver( CompilationServices compilationServices ) : base( compilationServices )
        {
            this._referenceAssemblyLocator = compilationServices.ServiceProvider.GetRequiredService<ReferenceAssemblyLocator>();
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
    }
}