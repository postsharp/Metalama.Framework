﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
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
    private readonly ReferenceAssemblyLocator _assemblyLocator;

    public CurrentAppDomainTypeResolver( ProjectServiceProvider serviceProvider, CompilationContext compilationContext ) : base( compilationContext )
    {
        this._assemblyLocator = serviceProvider.GetReferenceAssemblyLocator();
    }

    protected virtual bool IsSupportedAssembly( string assemblyName ) => true;

    protected virtual Type? GetWellKnownType( string typeName ) => null;

    protected virtual bool CanLoadTypeFromAssembly( AssemblyName assemblyName ) => true;

    protected override Type? GetCompileTimeNamedType( INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default )
    {
        return this.Cache.GetOrAdd( typeSymbol, ( t ) => this.GetCompileTimeNamedTypeCore( (INamedTypeSymbol) t, CancellationToken.None ) );
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

            // If we have a system assembly, we must load the current version of the assembly, not the version required by the client,
            // so that we can later cast an instance of this type to types of our own assembly versions.
            if ( this._assemblyLocator.StandardAssemblyIdentities.TryGetValue( assemblyIdentity.Name, out var currentVersionAssemblyIdentity ) )
            {
                assemblyIdentity = currentVersionAssemblyIdentity;
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