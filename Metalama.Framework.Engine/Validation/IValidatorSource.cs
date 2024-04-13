// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Validation;

internal enum CompilationModelVersion
{
    Initial,
    Current,
    Final
}

internal interface IValidatorSource
{
    Task AddValidatorsAsync(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        CompilationModel compilation,
        OutboundActionCollector collector,
        CancellationToken cancellationToken );
}