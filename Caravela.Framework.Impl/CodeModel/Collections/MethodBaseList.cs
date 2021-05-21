// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class MethodBaseList<T> : MemberList<T, MemberRef<T>>
        where T : class, IMethodBase
    {
        // TODO: This should be further extracted into MemberList for (parameterized) property search.
        // TODO: Verify that passing a delegate to a static method does not result in allocation.

        protected MethodBaseList() { }

        protected MethodBaseList( Declaration? containingElement, IEnumerable<MemberRef<T>> sourceItems ) : base( containingElement, sourceItems ) { }

        /// <summary>
        /// Gets the member list for a given type.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        protected abstract MethodBaseList<T> GetMemberListForType( INamedType declaringType );

        /// <summary>
        /// Gets the number of generic parameters for the given member.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected abstract int GetGenericParameterCount( T x );

        /// <summary>
        /// Gets all members that match given requirements on signature.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="argumentGetter"/>.</typeparam>
        /// <param name="payload">Payload object, passed to <paramref name="argumentGetter"/>.</param>
        /// <param name="name">Required name, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="genericParameterCount">Required number of generic parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="argumentCount">Required number of parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="argumentGetter">Predicate for matching parameters.</param>
        /// <param name="isStatic">Specifies whether the staticity should be matched (it is normally not a part of signature).</param>
        /// <param name="declaredOnly">Specifies whether declarations in base classes should be taken into account.</param>
        /// <returns>Enumeration of all members matching all conditions.</returns>
        protected IEnumerable<T> OfCompatibleSignature<TPayload>(
            TPayload payload,
            string? name,
            int? genericParameterCount,
            int? argumentCount,
            Func<TPayload, int, (IType? Type, RefKind? RefKind)> argumentGetter,
            bool? isStatic,
            bool declaredOnly )
        {
            var compilation = this.Compilation;

            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                foreach ( var candidate in GetCandidates( this, payload, name, genericParameterCount, argumentCount, argumentGetter, isStatic, compilation ) )
                {
                    yield return candidate;
                }
            }
            else
            {
                // TODO: (#28475) There should be a generic context, which changes when descending to the base type.

                // Descent into base types and collect methods with previously unseen signatures.
                INamedType? currentType = namedType;
                var collectedMethods = new HashSet<T>( SignatureEqualityComparer.Instance );

                while ( currentType != null )
                {
                    foreach ( var candidate in GetCandidates(
                        this.GetMemberListForType( currentType ),
                        payload,
                        name,
                        genericParameterCount,
                        argumentCount,
                        argumentGetter,
                        isStatic,
                        compilation ) )
                    {
                        if ( collectedMethods.Add( candidate ) )
                        {
                            yield return candidate;
                        }
                    }

                    currentType = currentType.BaseType;
                }
            }

            static IEnumerable<T> GetCandidates(
                MethodBaseList<T> instance,
                TPayload payload,
                string? name,
                int? genericParameterCount,
                int? argumentCount,
                Func<TPayload, int, (IType? Type, RefKind? RefKind)>? argumentGetter,
                bool? isStatic,
                CompilationModel compilation )
            {
                return instance.OfSignature(
                    (payload, argumentGetter, compilation),
                    name,
                    genericParameterCount,
                    argumentCount,
                    IsMatchingParameter,
                    isStatic,
                    true );
            }

            static bool IsMatchingParameter(
                (TPayload InnerPayload, Func<TPayload, int, (IType? Type, RefKind? RefKind)>? ArgumentGetter, CompilationModel Compilation) payload,
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
                    (parameterInfo.Value.Type == null || payload.Compilation.InvariantComparer.Is( parameterInfo.Value.Type, expectedType ))
                    && (parameterInfo.Value.RefKind == null || expectedRefKind == parameterInfo.Value.RefKind);
            }
        }

        /// <summary>
        /// Attempts to find a member with an exact match of the specified signature.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="parameterGetter"/>.</typeparam>
        /// <param name="payload">Payload object, passed to <paramref name="parameterGetter"/>.</param>
        /// <param name="name">Required name of the method.</param>
        /// <param name="genericParameterCount">Required number of generic parameters.</param>
        /// <param name="parameterCount">Required number of parameters.</param>
        /// <param name="parameterGetter">Delegate that gets <see cref="IType"/> and <see cref="RefKind"/> or a parameters on the gives index.</param>
        /// <param name="isStatic">Specifies whether the staticity should be matched (it is normally not a part of signature).</param>
        /// <param name="declaredOnly">Specifies whether declarations in base classes should be taken into account.</param>
        /// <returns>Member matching requirements or <see langword="null"/> if there is none.</returns>
        protected T? OfExactSignature<TPayload>(
            TPayload payload,
            string? name,
            int genericParameterCount,
            int parameterCount,
            Func<TPayload, int, (IType Type, RefKind RefKind)> parameterGetter,
            bool? isStatic,
            bool declaredOnly )
        {
            var compilation = this.Compilation;

            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                return Get( this, payload, name, genericParameterCount, parameterCount, parameterGetter, isStatic, compilation );
            }
            else
            {
                // TODO: There should be a generic context, which changes when descending to the base type.

                // Descent into base types and collect methods with previously unseen signatures.
                INamedType? currentType = namedType;

                while ( currentType != null )
                {
                    var candidate = Get(
                        this.GetMemberListForType( currentType ),
                        payload,
                        name,
                        genericParameterCount,
                        parameterCount,
                        parameterGetter,
                        isStatic,
                        compilation );

                    if ( candidate != null )
                    {
                        return candidate;
                    }

                    currentType = currentType.BaseType;
                }

                return null;
            }

            static T? Get(
                MethodBaseList<T> instance,
                TPayload payload,
                string? name,
                int genericParameterCount,
                int parameterCount,
                Func<TPayload, int, (IType Type, RefKind RefKind)> parameterGetter,
                bool? isStatic,
                CompilationModel compilation )
            {
                return
                    instance.OfSignature(
                            (payload, parameterGetter, compilation),
                            name,
                            genericParameterCount,
                            parameterCount,
                            IsMatchingParameter,
                            isStatic )
                        .SingleOrDefault();
            }

            static bool IsMatchingParameter(
                (TPayload InnerPayload, Func<TPayload, int, (IType Type, RefKind RefKind)> ParameterGetter, CompilationModel Compilation) payload,
                int parameterIndex,
                IType expectedType,
                RefKind expectedRefKind )
            {
                var parameterInfo = payload.ParameterGetter( payload.InnerPayload, parameterIndex );

                return
                    payload.Compilation.InvariantComparer.Equals( expectedType, parameterInfo.Type )
                    && expectedRefKind == parameterInfo.RefKind;
            }
        }

        /// <summary>
        /// Finds method bases in a list with signatures that match given arguments.
        /// </summary>
        /// <typeparam name="TPayload">Payload type for the <paramref name="parameterPredicate"/>.</typeparam>
        /// <param name="payload">Payload object, passed to <paramref name="parameterPredicate"/>.</param>
        /// <param name="name">Required name, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="genericParameterCount">Required number of generic parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="parameterCount">Required number of parameters, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="parameterPredicate">Predicate for matching parameters.</param>
        /// <param name="isStatic">Required staticity, or <see langword="null"/> if there is no requirement.</param>
        /// <param name="expandParams">If true, methods with <see langword="params" /> are treated as having the requested number of parameters if possible.</param>
        /// <returns>Enumeration of all members matching all conditions.</returns>
        protected IEnumerable<T> OfSignature<TPayload>(
            TPayload payload,
            string? name,
            int? genericParameterCount,
            int? parameterCount,
            Func<TPayload, int, IType, RefKind, bool> parameterPredicate,
            bool? isStatic,
            bool expandParams = false )
        {
            var compilation = this.Compilation;
            IEnumerable<T> candidates;

            if ( name != null )
            {
                candidates = this.OfName( name );
            }
            else
            {
                candidates = ForCompilation( this.SourceItems, compilation );

                static IEnumerable<T> ForCompilation( ImmutableArray<MemberRef<T>> sourceItems, CompilationModel compilation )
                {
                    for ( var i = 0; i < sourceItems.Length; i++ )
                    {
                        yield return sourceItems[i].GetForCompilation( compilation );
                    }
                }
            }

            foreach ( var sourceItem in candidates )
            {
                if ( (isStatic != null && isStatic != sourceItem.IsStatic)
                     || (genericParameterCount != null && this.GetGenericParameterCount( sourceItem ) != genericParameterCount)
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
                            if ( i != sourceItem.Parameters.Count - 1 || sourceItem.Parameters[i].ParameterType.TypeKind != TypeKind.Array )
                            {
                                throw new AssertionFailedException();
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

                        if ( !parameterPredicate( payload, i, sourceItem.Parameters[i].ParameterType, sourceItem.Parameters[i].RefKind ) )
                        {
                            match = false;

                            break;
                        }
                    }
                }

                if ( match && !tryMatchParams && parameterCount != sourceItem.Parameters.Count )
                {
                    // Will not be matching params and parameter counts don't match.
                    continue;
                }

                if ( match )
                {
                    yield return sourceItem;
                }
                else if ( tryMatchParams )
                {
                    // Attempt to match C# params - all remaining parameter types should be assignable to the array element type.
                    var elementType = ((IArrayType) sourceItem.Parameters[sourceItem.Parameters.Count - 1].ParameterType).ElementType.AssertNotNull();
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

        // TODO: Not sure if this should be able to accept both IConstructor and IMethod or we should have two implementations.
        protected class SignatureEqualityComparer : IEqualityComparer<T>
        {
            public static SignatureEqualityComparer Instance { get; } = new();

            public bool Equals( T x, T y )
            {
                if ( !StringComparer.Ordinal.Equals( x.Name, y.Name )
                     || (x is IMethod xm && y is IMethod ym && xm.GenericParameters.Count != ym.GenericParameters.Count)
                     || x.Parameters.Count != y.Parameters.Count )
                {
                    return false;
                }

                for ( var i = 0; i < x.Parameters.Count; i++ )
                {
                    if ( !x.Compilation.InvariantComparer.Equals( x.Parameters[i].ParameterType, y.Parameters[i].ParameterType )
                         || x.Parameters[i].RefKind != y.Parameters[i].RefKind )
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode( T x )
            {
                var hashCode = HashCode.Combine( x.Name, x.Parameters.Count );

                if ( x is IMethod xm )
                {
                    hashCode = HashCode.Combine( hashCode, xm.GenericParameters.Count );
                }

                foreach ( var parameter in x.Parameters )
                {
                    hashCode = HashCode.Combine(
                        hashCode,
                        x.Compilation.InvariantComparer.GetHashCode( parameter.ParameterType ),
                        parameter.RefKind );
                }

                return hashCode;
            }
        }
    }
}