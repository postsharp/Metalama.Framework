// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Project;

namespace Metalama.Framework.Engine.Licensing;

public interface ILicenseConsumptionManagerProvider : IService
{
    ILicenseConsumptionManager? LicenseConsumptionManager { get; }
}