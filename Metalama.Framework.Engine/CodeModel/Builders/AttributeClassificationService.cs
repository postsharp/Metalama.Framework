// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class AttributeClassificationService : IProjectService
{
    private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _cache = new( SymbolEqualityComparer.Default );

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

    public bool MustCopyTemplateAttribute( IAttribute attribute )
    {
        if ( attribute.Type.FullName.StartsWith( "Metalama.Framework.Aspects.", StringComparison.Ordinal ) ||
             attribute.Type.FullName.Equals( "System.Runtime.CompilerServices.NullableAttribute", StringComparison.Ordinal ) )
        {
            return false;
        }

        var declarationFactory = attribute.GetCompilationModel().Factory;
        var templateAttributeType = declarationFactory.GetSpecialType( InternalSpecialType.ITemplateAttribute );

        return !attribute.Type.Is( templateAttributeType ) && !attribute.Type.Name.Equals( nameof(DynamicAttribute), StringComparison.Ordinal );
    }
}