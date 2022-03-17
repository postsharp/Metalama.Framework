// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal sealed class SerializationQueueItem<T>
    {
        public SerializationQueueItem( T? o, SerializationCause? cause )
        {
            this.Value = o;
            this.Cause = cause;
        }

        public T? Value { get; }

        public SerializationCause? Cause { get; }
    }
}