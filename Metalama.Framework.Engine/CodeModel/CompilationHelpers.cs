// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class CompilationHelpers : ICompilationHelpers
    {
        public IteratorInfo GetIteratorInfo( IMethod method ) => method.GetIteratorInfoImpl();

        public AsyncInfo GetAsyncInfo( IMethod method ) => method.GetAsyncInfoImpl();

        public AsyncInfo GetAsyncInfo( IType type ) => type.GetAsyncInfoImpl();

        public string GetMetadataName( INamedType type ) => ((INamedTypeSymbol) ((INamedTypeInternal) type).TypeSymbol).GetReflectionName();

        public string GetFullMetadataName( INamedType type ) => ((INamedTypeSymbol) ((INamedTypeInternal) type).TypeSymbol).GetReflectionFullName();

        public SerializableTypeId GetSerializableId( IType type ) => type.GetSymbol().GetSerializableTypeId();

        public IExpression ToTypeOfExpression( IType type ) => new TypeOfUserExpression( type );

        public bool DerivesFrom( INamedType left, INamedType right, DerivedTypesOptions options = DerivedTypesOptions.Default )
        {
            if ( right.TypeDefinition != right )
            {
                throw new ArgumentOutOfRangeException( nameof(right), "The type must not be a generic type instance." );
            }
            
            // We do not include the right type itself.
            if ( left.TypeDefinition.Equals( right ) )
            {
                return false;
            }

            switch ( options )
            {
                case DerivedTypesOptions.All:
                    return IsEqualOrDerivesFromWithAnyDegree( left );

                case DerivedTypesOptions.DirectOnly:
                    return DerivesFromDirectly( left );

                case DerivedTypesOptions.FirstLevelWithinCompilationOnly:
                    return DerivesFromWithFirstLevel( left );

                default:
                    throw new ArgumentOutOfRangeException( nameof(options) );
            }

            bool IsEqualOrDerivesFromWithAnyDegree( INamedType type )
            {
                if ( type.Equals( right ) )
                {
                    return true;
                }

                if ( type.BaseType != null )
                {
                    if ( IsEqualOrDerivesFromWithAnyDegree( type.BaseType.TypeDefinition ) )
                    {
                        return true;
                    }
                }

                foreach ( var i in type.ImplementedInterfaces )
                {
                    if ( IsEqualOrDerivesFromWithAnyDegree( i.TypeDefinition ) )
                    {
                        return true;
                    }
                }

                return false;
            }

            bool DerivesFromDirectly( INamedType type )
            {
                if ( type.BaseType != null )
                {
                    var baseType = type.BaseType.TypeDefinition;

                    if ( baseType.Equals( right ) )
                    {
                        return true;
                    }
                }

                foreach ( var i in type.ImplementedInterfaces )
                {
                    if ( i.TypeDefinition.Equals( right ) )
                    {
                        return true;
                    }
                }

                return false;
            }

            bool DerivesFromWithFirstLevel( INamedType type )
            {
                if ( type.BaseType != null && !type.BaseType.DeclaringAssembly.Equals( type.DeclaringAssembly ) )
                {
                    if ( IsEqualOrDerivesFromWithAnyDegree( type.BaseType.TypeDefinition ) )
                    {
                        return true;
                    }
                }

                foreach ( var i in type.ImplementedInterfaces )
                {
                    if ( !i.DeclaringAssembly.Equals( type.DeclaringAssembly ) && IsEqualOrDerivesFromWithAnyDegree( i.TypeDefinition ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}