// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Marks the declaration (and all children declarations) as compile-time for the template compiler.
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public class CompileTimeAttribute : Attribute
    {
    }
}