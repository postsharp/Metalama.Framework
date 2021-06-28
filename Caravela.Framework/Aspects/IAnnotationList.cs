// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A list of <see cref="IAnnotation"/>.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    [Obsolete( "Not implemented." )]
    public interface IAnnotationList : IReadOnlyList<IAnnotation>
    {
        bool Any<T>()
            where T : IAnnotation;
    }
}