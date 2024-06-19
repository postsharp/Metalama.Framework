﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Engine.Fabrics;

public interface IFabricInstanceInternal : IFabricInstance
{
    string FabricTypeFullName { get; }
}