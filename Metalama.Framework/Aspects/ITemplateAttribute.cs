// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Aspects;

public interface ITemplateAttribute : IAdviceAttribute
{
    string? Name { get; }

    bool? IsVirtual { get; }

    bool? IsSealed { get; }

    Accessibility? Accessibility { get; }
}