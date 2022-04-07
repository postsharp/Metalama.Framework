// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

internal enum CompilationModelVersion
{
    Initial,
    Current,
    Final
}

internal interface IValidatorSource
{
    IEnumerable<ValidatorInstance> GetValidators(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        CompilationModel compilation,
        IDiagnosticSink diagnosticAdder );
}