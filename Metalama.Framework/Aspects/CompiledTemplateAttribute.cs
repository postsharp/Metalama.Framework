// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// This custom attribute is internal to the Metalama infrastructure and should not be used in user code.
/// It is added by Metalama when an aspect is compile to store the original characteristics of the template because some are
/// changed during compilation.
/// </summary>
[CompileTime]
[AttributeUsage( AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property )]
public class CompiledTemplateAttribute : Attribute
{
    public Accessibility Accessibility { get; set; }
}