// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

public interface IReferenceValidatorProperties
{
    ReferenceKinds ReferenceKinds { get; }

    bool IncludeDerivedTypes { get; }

    DeclarationKind ValidatedDeclarationKind { get; }
}