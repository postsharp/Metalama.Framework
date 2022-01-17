// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that, when applied to an aspect class, means that instances of this aspect
    /// are inherited from the base class or interface to derived classes, from base methods to method overrides,
    /// from interface methods to method implementations, and so on. 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class InheritedAttribute : Attribute { }
}