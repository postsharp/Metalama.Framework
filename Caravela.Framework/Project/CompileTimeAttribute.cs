// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Attribute that means that the target declaration (and all children declarations) can be called from compile-time
    /// code. It can also be called from run-time code. See <see cref="CompileTimeOnlyAttribute"/> for declarations
    /// that cannot be called from run-time code.
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public class CompileTimeAttribute : Attribute { }
}