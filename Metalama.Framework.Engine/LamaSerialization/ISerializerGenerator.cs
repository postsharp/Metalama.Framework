// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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