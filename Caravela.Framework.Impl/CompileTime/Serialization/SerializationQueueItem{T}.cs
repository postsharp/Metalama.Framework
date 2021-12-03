// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    
    internal sealed class SerializationQueueItem<T>
    {
        public SerializationQueueItem( T o, SerializationCause cause )
        {
            this.Value = o;
            this.Cause = cause;
        }

        public T Value { get; }

        public SerializationCause Cause { get; }
    }
}