﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Linking
{
    internal readonly struct LinkerGeneratedAnnotation
    {
        public LinkerGeneratedFlags Flags { get; }

        public LinkerGeneratedAnnotation( LinkerGeneratedFlags flags )
        {
            this.Flags = flags;
        }

        public static LinkerGeneratedAnnotation FromString( string str )
        {
            // ReSharper disable once RedundantAssignment
            var success = Enum.TryParse<LinkerGeneratedFlags>( str, out var flags );

            Invariant.Assert( success );

            return new LinkerGeneratedAnnotation( flags );
        }

        public override string ToString()
        {
            return $"{this.Flags}";
        }
    }
}