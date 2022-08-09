// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Base interface for types that define template members, which must be annotated with <see cref="TemplateAttribute"/>.
/// To use an instance of this type, use the <see cref="IAdviceFactory.WithTemplateProvider"/> method of the <see cref="IAdviceFactory"/>. 
/// </summary>
[RunTimeOrCompileTime]
public interface ITemplateProvider { }