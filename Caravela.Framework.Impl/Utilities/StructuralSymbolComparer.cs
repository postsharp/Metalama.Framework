// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Compares symbols, possibly from different compilations.
    /// </summary>
    internal class StructuralSymbolComparer : IEqualityComparer<ISymbol>
    {
        // TODO: At this point the default display string seems to be enough for comparison.

        public static readonly StructuralSymbolComparer Default =
            new(
                StructuralSymbolComparerOptions.ContainingElement |
                StructuralSymbolComparerOptions.Name |
                StructuralSymbolComparerOptions.GenericParameterCount |
                StructuralSymbolComparerOptions.ParameterTypes |
                StructuralSymbolComparerOptions.ParameterModifiers
            );

        public static readonly StructuralSymbolComparer Signature =
            new(
                StructuralSymbolComparerOptions.Name |
                StructuralSymbolComparerOptions.GenericParameterCount |
                StructuralSymbolComparerOptions.ParameterTypes |
                StructuralSymbolComparerOptions.ParameterModifiers
            );

        private readonly StructuralSymbolComparerOptions _options;

        public StructuralSymbolComparer( StructuralSymbolComparerOptions options )
        {
            this._options = options;
        }

        public bool Equals( ISymbol x, ISymbol y )
        {
            if ( x.Kind != y.Kind )
            {
                // Inequal kinds.
                return false;
            }

            switch ( (x, y) )
            {
                case (IMethodSymbol methodX, IMethodSymbol methodY):
                    if ( !MethodEquals( methodX, methodY, this._options ) )
                    {
                        return false;
                    }

                    break;

                case (INamedTypeSymbol namedTypeX, INamedTypeSymbol namedTypeY):
                    if ( !NamedTypeEquals( namedTypeX, namedTypeY, this._options ) )
                    {
                        return false;
                    }

                    break;

                case (INamespaceSymbol namespaceX, INamespaceSymbol namespaceY):
                    if ( !StringComparer.Ordinal.Equals( namespaceX.Name, namespaceY.Name ) )
                    {
                        return false;
                    }

                    break;

                default:
                    throw new NotImplementedException( $"{x.Kind}" );
            }

            if ( this._options.HasFlag( StructuralSymbolComparerOptions.ContainingElement )
                 && !ContainingElementEquals( x.ContainingSymbol, y.ContainingSymbol ) )
            {
                return false;
            }

            if ( this._options.HasFlag( StructuralSymbolComparerOptions.ContainingAssembly )
                 && !ContainingModuleEquals( x.ContainingModule, y.ContainingModule ) )
            {
                return false;
            }

            return true;
        }

        private static bool NamedTypeEquals( INamedTypeSymbol namedTypeX, INamedTypeSymbol namedTypeY, StructuralSymbolComparerOptions options )
        {
            if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) && !StringComparer.Ordinal.Equals( namedTypeX.Name, namedTypeY.Name ) )
            {
                return false;
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.GenericParameterCount )
                 && namedTypeX.TypeParameters.Length != namedTypeY.TypeParameters.Length )
            {
                return false;
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.GenericArguments ) )
            {
                // TODO: optimize using for loop.
                foreach ( var (typeArgumentX, typeArgumentY) in Enumerable.Zip( namedTypeX.TypeArguments, namedTypeY.TypeArguments, ( x, y ) => (x, y) ) )
                {
                    if ( !TypeEquals( typeArgumentX, typeArgumentY ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool MethodEquals( IMethodSymbol methodX, IMethodSymbol methodY, StructuralSymbolComparerOptions options )
        {
            if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) && !StringComparer.Ordinal.Equals( methodX.Name, methodY.Name ) )
            {
                return false;
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.GenericParameterCount ) && methodX.TypeParameters.Length != methodY.TypeParameters.Length )
            {
                return false;
            }

            if ( (options.HasFlag( StructuralSymbolComparerOptions.ParameterTypes ) || options.HasFlag( StructuralSymbolComparerOptions.ParameterModifiers ))
                 && methodX.Parameters.Length != methodY.Parameters.Length )
            {
                return false;
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterTypes ) && options.HasFlag( StructuralSymbolComparerOptions.ParameterModifiers ) )
            {
                // TODO: optimize using for loop.
                foreach ( var (parameterX, parameterY) in Enumerable.Zip( methodX.Parameters, methodY.Parameters, ( x, y ) => (x, y) ) )
                {
                    if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterTypes ) && !TypeEquals( parameterX.Type, parameterY.Type ) )
                    {
                        return false;
                    }

                    if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterModifiers ) && parameterY.RefKind != parameterY.RefKind )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool TypeEquals( ITypeSymbol typeX, ITypeSymbol typeY )
        {
            if ( typeX.Kind != typeY.Kind )
            {
                // Inequal kinds.
                return false;
            }

            switch ( (typeX, typeY) )
            {
                case (ITypeParameterSymbol typeParamX, ITypeParameterSymbol typeParamY):
                    return StringComparer.Ordinal.Equals( typeParamX.Name, typeParamY.Name )
                           && typeParamX.TypeParameterKind == typeParamY.TypeParameterKind;

                case (INamedTypeSymbol namedTypeX, INamedTypeSymbol namedTypeY):
                    return NamedTypeEquals( namedTypeX, namedTypeY, StructuralSymbolComparerOptions.Type );

                case (IArrayTypeSymbol arrayTypeX, IArrayTypeSymbol arrayTypeY):
                    return arrayTypeX.Rank == arrayTypeY.Rank
                           && TypeEquals( arrayTypeX.ElementType, arrayTypeY.ElementType );

                default:
                    throw new NotImplementedException( $"{typeX.Kind}" );
            }
        }

        private static bool ContainingElementEquals( ISymbol x, ISymbol y )
        {
            var currentX = x;
            var currentY = y;

            while ( currentX != null && currentY != null )
            {
                if ( currentX.Kind != currentY.Kind )
                {
                    return false;
                }

                switch ( (currentX, currentY) )
                {
                    case (IMethodSymbol methodX, IMethodSymbol methodY):
                        if ( !MethodEquals( methodX, methodY, StructuralSymbolComparerOptions.MethodSignature ) )
                        {
                            return false;
                        }

                        break;

                    case (INamedTypeSymbol namedTypeX, INamedTypeSymbol namedTypeY):
                        if ( !NamedTypeEquals( namedTypeX, namedTypeY, StructuralSymbolComparerOptions.Type ) )
                        {
                            return false;
                        }

                        break;

                    case (INamespaceSymbol namespaceX, INamespaceSymbol namespaceY):
                        if ( !StringComparer.Ordinal.Equals( namespaceX.Name, namespaceY.Name ) )
                        {
                            return false;
                        }

                        break;

                    case (IModuleSymbol _, IModuleSymbol _):
                        return true;

                    default:
                        throw new NotImplementedException( $"{currentX.Kind}" );
                }

                currentX = currentX.ContainingSymbol;
                currentY = currentY.ContainingSymbol;
            }

            // Check that the depth of the hierarchy is not the same. Should not be reachable.
            return currentX != null || currentY != null;
        }

        private static bool ContainingModuleEquals( IModuleSymbol moduleX, IModuleSymbol moduleY )
        {
            return
                StringComparer.Ordinal.Equals( moduleX.Name, moduleY.Name )
                && StringComparer.Ordinal.Equals( moduleX.ContainingAssembly.Name, moduleY.ContainingAssembly.Name )
                && Enumerable.SequenceEqual( moduleX.ContainingAssembly.Identity.PublicKey, moduleY.ContainingAssembly.Identity.PublicKey )
                && EqualityComparer<Version>.Default.Equals( moduleX.ContainingAssembly.Identity.Version, moduleY.ContainingAssembly.Identity.Version );
        }

        public int GetHashCode( ISymbol symbol )
        {
            // For performance reasons, GetHashCode should use a limited subset of properties where collisions are likely.
            return GetHashCode( symbol, this._options );
        }

        private static int GetHashCode( ISymbol symbol, StructuralSymbolComparerOptions options )
        {
            var h = 701_142_619; // Random prime.

            h = HashCode.Combine( h, symbol.Kind );

            switch ( symbol )
            {
                case INamedTypeSymbol type:
                    if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) )
                    {
                        h = HashCode.Combine( h, type.Name );
                    }

                    if ( options.HasFlag( StructuralSymbolComparerOptions.GenericParameterCount ) )
                    {
                        h = HashCode.Combine( h, type.TypeParameters.Length );
                    }

                    break;

                case IMethodSymbol method:
                    if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) )
                    {
                        h = HashCode.Combine( h, method.Name );
                    }

                    if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterTypes )
                         || options.HasFlag( StructuralSymbolComparerOptions.ParameterModifiers ) )
                    {
                        h = HashCode.Combine( h, method.Parameters.Length );

                        // TODO: optimize using for loop.
                        foreach ( var parameter in method.Parameters )
                        {
                            if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterTypes ) )
                            {
                                h = HashCode.Combine( h, GetHashCode( parameter.Type, StructuralSymbolComparerOptions.Type ) );
                            }

                            if ( options.HasFlag( StructuralSymbolComparerOptions.ParameterModifiers ) )
                            {
                                h = HashCode.Combine( h, parameter.RefKind );
                            }
                        }
                    }

                    break;

                case INamespaceSymbol @namespace:
                    if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) )
                    {
                        h = HashCode.Combine( h, @namespace.Name );
                    }

                    break;

                case IModuleSymbol _:
                    break;

                case ITypeParameterSymbol typeParameter:
                    if ( options.HasFlag( StructuralSymbolComparerOptions.Name ) )
                    {
                        h = HashCode.Combine( h, typeParameter.Name );
                    }

                    break;

                case IArrayTypeSymbol arrayType:
                    h = HashCode.Combine( h, arrayType.Rank, GetHashCode( arrayType.ElementType, StructuralSymbolComparerOptions.Type ) );

                    break;

                default:
                    throw new NotImplementedException( $"{symbol.Kind}" );
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.ContainingElement ) )
            {
                var current = symbol.ContainingSymbol;

                while ( current != null )
                {
                    switch ( current )
                    {
                        case INamedTypeSymbol namedType:
                            h = HashCode.Combine( h, namedType.Name, namedType.TypeParameters.Length );

                            break;

                        case INamespaceSymbol @namespace:
                            h = HashCode.Combine( h, @namespace.Name );

                            break;

                        case IMethodSymbol method:
                            h = HashCode.Combine( h, method.Name, method.TypeParameters.Length, method.Parameters.Length );

                            // This runs only if the original symbol was a local function.
                            foreach ( var parameter in method.Parameters )
                            {
                                h = HashCode.Combine( h, GetHashCode( parameter.Type, StructuralSymbolComparerOptions.Type ) );
                            }

                            break;

                        case IAssemblySymbol _:
                        case IModuleSymbol _:
                            // These are included below if required.
                            break;

                        default:
                            throw new NotImplementedException( $"{symbol.Kind}" );
                    }

                    current = current.ContainingSymbol;
                }
            }

            if ( options.HasFlag( StructuralSymbolComparerOptions.ContainingAssembly ) )
            {
                // Version should not differ often.
                h = HashCode.Combine( h, symbol.ContainingModule.Name, symbol.ContainingAssembly.Name );
            }

            return h;
        }
    }

    [Flags]
    internal enum StructuralSymbolComparerOptions
    {
        ContainingAssembly = 1 << 0,
        ContainingElement = 1 << 1,
        Name = 1 << 2,
        GenericParameterCount = 1 << 3,
        GenericArguments = 1 << 4,
        ParameterTypes = 1 << 5,
        ParameterModifiers = 1 << 6,
        FieldPromotions = 1 << 7,

        MethodSignature = Name | GenericParameterCount | GenericArguments | ParameterTypes | ParameterModifiers,
        Type = Name | GenericParameterCount | GenericArguments
    }
}