// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Aspects;

public interface ITemplateAttribute : IAdviceAttribute
{
    string? Name { get; }

    bool? IsVirtual { get; }

    bool? IsSealed { get; }

    Accessibility? Accessibility { get; }
}