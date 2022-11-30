// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Read-only list of <see cref="IParameterBuilder"/>.
    /// </summary>
    public interface IParameterBuilderList : IReadOnlyList<IParameterBuilder>
    {
        // This type cannot extend IParameterList, because it leads to ambiguity.

        IParameterBuilder this[ string name ] { get; }
    }
}