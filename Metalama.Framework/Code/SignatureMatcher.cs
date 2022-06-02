// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code
{
    internal static class SignatureMatcher
    {
        /// <summary>
        /// Gets all members that match given requirements on signature.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="argumentGetter"/>.</typeparam>
        /// <typeparam name="TMember">Type of members.</typeparam>
        /// <param name="members">A collection of members.</param>
        /// <param name="payload">Payload object, passed to <paramref name="argumentGetter"/>.</param>
        /// <param name="name">Required name, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="argumentCount">Required number of parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="argumentGetter">Predicate for matching parameters.</param>
        /// <param name="isStatic">Specifies whether the staticity should be matched (it is normally not a part of signature).</param>
        /// <returns>Enumeration of all members matching all conditions.</returns>
        public static IEnumerable<TMember> OfCompatibleSignature<TMember, TPayload>(
            this IMemberCollection<TMember> members,
            TPayload payload,
            string? name,
            int? argumentCount,
            Func<TPayload, int, (IType? Type, RefKind? RefKind)> argumentGetter,
            bool? isStatic )
            where TMember : class, IMethodBase
        {
            var compilation = members.DeclaringType.Compilation;

            return OfSignature(
                members,
                (payload, argumentGetter, compilation),
                name,
                argumentCount,
                IsMatchingParameter,
                isStatic,
                true );

            static bool IsMatchingParameter(
                (TPayload InnerPayload, Func<TPayload, int, (IType? Type, RefKind? RefKind)>? ArgumentGetter, ICompilation Compilation) payload,
                int parameterIndex,
                IType expectedType,
                RefKind expectedRefKind )
            {
                var parameterInfo = payload.ArgumentGetter?.Invoke( payload.InnerPayload, parameterIndex );

                if ( parameterInfo == null )
                {
                    return true;
                }

                return
                    (parameterInfo.Value.Type == null || payload.Compilation.InvariantComparer.Is(
                        parameterInfo.Value.Type,
                        expectedType ))
                    && (parameterInfo.Value.RefKind == null || expectedRefKind == parameterInfo.Value.RefKind);
            }
        }

        /// <summary>
        /// Attempts to find a member with an exact match of the specified signature.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="parameterGetter"/>.</typeparam>
        /// <typeparam name="TMember">Type of members.</typeparam>
        /// <param name="members">A collection of members.</param>
        /// <param name="payload">Payload object, passed to <paramref name="parameterGetter"/>.</param>
        /// <param name="name">Required name of the method.</param>
        /// <param name="parameterCount">Required number of parameters.</param>
        /// <param name="parameterGetter">Delegate that gets <see cref="IType"/> and <see cref="RefKind"/> or a parameters on the gives index.</param>
        /// <param name="isStatic">Specifies whether the staticity should be matched (it is normally not a part of signature).</param>
        /// <returns>Member matching requirements or <see langword="null"/> if there is none.</returns>
        public static TMember? OfExactSignature<TMember, TPayload>(
            this IMemberCollection<TMember> members,
            TPayload payload,
            string? name,
            int parameterCount,
            Func<TPayload, int, (IType Type, RefKind RefKind)> parameterGetter,
            bool? isStatic )
            where TMember : class, IMethodBase
        {
            var compilation = members.DeclaringType.Compilation;

            var matching = OfSignature(
                members,
                (payload, parameterGetter, compilation),
                name,
                parameterCount,
                IsMatchingParameter,
                isStatic );

            // We use First and not Single because advices may provide many methods with same signature.
            // They do not make it to the final code, but they are stored in this class.
            return matching.FirstOrDefault();

            static bool IsMatchingParameter(
                (TPayload InnerPayload, Func<TPayload, int, (IType Type, RefKind RefKind)> ParameterGetter, ICompilation Compilation) payload,
                int parameterIndex,
                IType expectedType,
                RefKind expectedRefKind )
            {
                var parameterInfo = payload.ParameterGetter( payload.InnerPayload, parameterIndex );

                // TODO: This comparison does not work for generic type parameters.
                return
                    payload.Compilation.InvariantComparer.Equals( expectedType, parameterInfo.Type )
                    && expectedRefKind == parameterInfo.RefKind;
            }
        }

        /// <summary>
        /// Finds method bases in a list with signatures that match given arguments.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="parameterPredicate"/>.</typeparam>
        /// <typeparam name="TMember">Type of members.</typeparam>
        /// <param name="members">A collection of members.</param>
        /// <param name="payload">Payload object, passed to <paramref name="parameterPredicate"/>.</param>
        /// <param name="name">Required name, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="parameterCount">Required number of parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="parameterPredicate">Predicate for matching parameters.</param>
        /// <param name="isStatic">Required staticity, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="expandParams">If true, methods with <see langword="params" /> are treated as having the requested number of parameters if possible.</param>
        /// <returns>Enumeration of all members matching all conditions.</returns>
        private static IEnumerable<TMember> OfSignature<TMember, TPayload>(
            this IMemberCollection<TMember> members,
            TPayload payload,
            string? name,
            int? parameterCount,
            Func<TPayload, int, IType, RefKind, bool> parameterPredicate,
            bool? isStatic,
            bool expandParams = false )
            where TMember : class, IMethodBase
        {
            var candidates = name != null ? members.OfName( name ) : members;

            // Exclude any explicit interface implementation.
            // TODO: the Name be fully qualified, having it non-qualified is confusing and does not follow other implementations (28810).
            candidates = candidates.Where( c => !c.IsExplicitInterfaceImplementation );

            foreach ( var sourceItem in candidates )
            {
                if ( (isStatic != null && isStatic != sourceItem.IsStatic)
                     || (parameterCount != null && !expandParams && sourceItem.Parameters.Count != parameterCount)
                     || (parameterCount != null && expandParams && sourceItem.Parameters.Count > parameterCount + 1) )
                {
                    continue;
                }

                if ( parameterCount == null )
                {
                    yield return sourceItem;

                    continue;
                }

                var match = true;           // Determines whether the item matched all it's parameters, with exception of params.
                var tryMatchParams = false; // Determines whether the last parameter was params and whether we want to match rest of the arguments to it.

                if ( sourceItem.Parameters.Count > 0 )
                {
                    for ( var i = 0; i < sourceItem.Parameters.Count; i++ )
                    {
                        if ( sourceItem.Parameters[i].IsParams && expandParams && match )
                        {
                            if ( i != sourceItem.Parameters.Count - 1 || sourceItem.Parameters[i].Type.TypeKind != TypeKind.Array )
                            {
                                throw new InvalidOperationException( "Assertion failed." );
                            }

                            if ( expandParams )
                            {
                                tryMatchParams = true;
                            }
                        }

                        if ( i >= parameterCount )
                        {
                            match = false;

                            break;
                        }

                        if ( !parameterPredicate( payload, i, sourceItem.Parameters[i].Type, sourceItem.Parameters[i].RefKind ) )
                        {
                            match = false;

                            break;
                        }
                    }
                }

                if ( match )
                {
                    if ( !tryMatchParams && parameterCount != sourceItem.Parameters.Count )
                    {
                        // Will not be matching params and parameter counts don't match.
                        continue;
                    }

                    yield return sourceItem;
                }
                else if ( tryMatchParams )
                {
                    // Attempt to match C# params - all remaining parameter types should be assignable to the array element type.
                    var elementType = ((IArrayType) sourceItem.Parameters[sourceItem.Parameters.Count - 1].Type).ElementType;
                    var paramsMatch = true;

                    for ( var i = sourceItem.Parameters.Count - 1; i < parameterCount; i++ )
                    {
                        if ( !parameterPredicate( payload, i, elementType, RefKind.None ) )
                        {
                            paramsMatch = false;

                            break;
                        }
                    }

                    if ( paramsMatch )
                    {
                        yield return sourceItem;
                    }
                }
            }
        }
    }
}