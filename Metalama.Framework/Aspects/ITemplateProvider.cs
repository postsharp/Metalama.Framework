// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Mark the class with <see cref="TemplateProviderAttribute"/> instead.
/// </summary>
[Obsolete("Mark the class with [TemplateProvider] instead.", error: true)]
public interface ITemplateProvider { }