// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Fabrics;
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
    Task CollectValidatorsAsync(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        OutboundActionCollectionContext context );
}