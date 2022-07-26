// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using System;

namespace Metalama.Framework.Engine.Fabrics;

internal interface IAspectReceiverParent : IValidatorDriverFactory, IAspectOrValidatorSourceCollector
{
    IServiceProvider ServiceProvider { get; }

    BoundAspectClassCollection AspectClasses { get; }

    UserCodeInvoker UserCodeInvoker { get; }

    AspectPredecessor AspectPredecessor { get; }

    Type Type { get; }
}