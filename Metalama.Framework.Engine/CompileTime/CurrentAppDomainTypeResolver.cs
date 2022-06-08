// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// An implementation of <see cref="CompileTimeTypeResolver"/> that will resolve any type of an assembly that
/// is already loaded in the AppDomain.
/// </summary>
internal class CurrentAppDomainTypeResolver : CompileTimeTypeResolver
{
    public CurrentAppDomainTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected virtual bool IsSupportedAssembly( string assemblyName ) => true;

    protected virtual Type? GetWellKnownType( string typeName ) => null;

    protected virtual bool CanLoadTypeFromAssembly( AssemblyName assemblyName ) => true;

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

            if ( !this.IsSupportedAssembly( assemblyIdentity.Name ) )
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

            if ( !this.CanLoadTypeFromAssembly( assemblyName ) )
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