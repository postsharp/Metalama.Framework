// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IParameter"/>.
    /// </summary>
    [CompileTime]
    public interface IParameterList : IReadOnlyList<IParameter>
    {
        IParameter this[ string name ] { get; }
    }
}