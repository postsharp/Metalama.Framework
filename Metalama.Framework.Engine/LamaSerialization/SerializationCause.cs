// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal class SerializationCause
    {
        public SerializationCause? Parent { get; }

        public string Description { get; }

        private SerializationCause( string description, SerializationCause? parent )
        {
            this.Parent = parent;
            this.Description = description;
        }

        public static SerializationCause WithTypedValue( SerializationCause? parent, string fieldName, Type type )
        {
            return new SerializationCause( $"{(parent != null ? "." : "")}{type.Name}::{fieldName}", parent );
        }

        public static SerializationCause WithIndices( SerializationCause? parent, params int[] indices )
        {
            return new SerializationCause( string.Join( "", indices.SelectArray( i => $"[{i}]" ) ), parent );
        }
    }
}