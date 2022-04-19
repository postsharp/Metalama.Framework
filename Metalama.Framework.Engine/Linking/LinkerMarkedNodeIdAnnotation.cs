// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    internal readonly struct LinkerMarkedNodeIdAnnotation
    {
        public string Id { get; }

        public LinkerMarkedNodeIdAnnotation( string id )
        {
            this.Id = id;
        }

        public static LinkerMarkedNodeIdAnnotation FromString( string id )
        {
            return new LinkerMarkedNodeIdAnnotation( id );
        }

        public override string ToString()
        {
            return $"{this.Id}";
        }
    }
}