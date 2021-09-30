// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IConstructor"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IConstructorList : IMemberList<IConstructor>
    {
        /// <summary>
        /// Gets an enumeration of constructors with signatures compatible with specified constraints given using the <c>System.Reflection</c> API.
        /// </summary>
        /// <param name="argumentTypes">Constraint on reflection types of arguments. <c>Null</c>items in the list signify any type.</param>
        /// <returns>Enumeration of constructors matching specified constraints.</returns>
        IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?>? argumentTypes );

        /// <summary>
        /// Gets an enumeration of constructors with signatures compatible with specified constraints given using the Caravela API.
        /// </summary>
        /// <param name="argumentTypes">Constraint on types of arguments. <c>Null</c>items in the list signify any type.</param>
        /// <param name="refKinds">Constraint on reference kinds of arguments. <c>Null</c>items in the list signify any reference kind.</param>
        /// <returns>Enumeration of constructors matching specified constraints.</returns>
        IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null );

        /// <summary>
        /// Gets a constructor that exactly matches the specified signature given using the <c>System.Reflection</c> API.
        /// </summary>
        /// <param name="parameterTypes">List of parameter types.</param>
        /// <param name="refKinds">List of parameter reference kinds, or <c>null</c> if all parameters should be by-value.</param>
        /// <returns>A <see cref="IConstructor"/> that matches the given signature.</returns>
        IConstructor? OfExactSignature( IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null );

        // TODO: add this method
        // IConstructor? OfExactSignature( IReadOnlyList<Type> parameterTypes );

        /// <summary>
        /// Gets a constructor that exactly matches the signature of the specified method.
        /// </summary>
        /// <param name="signatureTemplate">Constructor signature of which to should be considered.</param>
        /// <returns>A <see cref="IConstructor"/> that matches the given signature.</returns>
        IConstructor? OfExactSignature( IConstructor signatureTemplate );

        // TODO: IMethod? OfBestSignature( ... )
    }
}