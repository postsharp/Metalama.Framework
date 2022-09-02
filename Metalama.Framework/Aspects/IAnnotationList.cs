// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// A list of <see cref="IAnnotation"/>.
    /// </summary>
    [CompileTime]
    [InternalImplement]
    [Obsolete( "Not implemented." )]
    public interface IAnnotationList : IReadOnlyList<IAnnotation>
    {
        bool Any<T>()
            where T : IAnnotation;
    }
}