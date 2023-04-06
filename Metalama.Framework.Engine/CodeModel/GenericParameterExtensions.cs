// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
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
            // Unconstrained, class? constrained and IFoo? constrained are considered nullable,
            // even if they have NullableAnnotation.NotAnnotated.
            if ( (typeParameter.TypeKindConstraint == TypeKindConstraint.None && !typeParameter.TypeConstraints.Any())
                 || typeParameter.IsConstraintNullable == true
                 || typeParameter.TypeConstraints.Any( t => t.IsNullable == true ) )
            {
                return true;
            }

            // Otherwise, annotation takes priority over constraint.
            // E.g. in void M<T>(T? t) where T : notnull, the type of t is ITypeParameter with TypeKindConstraint of NotNull and NullableAnnotation.Annotated.
            if ( ((IDeclaration) typeParameter).GetSymbol() is ITypeSymbol { NullableAnnotation: var annotation } )
            {
                return annotation.ToIsAnnotated();
            }

            // If we couldn't find a symbol for NullableAnnotation, fall back to checking constraints.
            return typeParameter.TypeKindConstraint switch
            {
                TypeKindConstraint.Class => typeParameter.IsConstraintNullable,
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