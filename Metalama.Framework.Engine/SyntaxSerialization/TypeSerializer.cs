// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class TypeSerializer : ObjectSerializer<Type>
    {
        public override ExpressionSyntax Serialize( Type obj, SyntaxSerializationContext serializationContext )
            => TypeSerializationHelper.SerializeTypeSymbolRecursive( serializationContext.GetTypeSymbol( obj ), serializationContext );
      

        public TypeSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}