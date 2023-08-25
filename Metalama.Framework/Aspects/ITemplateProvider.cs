// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Base interface for types that define template members, which must be annotated with <see cref="TemplateAttribute"/>.
/// To use an instance of this type, use the <see cref="IAdviceFactory.WithTemplateProvider(ITemplateProvider)"/> method. 
/// </summary>
[TemplateProvider]
public interface ITemplateProvider { }