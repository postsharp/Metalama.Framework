// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : CompileTimeTypeResolver, IService
    {
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SystemTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._referenceAssemblyLocator = serviceProvider.GetService<ReferenceAssemblyLocator>();
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

            if ( typeSymbol.ContainingAssembly != null )
            {
                var assemblyName = typeSymbol.ContainingAssembly.Name;

                // We load only system assemblies, not user assemblies loaded in the AppDomain.
                if ( !this.IsStandardAssemblyName( assemblyName ) )
                {
                    return null;
                }

                // We don't allow loading new assemblies to the AppDomain except.
                if ( AppDomain.CurrentDomain.GetAssemblies().All( a => a.GetName().Name != assemblyName ) )
                {
                    if ( assemblyName == "System.Runtime" )
                    {
                        assemblyName = "mscorlib";
                    }
                    else
                    {
                        // Coverage: ignore
                        return null;
                    }
                }

                typeName += ", " + assemblyName;
            }

            var type = Type.GetType( typeName );

            return type;
        }
    }
}