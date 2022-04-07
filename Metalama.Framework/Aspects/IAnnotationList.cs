// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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