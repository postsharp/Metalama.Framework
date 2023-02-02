// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal static class SerializableTypeIdGenerator
{
    internal static SerializableTypeId GetSerializableTypeId( this ITypeSymbol symbol )
    {
        return new SerializableTypeId( OurSyntaxGenerator.CompileTime.TypeOfExpression( symbol ).ToString() );
    }
}