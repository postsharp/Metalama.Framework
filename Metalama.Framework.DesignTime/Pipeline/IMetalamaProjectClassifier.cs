// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal interface IMetalamaProjectClassifier : IService
{
    bool IsMetalamaEnabled( Compilation compilation );
}