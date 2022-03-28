﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any transformation.
    /// </summary>
    internal interface ITransformation
    {
        Advice Advice { get; }
    }
}