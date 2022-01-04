// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that, when applied to an aspect class, means that this aspect can be used
/// interactively, at design time, as a live template.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class LiveTemplateAttribute : Attribute { }