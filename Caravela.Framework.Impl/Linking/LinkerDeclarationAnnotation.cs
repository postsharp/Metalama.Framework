﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Linking
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