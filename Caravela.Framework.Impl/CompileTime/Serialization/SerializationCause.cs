// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal class SerializationCause
    {
        public SerializationCause Parent { get; }

        public string Description { get; }

        private SerializationCause(  string description, SerializationCause parent )
        {
            this.Parent = parent;
            this.Description = description;
        }

        public static SerializationCause WithTypedValue( SerializationCause parent, string fieldName, Type type )
        {
            return new SerializationCause( $"{(parent != null ? "." : "")}{type.Name}::{fieldName}", parent );
        }
        
        public static SerializationCause WithIndices( SerializationCause parent, params int[] indices )
        {
            return new SerializationCause( string.Join( "", indices.Select( i => $"[{i}]" ).ToArray() ), parent );
        }
    }
}