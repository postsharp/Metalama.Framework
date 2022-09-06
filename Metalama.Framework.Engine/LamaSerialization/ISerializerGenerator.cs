// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal interface ISerializerGenerator
    {
        bool ShouldSuppressReadOnly( SerializableTypeInfo serializableType, ISymbol memberSymbol );

        MemberDeclarationSyntax CreateDeserializingConstructor( SerializableTypeInfo serializableType, in QualifiedTypeNameInfo serializedTypeName );

        TypeDeclarationSyntax CreateSerializerType( SerializableTypeInfo serializedType, in QualifiedTypeNameInfo serializedTypeName );
    }
}