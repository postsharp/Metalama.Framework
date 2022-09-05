// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute to be used besides <see cref="TemplateAttribute"/>, which means that the target method has no implementation and should be ignored.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
    [CompileTime]
    internal sealed class AbstractAttribute : Attribute { }
}