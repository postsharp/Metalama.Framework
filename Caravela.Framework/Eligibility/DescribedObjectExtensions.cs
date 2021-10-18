// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// Extension methods for <see cref="IDescribedObject{T}"/>.
    /// </summary>
    [CompileTimeOnly]
    public static class DescribedObjectExtensions
    {
        public static IDescribedObject<TOut> Cast<TIn, TOut>( this IDescribedObject<TIn> describedObject )
            => new DescribedObject<TOut>( (TOut) (object) describedObject.Object!, describedObject.Description );
    }
}