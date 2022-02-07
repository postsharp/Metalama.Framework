﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Aspect drivers are responsible for executing aspects.
    /// </summary>
    /// <remarks>
    /// There are low-level aspect drivers, which should implement <see cref="IAspectWeaver"/>, and a high-level aspect driver implemented
    /// by Metalama. These two families of drivers don't share any semantic. This interface exists for clarity and type safety only.
    /// </remarks>
    public interface IAspectDriver { }
}