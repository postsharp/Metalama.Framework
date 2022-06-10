// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class AttributeClassificationService : IService
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
        var declarationFactory = attribute.GetCompilationModel().Factory;
        var templateAttributeType = declarationFactory.GetSpecialType( InternalSpecialType.TemplateAttribute );

        return !attribute.Type.Is( templateAttributeType ) && !attribute.Type.Name.Equals( nameof(DynamicAttribute) );
    }
}