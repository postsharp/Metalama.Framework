// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeTypeResolver"/> that will resolve any type of an assembly that
    /// is already loaded in the AppDomain.
    /// </summary>
    internal class CurrentAppDomainTypeResolver : CompileTimeTypeResolver
    {
        public CurrentAppDomainTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider )
        {
        }

        protected virtual bool IsAcceptableAssemblyName( string assemblyName ) => true;

        protected virtual Type? GetWellKnownType(string typeName) => null;

        protected virtual bool CanLoadAssembly( AssemblyName assemblyName ) => true;

        protected override Type? GetCompileTimeNamedType( INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default )
        {
            if ( !this.Cache.TryGetValue( typeSymbol, out var type ) )
            {
                cancellationToken.ThrowIfCancellationRequested();

                type = this.GetCompileTimeNamedTypeCore( typeSymbol );

                this.Cache.Add( typeSymbol, type );
            }

            return type;
        }

        private Type? GetCompileTimeNamedTypeCore( INamedTypeSymbol typeSymbol )
        {
            var typeName = typeSymbol.GetReflectionName();

            if ( typeName == null )
            {
                return null;
            }

            if ( typeSymbol.ContainingAssembly != null )
            {
                var assemblyIdentity = typeSymbol.ContainingAssembly.Identity;

                if ( !this.IsAcceptableAssemblyName( assemblyIdentity.Name ) )
                {
                    return null;
                }

                var wellKnownType = this.GetWellKnownType( typeName );

                if ( wellKnownType != null )
                {
                    return wellKnownType;
                }

                // We don't allow loading new assemblies to the AppDomain.
                var assemblyName = new AssemblyName( assemblyIdentity.GetDisplayName() );

                if ( !this.CanLoadAssembly( assemblyName ) )
                {
                    // Coverage: ignore
                    return null;
                }

                typeName += ", " + assemblyIdentity.GetDisplayName();
            }

            var type = Type.GetType( typeName );

            return type;
        }
    }
    
    /// <summary>
    /// An implementation of <see cref="CompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : CurrentAppDomainTypeResolver, IService
    {
        // Avoid initializing from a static member because it is more difficult to debug.
        private readonly Assembly _netStandardAssembly = Assembly.Load( "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" );

        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SystemTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._referenceAssemblyLocator = serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();
        }

        protected override bool CanLoadAssembly( AssemblyName assemblyName ) => AppDomain.CurrentDomain.GetAssemblies().All( a => AssemblyName.ReferenceMatchesDefinition( assemblyName, a.GetName() ) );

        protected override bool IsAcceptableAssemblyName( string assemblyName ) => this._referenceAssemblyLocator.IsStandardAssemblyName( assemblyName );

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