// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [Obsolete( "Not implemented." )]
    public static class DescribedObjectExtensions
    {
        public static IDescribedObject<TOut> Cast<TIn, TOut>( this IDescribedObject<TIn> describedObject )
            => new DescribedObject<TOut>( (TOut) (object) describedObject.Object!, describedObject.FormatProvider, describedObject.Description );
    }
}