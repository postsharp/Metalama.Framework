﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Validation;

public abstract class OutboundReferenceValidator : BaseReferenceValidator
{
    public sealed override ReferenceDirection Direction => ReferenceDirection.Outbound;
}