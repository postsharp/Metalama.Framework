// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;

namespace Metalama.Framework.Engine.Licensing;

[JsonObject]
public sealed class LicenseCreditConsumption
{
    public string ItemName { get; }

    public decimal ConsumedCredits { get; }

    public LicenseCreditConsumptionKind Kind { get; }

    public LicenseCreditConsumption( string itemName, decimal consumedCredits, LicenseCreditConsumptionKind kind )
    {
        this.ItemName = itemName;
        this.ConsumedCredits = consumedCredits;
        this.Kind = kind;
    }
}