﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace Metalama.Framework.Engine.CodeModel.Introductions.Helpers;

#pragma warning disable CA1822 // Mark members as static

internal sealed class AttributeClassificationService : IGlobalService, IDisposable
{
    private readonly WeakCache<INamedTypeSymbol, bool> _cache = new();

    public bool MustMoveFromFieldToProperty( INamedTypeSymbol attributeType ) => this._cache.GetOrAdd( attributeType, MustMoveFromFieldToPropertyCore );

    private static bool MustMoveFromFieldToPropertyCore( INamedTypeSymbol type ) => (GetAttributeTargets( type ) & AttributeTargets.Property) != 0;

    private static AttributeTargets GetAttributeTargets( INamedTypeSymbol type )
    {
        var attributeUsageAttribute = type.GetAttributes().FirstOrDefault( a => a.AttributeClass is { Name: nameof(AttributeUsageAttribute) } );

        if ( attributeUsageAttribute == null )
        {
            if ( type.BaseType == null )
            {
                return AttributeTargets.All;
            }
            else
            {
                return GetAttributeTargets( type.BaseType );
            }
        }
        else
        {
            return (AttributeTargets) Enum.ToObject( typeof(AttributeTargets), (int) attributeUsageAttribute.ConstructorArguments[0].Value! );
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool MustCopyTemplateAttribute( IAttribute attribute )
    {
        var fullName = attribute.Type.FullName;

        if ( IsCompilerOrMetalamaAttribute( fullName ) )
        {
            return false;
        }

        var templateAttributeType = attribute.GetCompilationModel().Cache.ITemplateAttributeType;

        return !attribute.Type.Is( templateAttributeType ) && !attribute.Type.Name.Equals( nameof(DynamicAttribute), StringComparison.Ordinal );
    }

    public bool MustCopyTemplateAttribute( AttributeData attribute )
    {
        var fullName = attribute.AttributeConstructor.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes )
            .ContainingType.GetFullName()
            .AssertNotNull();

        return !IsCompilerOrMetalamaAttribute( fullName );
    }

    private static bool IsCompilerOrMetalamaAttribute( string fullAttributeName )
    {
        if ( fullAttributeName.StartsWith( "Metalama.Framework.Aspects.", StringComparison.Ordinal ) ||
             fullAttributeName is "System.Runtime.CompilerServices.NullableAttribute" or
                 "System.Runtime.CompilerServices.NullableContextAttribute" or
                 "System.Runtime.CompilerServices.CompilerGeneratedAttribute" or
                 "System.Runtime.CompilerServices.AsyncStateMachineAttribute" or
                 "System.Runtime.CompilerServices.IteratorStateMachineAttribute" or
                 "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute" or
                 "System.Diagnostics.DebuggerBrowsableAttribute" or
                 "System.Diagnostics.DebuggerStepThroughAttribute" )
        {
            return true;
        }

        return false;
    }

#pragma warning disable CA1822 // Mark members as static
    public bool IsCompilerRecognizedAttribute( INamedTypeSymbol attributeType )
#pragma warning restore CA1822 // Mark members as static
    {
        if ( attributeType.GetFullName() == "System.Runtime.CompilerServices.EnumeratorCancellationAttribute" )
        {
            return true;
        }

        return false;
    }

    public void Dispose() => this._cache.Dispose();
}