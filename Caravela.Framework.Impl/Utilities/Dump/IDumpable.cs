// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Caravela.Framework.Impl.Utilities.Dump
{
    /// <summary>
    /// Exposes <see cref="ToDump"/>, a method used by LINQPad to format objects. All types that are visible from
    /// the public API should implement it.
    /// </summary>
    [Obfuscation( Exclude = true )]
    internal interface IDumpable
    {
        /// <summary>
        /// Gets a representation of the object that is suitable for LINQPad. 
        /// </summary>
        object ToDump();
    }
}