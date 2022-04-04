// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for <see cref="RefKind"/>.
    /// </summary>
    [CompileTime]
    public static class RefKindExtensions
    {
        // Coverage: ignore

        /// <summary>
        /// Determines whether the parameter or return value represents a reference (<c>in</c> and <c>out</c> properties, <c>ref</c> and <c>ref readonly</c> methods and properties).  
        /// </summary>
        public static bool IsByRef( this RefKind kind ) => kind != RefKind.None;

        // Coverage: ignore

        /// <summary>
        /// Determines whether the parameter or return value can be assigned.
        /// </summary>
        public static bool IsWritable( this RefKind kind )
            => kind switch
            {
                RefKind.None => true,
                RefKind.In => false,
                RefKind.Ref => true,
                RefKind.Out => true,
                _ => throw new ArgumentOutOfRangeException( nameof(kind) )
            };

        /// <summary>
        /// Determines whether the parameter or return value can be read before it has been assigned.
        /// </summary>
        public static bool IsReadable( this RefKind kind ) => kind != RefKind.Out;
    }
}