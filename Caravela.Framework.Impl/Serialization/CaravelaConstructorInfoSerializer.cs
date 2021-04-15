// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CaravelaConstructorInfoSerializer : TypedObjectSerializer<CompileTimeConstructorInfo>
    {
        private readonly CaravelaTypeSerializer _typeSerializer;

        public CaravelaConstructorInfoSerializer( CaravelaTypeSerializer typeSerializer )
        {
            this._typeSerializer = typeSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeConstructorInfo o )
        {
            return CaravelaMethodInfoSerializer.CreateMethodBase( this._typeSerializer, o );
        }
    }
}