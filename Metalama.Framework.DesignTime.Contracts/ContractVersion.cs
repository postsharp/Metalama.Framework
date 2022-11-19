// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts;

[Guid( "8A5841E3-5D21-495C-99D8-280558B3A7BD" )]
public struct ContractVersion
{
    public string Version;
    public int Revision;
}