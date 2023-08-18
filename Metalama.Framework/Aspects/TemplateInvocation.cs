// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Represents call to a template method.
/// </summary>
/// <param name="TemplateName">The name of the called template method.</param>
/// <param name="TemplateProvider">Object on which the template method will be called, <see cref="Type"/> for static template methods, or <see langword="null"/> for the current template provider (usually the current aspect).</param>
/// <param name="Arguments">Compile-time template arguments that will be passed to the template.</param>
[CompileTime]
public record TemplateInvocation( string TemplateName, object? TemplateProvider, object? Arguments = null );