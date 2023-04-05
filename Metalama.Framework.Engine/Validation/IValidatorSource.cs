// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
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
        UserDiagnosticSink diagnosticAdder );
}