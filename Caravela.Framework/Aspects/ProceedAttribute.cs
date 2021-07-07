// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Marks the method as having <c>proceed</c> semantics.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    internal class ProceedAttribute : Attribute { }

    // TODO: This class and its usages must be removed.
}