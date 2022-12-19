// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Maps System.Reflection objects to Roslyn symbols.
    /// </summary>
    internal sealed class ReflectionMapper
    {
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<Type, ITypeSymbol> _symbolCache = new();
        private ImmutableDictionaryOfArray<string, IAssemblySymbol>? _referencedAssemblies;

        internal ReflectionMapper( Compilation compilation )
        {
            this._compilation = compilation;
        }

        /// <summary>
        /// Gets a <see cref="INamedTypeSymbol"/> by metadata name.
        /// </summary>
        public INamedTypeSymbol GetNamedTypeSymbolByMetadataName( string metadataName, AssemblyName? assemblyName )
        {
            var symbol = this._compilation.GetTypeByMetadataName( metadataName );

            if ( symbol == null )
            {
                // When the first attempt to resolve a symbol fails, we try harder to determine the cause 
                // and display a better error message. We may also be able to resolve version conflicts.

                // This field cannot be set in the constructor because of linker tests.
                this._referencedAssemblies ??= this._compilation.SourceModule.ReferencedAssemblySymbols.ToMultiValueDictionary( a => a.Identity.Name, a => a );

                var assemblyShortName = assemblyName?.Name;

                if ( assemblyShortName != null )
                {
                    var assemblies = this._referencedAssemblies[assemblyShortName];

                    if ( assemblies.IsEmpty )
                    {
                        throw new InvalidOperationException(
                            $"Cannot find the reference '{assemblyName}' in project '{this._compilation.AssemblyName}' required for type '{metadataName}'." );
                    }
                    else if ( assemblies.Length > 1 )
                    {
                        throw new InvalidOperationException(
                            $"Found more than one assembly named '{assemblyShortName}' in project '{this._compilation.AssemblyName}': {string.Join( ",", assemblies.Select( x => $"'{x.Identity}'" ) )}." );
                    }

                    symbol = assemblies[0].GetTypeByMetadataName( metadataName );
                }
            }

            if ( symbol == null )
            {
                throw GeneralDiagnosticDescriptors.CannotFindType.CreateException( (metadataName, assemblyName?.ToString()) );
            }

            return symbol;
        }

        /// <summary>
        /// Gets a <see cref="ITypeSymbol"/> given a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITypeSymbol GetTypeSymbol( Type type )
        {
            switch ( type )
            {
                case CompileTimeType compileTimeType:
                    return (ITypeSymbol) compileTimeType.Target.GetSymbol( this._compilation, true )
                        .AssertNotNull( Justifications.SerializersNotImplementedForIntroductions );

                default:
                    return this._symbolCache.GetOrAdd( type, this.GetTypeSymbolCore );
            }
        }

        private ITypeSymbol GetTypeSymbolCore( Type type )
        {
            if ( type.IsGenericParameter )
            {
                return this.GetNamedTypeSymbol( type.DeclaringType.AssertNotNull(), Array.Empty<Type>() ).TypeParameters[type.GenericParameterPosition];
            }

            if ( type is CompileTimeType compileTimeType )
            {
                return (ITypeSymbol) compileTimeType.Target.GetSymbol( this._compilation, true ).AssertNotNull( Justifications.TypesAlwaysHaveSymbol );
            }

            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types cannot be represented as Metalama types." );
            }

            if ( type.IsArray )
            {
                var elementType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreateArrayTypeSymbol( elementType, type.GetArrayRank() );
            }

            if ( type.IsPointer )
            {
                var pointedToType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreatePointerTypeSymbol( pointedToType );
            }

            return this.GetNamedTypeSymbol( type, type.GenericTypeArguments );
        }

        private INamedTypeSymbol GetNamedTypeSymbol( Type type, Type[] genericArguments )
        {
            if ( type.DeclaringType != null )
            {
                // In case of nested type, we need to determine the arity from the name. This info is otherwise not exposed.
                var indexOfQuote = type.Name.IndexOfOrdinal( '`' );

                var arity = 0;

                if ( indexOfQuote >= 0 )
                {
                    var arityString = type.Name.Substring( indexOfQuote + 1 );
                    arity = int.Parse( arityString, CultureInfo.InvariantCulture );
                }

                var declaringTypeGenericArguments = genericArguments.Take( genericArguments.Length - arity ).ToArray();
                var nestedTypeGenericArguments = genericArguments.Skip( declaringTypeGenericArguments.Length ).ToArray();

                var declaringTypeSymbol = this.GetNamedTypeSymbol( type.DeclaringType, declaringTypeGenericArguments );
                var nestedSymbol = declaringTypeSymbol.GetTypeMembers().Single( s => s.MetadataName == type.Name );

                if ( nestedTypeGenericArguments.Length > 0 )
                {
                    nestedSymbol = nestedSymbol.Construct( nestedTypeGenericArguments.SelectAsArray( this.GetTypeSymbol ) );
                }

                return nestedSymbol;
            }
            else if ( genericArguments.Length > 0 )
            {
                var genericDefinition = this.GetNamedTypeSymbolByMetadataName(
                    type.GetGenericTypeDefinition().FullName.AssertNotNull(),
                    type.Assembly.GetName() );

                var genericArgumentSymbols = genericArguments.SelectAsArray( this.GetTypeSymbol );

                return genericDefinition.Construct( genericArgumentSymbols );
            }
            else
            {
                return this.GetNamedTypeSymbolByMetadataName( type.FullName.AssertNotNull(), type.Assembly.GetName() );
            }
        }
    }
}