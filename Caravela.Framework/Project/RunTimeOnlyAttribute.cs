﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Attribute that means that the return value of the target method (this attribute must be added to the return value and not to the method itself)
    /// is a run-time value. This attribute is typically applied to compile-time methods that return run-time values.
    /// </summary>
    [AttributeUsage( AttributeTargets.ReturnValue )]
    public class RunTimeOnlyAttribute : Attribute { }
}