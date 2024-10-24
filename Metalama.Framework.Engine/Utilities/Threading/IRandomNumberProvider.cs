﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Utilities.Threading;

public interface IRandomNumberProvider : IGlobalService
{
    int GetNextInt();

    double GetNextDouble();
}