// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// This custom attribute is internal to the Metalama infrastructure and should not be used in user code.
/// It is added by Metalama when an aspect is compile to store the original accessibility of the template, because the
/// actual accessibility is changed to <c>public</c>.
/// </summary>
[CompileTime]
[AttributeUsage( AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property )]
public class AccessibilityAttribute : Attribute
{
    public AccessibilityAttribute( Accessibility accessibility )
    {
        _ = accessibility;
    }
}