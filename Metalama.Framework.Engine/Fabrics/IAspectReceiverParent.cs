// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Engine.Fabrics;

internal interface IAspectReceiverParent : IValidatorDriverFactory, IPipelineContributorSourceCollector, IDiagnosticSource
{
    IProject Project { get; }

    string? Namespace { get; }

    ProjectServiceProvider ServiceProvider { get; }

    BoundAspectClassCollection AspectClasses { get; }
    
    UserCodeInvoker UserCodeInvoker { get; }

    AspectPredecessor AspectPredecessor { get; }

    Type Type { get; }

    LicenseVerifier? LicenseVerifier { get; }
}