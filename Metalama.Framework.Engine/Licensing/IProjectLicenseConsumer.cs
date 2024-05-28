// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Licensing;

public interface IProjectLicenseConsumer : ILicenseConsumer, IProjectService
{
    ILicenseConsumptionService Service { get; }
}