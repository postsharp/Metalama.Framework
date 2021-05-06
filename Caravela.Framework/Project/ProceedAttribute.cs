// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Marks the method as having <c>proceed</c> semantics.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class ProceedAttribute : Attribute { }
}