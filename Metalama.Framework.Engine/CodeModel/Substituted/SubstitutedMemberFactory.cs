// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal static class SubstitutedMemberFactory
{
    public static Ref<T> Substitute<T>( T sourceDeclaration, INamedTypeSymbol targetType )
        where T : class, IMemberOrNamedType
        => Substitute( sourceDeclaration, new( targetType.TypeArguments, sourceDeclaration.Compilation.GetRoslynCompilation() ), targetType );

    public static Ref<T> Substitute<T>( T sourceDeclaration, GenericMap genericMap )
        where T : class, IDeclaration
        => sourceDeclaration switch
        {
            IParameter parameter => ((T) Substitute( parameter.DeclaringMember, genericMap )
                .GetTarget( ReferenceResolutionOptions.Default )
                .Parameters[parameter.Index]).ToTypedRef(),
            IMemberOrNamedType member => Substitute( member, genericMap, null ).As<T>(),
            _ => throw new AssertionFailedException( $"Unexpected declaration of type {sourceDeclaration.GetType()}" )
        };

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static Ref<T> Substitute<T>( T sourceDeclaration, GenericMap genericMap, INamedTypeSymbol? targetType = null )
        where T : class, IMemberOrNamedType
    {
        targetType ??= genericMap.Map( sourceDeclaration.DeclaringType.AssertNotNull().GetSymbol() );
        
        switch ( sourceDeclaration )
        {
            case SymbolBasedDeclaration { Symbol: var sourceSymbol }:
                {
                    // TODO: this won't work for symbols that are not directly contained in types, like parameters 
                    var substitutedMember = targetType.GetMembers( sourceSymbol.Name )
                        .Single( member => SymbolEqualityComparer.Default.Equals( member.OriginalDefinition, sourceSymbol ) );

                    return new( substitutedMember, sourceDeclaration.GetCompilationModel().CompilationContext );
                }

            case BuiltDeclaration builtDeclaration:
                {
                    var substitutedMember = Create( builtDeclaration, targetType );

                    return substitutedMember.ToRef().As<T>();
                }

            default:
                throw new AssertionFailedException( $"Unexpected source declaration type {sourceDeclaration.GetType()}" );
        }
    }
    
    // TODO: cache (in DeclarationFactory?)
    private static SubstitutedMember Create( BuiltDeclaration sourceDeclaration, INamedTypeSymbol substitutedType )
        => sourceDeclaration switch
        {
            BuiltMethod method => new SubstitutedMethod( method, substitutedType ),
            _ => throw new AssertionFailedException( $"Unexpect declaration type {sourceDeclaration.GetType()}" )
        };
}