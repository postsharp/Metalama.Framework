// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Helpers
{
    internal static class GenericParameterExtensions
    {
        public static bool? IsReferenceTypeImpl( this ITypeParameter typeParameter )
            => typeParameter.TypeKindConstraint switch
            {
                TypeKindConstraint.Class => true,
                TypeKindConstraint.Struct => false,
                TypeKindConstraint.Unmanaged => false,
                _ => null
            };

        public static bool? IsNullableImpl( this ITypeParameter typeParameter )
        {
            // Unconstrained, class? constrained and IFoo? constrained are *not* considered nullable,
            // if they have NullableAnnotation.NotAnnotated, because non-nullable types also satify these constraints.

            // Annotation takes priority over constraint.
            // E.g. in void M<T>(T? t) where T : notnull, the type of t is ITypeParameter with TypeKindConstraint of NotNull and NullableAnnotation.Annotated.
            if ( typeParameter.GetSymbol() is ITypeSymbol { NullableAnnotation: var annotation } && annotation.ToIsAnnotated() == true )
            {
                return true;
            }

            // If we couldn't find a symbol for NullableAnnotation or it didn't have '?', check constraints.
            if ( typeParameter.TypeConstraints.Any( t => t.IsNullable == false ) )
            {
                return false;
            }

            return typeParameter.TypeKindConstraint switch
            {
                TypeKindConstraint.Class => typeParameter.IsConstraintNullable == false ? false : null,
                TypeKindConstraint.Struct => false,
                TypeKindConstraint.NotNull => false,
                TypeKindConstraint.Unmanaged => false,
                _ => null
            };
        }

        public static bool IsCompatibleWith( this ITypeParameter a, ITypeParameter b )
        {
            // Check new() constraint.
            if ( a.HasDefaultConstructorConstraint && !b.HasDefaultConstructorConstraint )
            {
                return false;
            }

            // Check type kind.
            switch ( a.TypeKindConstraint )
            {
                case TypeKindConstraint.Class when b.TypeKindConstraint is not TypeKindConstraint.Class:
                case TypeKindConstraint.Struct when b.TypeKindConstraint is not (TypeKindConstraint.Struct or TypeKindConstraint.Unmanaged):
                case TypeKindConstraint.Unmanaged when b.TypeKindConstraint is not TypeKindConstraint.Unmanaged:
                case TypeKindConstraint.NotNull
                    when b.TypeKindConstraint is not (TypeKindConstraint.NotNull or TypeKindConstraint.Unmanaged or TypeKindConstraint.Struct):
                    return false;
            }

            // Check types.
            if ( a.TypeConstraints.Any( aConstraint => !b.TypeConstraints.Any( bConstraint => bConstraint.Is( aConstraint ) ) ) )
            {
                return false;
            }

            return true;
        }
    }
}