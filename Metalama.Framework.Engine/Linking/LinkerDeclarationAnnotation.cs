// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Linking
{
    internal readonly struct LinkerDeclarationAnnotation
    {
        public LinkerDeclarationFlags Flags { get; }

        public LinkerDeclarationAnnotation( LinkerDeclarationFlags flags )
        {
            this.Flags = flags;
        }

        public static LinkerDeclarationAnnotation FromString( string str )
        {
            // ReSharper disable once RedundantAssignment
            var success = Enum.TryParse<LinkerDeclarationFlags>( str, out var flags );

            Invariant.Assert( success );

            return new LinkerDeclarationAnnotation( flags );
        }

        public override string ToString()
        {
            return $"{this.Flags}";
        }
    }
}