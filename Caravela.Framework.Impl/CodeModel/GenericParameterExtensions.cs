// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
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
            => typeParameter.TypeKindConstraint switch
            {
                TypeKindConstraint.Class => typeParameter.HasDefaultConstructorConstraint,
                TypeKindConstraint.Struct => false,
                TypeKindConstraint.NotNull => false,
                TypeKindConstraint.Unmanaged => false,
                _ => null
            };

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
                case TypeKindConstraint.Struct when b.TypeKindConstraint is not TypeKindConstraint.Struct or TypeKindConstraint.Unmanaged:
                case TypeKindConstraint.Unmanaged when b.TypeKindConstraint is not TypeKindConstraint.Unmanaged:
                case TypeKindConstraint.NotNull
                    when b.TypeKindConstraint is not TypeKindConstraint.NotNull or TypeKindConstraint.Unmanaged or TypeKindConstraint.Struct:
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