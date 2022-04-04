﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Attribute that means that the target declaration (and all children declarations) can only be called from compile-time
    /// code, therefore not from run-time code. See <see cref="RunTimeOrCompileTimeAttribute"/> for declarations
    /// that can be called both from compile- and run-time code.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface
        | AttributeTargets.Assembly | AttributeTargets.ReturnValue | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
        | AttributeTargets.Constructor | AttributeTargets.Event )]
    public class CompileTimeAttribute : ScopeAttribute { }
}