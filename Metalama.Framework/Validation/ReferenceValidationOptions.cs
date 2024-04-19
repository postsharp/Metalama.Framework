// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Validation;

[Flags]
[CompileTime]
public enum ReferenceValidationOptions
{
    Default = 0,

    /// <summary>
    /// Indicates that references to derived types should also be visited by the validation method.
    /// This property is only evaluated when the validated declaration is an <see cref="INamedType"/>.
    /// </summary>
    IncludeDerivedTypes = 1
};