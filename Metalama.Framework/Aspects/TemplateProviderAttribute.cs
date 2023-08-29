// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Attribute used to mark types that define template members, which must be annotated with <see cref="TemplateAttribute"/>.
/// To use an instance of this type, use the <see cref="IAdviceFactory.WithTemplateProvider"/> method. 
/// </summary>
[RunTimeOrCompileTime]
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface )]
public class TemplateProviderAttribute : Attribute { }