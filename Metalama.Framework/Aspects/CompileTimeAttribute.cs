// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Attribute that means that the target declaration (and all children declarations) can be called from compile-time
    /// code. It can also be called from run-time code. See <see cref="CompileTimeOnlyAttribute"/> for declarations
    /// that cannot be called from run-time code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use this attribute on classes that must be included in the compile-time project and therefore made
    /// available to your aspects.
    /// </para>
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface
        | AttributeTargets.Assembly )]
    public class CompileTimeAttribute : ScopeAttribute { }
}