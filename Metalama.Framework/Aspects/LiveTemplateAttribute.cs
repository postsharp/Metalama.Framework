// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that, when applied to an aspect class, means that this aspect can be used
/// interactively, at design time, as a live template.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class LiveTemplateAttribute : Attribute { }