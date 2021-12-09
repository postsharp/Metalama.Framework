// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// A dictionary of tags that can be passed to an advise.
    /// </summary>
    public sealed class Tags : Dictionary<string, object?> { }
}