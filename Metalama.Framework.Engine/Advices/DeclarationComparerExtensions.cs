// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Advices
{
    internal static class DeclarationComparerExtensions
    {
        public static bool ParameterTypeEquals( this IDeclarationComparer comparer, IType parameterType, IType otherType )
        {
            if ( parameterType.GetSymbol() is ITypeParameterSymbol typeParam && otherType.GetSymbol() is ITypeParameterSymbol otherTypeParam )
            {
                return typeParam.Ordinal == otherTypeParam.Ordinal;
            }
            else
            {
                return comparer.Equals( parameterType, otherType );
            }
        }
    }
}