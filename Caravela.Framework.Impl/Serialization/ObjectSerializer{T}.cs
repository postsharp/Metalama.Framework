// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Serialization
{
    internal abstract class ObjectSerializer<T> : ObjectSerializer<T, T>
    {
        protected ObjectSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}