// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Kinds of change of the compile-time status of a syntax tree.
    /// </summary>
    internal enum CompileTimeChangeKind
    {
        /// <summary>
        /// No change in the compile-time status.
        /// </summary>
        None,
        
        /// <summary>
        /// The syntax tree is newly compile-time.
        /// </summary>
        NewlyCompileTime,
        
        /// <summary>
        /// The syntax tree is no longer compile-time.
        /// </summary>
        NoLongerCompileTime
    }
}