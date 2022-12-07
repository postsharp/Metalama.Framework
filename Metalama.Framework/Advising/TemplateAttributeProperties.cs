// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

public record TemplateAttributeProperties(
    string? Name = null,
    Accessibility? Accessibility = null,
    bool? IsVirtual = null,
    bool? IsSealed = null,
    bool? IsRequired = null );