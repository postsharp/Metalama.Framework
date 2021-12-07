// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute to be used besides <see cref="TemplateAttribute"/>, which means that the target method has no implementation and should be ignored.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
    [CompileTimeOnly]
    internal sealed class AbstractAttribute : Attribute { }
}