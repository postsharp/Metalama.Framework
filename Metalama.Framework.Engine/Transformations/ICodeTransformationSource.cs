// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a transformation that provides code transformations.
    /// </summary>
    internal interface ICodeTransformationSource : ISyntaxTreeTransformation
    {
        IEnumerable<ICodeTransformation> GetCodeTransformations( in CodeTransformationSourceContext context );
    }
}