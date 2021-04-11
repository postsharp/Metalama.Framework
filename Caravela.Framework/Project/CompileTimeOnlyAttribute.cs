﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    
    /// <summary>
    /// Attribute that means that the target declaration (and all children declarations) can only be called from compile-time
    /// code, therefore not from run-time code. See <see cref="CompileTimeAttribute"/> for declarations
    /// that can be called both from compile- and run-time code.
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public class CompileTimeOnlyAttribute : Attribute
    {
    }
}