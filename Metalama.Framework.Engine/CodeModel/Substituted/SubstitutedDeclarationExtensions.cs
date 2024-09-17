// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal static class SubstitutedDeclarationExtensions
{
    [return: NotNullIfNotNull( nameof(type) )]
    public static T? Substitute<T>( this ISubstitutedDeclaration declaration, T? type )
        where T : class, IType
    {
        if ( type == null )
        {
            return null;
        }

        return (T) declaration.GenericMap.Substitute( type );
    }
}