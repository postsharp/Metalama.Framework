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
    /// An implementation of <see cref="CompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : CompileTimeTypeResolver, IService
    {
        // Avoid initializing from a static member because it is more difficult to debug.
        private readonly Assembly _netStandardAssembly = Assembly.Load( "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" );
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SystemTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._referenceAssemblyLocator = serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();
        }

        protected virtual bool IsStandardAssemblyName( string assemblyName ) => this._referenceAssemblyLocator.IsStandardAssemblyName( assemblyName );

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

                // We load only system assemblies, not user assemblies loaded in the AppDomain.
                if ( !this.IsStandardAssemblyName( assemblyIdentity.Name ) )
                {
                    return null;
                }

                // Check if this is a system type. If yes, it does not need to be in the same assembly.
                var systemType = this._netStandardAssembly.GetType( typeName, false );

                if ( systemType != null )
                {
                    return systemType;
                }

                // We don't allow loading new assemblies to the AppDomain.
                var assemblyName = new AssemblyName( assemblyIdentity.GetDisplayName() );

                if ( AppDomain.CurrentDomain.GetAssemblies().All( a => AssemblyName.ReferenceMatchesDefinition( assemblyName, a.GetName() ) ) )
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
}