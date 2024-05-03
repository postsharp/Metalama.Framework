﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Advising;

internal interface IAdvisableInternal
{
    IAdviceFactory AdviceFactory { get; }
}