// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        if ( !this.Cache.TryGetValue( typeSymbol, out var typeBox ) )
        {
            cancellationToken.ThrowIfCancellationRequested();

            typeBox = this.Cache.GetOrAdd(
                typeSymbol,
                ( t, ct ) => new StrongBox<Type?>( this.GetCompileTimeNamedTypeCore( (INamedTypeSymbol) t, ct ) ),
                cancellationToken );
        }

        return typeBox.Value;
    }

    private Type? GetCompileTimeNamedTypeCore( INamedTypeSymbol typeSymbol, CancellationToken cancellationToken )
    {
        var typeName = typeSymbol.GetReflectionName();

        if ( typeName == null )
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

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