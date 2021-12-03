// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal interface IMetaSerializerGenerator
    {
        MemberDeclarationSyntax CreateDeserializingConstructor( MetaSerializableTypeInfo serializableType );

        CompilationUnitSyntax CreateActivatorCompilationUnit();

        TypeDeclarationSyntax CreateSerializerType( MetaSerializableTypeInfo serializedType );
    }
}