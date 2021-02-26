﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attributes that marks the target method as a template for <see cref="IOverrideMethodAdvice"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class OverrideMethodTemplateAttribute : TemplateAttribute
    {
    }
}
