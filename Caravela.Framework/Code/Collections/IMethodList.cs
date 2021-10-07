// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IMethod"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IMethodList : IMemberList<IMethod>
    {
        /// <summary>
        /// Gets the list of methods with signatures compatible with specified constraints.
        /// </summary>
        /// <param name="name">Name of the method.</param>
        /// <param name="argumentTypes">Constraint on reflection types of arguments. <c>Null</c>items in the list signify any type.</param>
        /// <param name="isStatic">Constraint on staticity of the method.</param>
        /// <param name="declaredOnly"><c>True</c> if only declared methods should be considered or <c>false</c> if all methods, including those declared in base types should be considered.</param>
        /// <returns>Enumeration of methods matching specified constraints. If <paramref name="declaredOnly" /> is set to <c>false</c>, only the top-most visible method of the same signature is included.</returns>
        IEnumerable<IMethod> OfCompatibleSignature(
            string name,
            IReadOnlyList<Type?>? argumentTypes,
            bool? isStatic = false,
            bool declaredOnly = true );

        /// <summary>
        /// Gets the list of methods with signatures compatible with specified constraints.
        /// </summary>
        /// <param name="name">Name of the method.</param>
        /// <param name="argumentTypes">Constraint on types of arguments. <c>Null</c>items in the list signify any type.</param>
        /// <param name="refKinds">Constraint on reference kinds of arguments. <c>Null</c>items in the list signify any reference kind.</param>
        /// <param name="isStatic">Constraint on staticity of the method.</param>
        /// <param name="declaredOnly"><c>True</c> if only declared methods should be considered or <c>false</c> if all methods, including those declared in base types should be considered.</param>
        /// <returns>Enumeration of methods matching specified constraints. If <paramref name="declaredOnly" /> is set to <c>false</c>, only the top-most visible method of the same signature is included.</returns>
        IEnumerable<IMethod> OfCompatibleSignature(
            string name,
            IReadOnlyList<IType?>? argumentTypes,
            IReadOnlyList<RefKind?>? refKinds = null,
            bool? isStatic = false,
            bool declaredOnly = true );

        /// <summary>
        /// Gets a method that exactly matches the specified signature.
        /// </summary>
        /// <param name="name">Name of the method.</param>
        /// <param name="parameterTypes">List of parameter types.</param>
        /// <param name="refKinds">List of parameter reference kinds, or <c>null</c> if all parameters should be by-value.</param>
        /// <param name="isStatic">Staticity of the method.</param>
        /// <param name="declaredOnly"><c>True</c> if only declared methods should be considered or <c>false</c> if all methods, including those declared in base types should be considered.</param>
        /// <returns>A <see cref="IMethod"/> that matches the given signature. If <paramref name="declaredOnly" /> is set to <c>false</c>, the top-most visible method is shown.</returns>
        IMethod? OfExactSignature(
            string name,
            IReadOnlyList<IType> parameterTypes,
            IReadOnlyList<RefKind>? refKinds = null,
            bool? isStatic = null,
            bool declaredOnly = true );

        // TODO: Add this method:
        // IMethod? OfExactSignature(
        //     string name,
        //     int genericParameterCount,
        //     IReadOnlyList<Type> parameterTypes,
        //     bool? isStatic = null,
        //     bool declaredOnly = true );

        /// <summary>
        /// Gets a method that exactly matches the signature of the specified method.
        /// </summary>
        /// <param name="signatureTemplate">Method signature of which to should be considered.</param>
        /// <param name="matchIsStatic">Value indicating whether the staticity of the method should be matched.</param>
        /// <param name="declaredOnly"><c>True</c> if only declared methods should be considered or <c>false</c> if all methods, including those declared in base types should be considered.</param>
        /// <returns>A <see cref="IMethod"/> that matches the given signature. If <paramref name="declaredOnly" /> is set to <c>false</c>, the top-most visible method is shown.</returns>
        IMethod? OfExactSignature( IMethod signatureTemplate, bool matchIsStatic = true, bool declaredOnly = true );

        /// <summary>
        /// Gets the list of methods of a given <see cref="MethodKind"/> (such as <see cref="MethodKind.ConversionOperator"/> or <see cref="MethodKind.Default"/>.
        /// </summary>
        IEnumerable<IMethod> OfKind( MethodKind kind );

        // TODO: IMethod? OfBestSignature( ... ) - but that would mean recreating C# overload resolution which is hard. Roslyn code is not reusable directly (OverloadResolution.cs works with internal Symbols, not with ISymbols).
    }
}