// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class MethodBaseList<T> : MemberList<T, MemberLink<T>>
        where T : class, IMethodBase
    {
        // TODO: This should be further extracted into MemberList for (parameterized) property search.

        protected MethodBaseList()
        {
        }

        protected MethodBaseList( CodeElement? containingElement, IEnumerable<MemberLink<T>> sourceItems ) : base (containingElement, sourceItems)
        {
        }

        protected abstract MethodBaseList<T> GetMemberListForBaseClass( INamedType declaringType);

        protected abstract int GetGenericParameterCount( T x );

        protected IEnumerable<T> OfCompatibleSignature<TContext>( 
            TContext context, 
            string? name, 
            int? genericParameterCount, 
            int? argumentCount, 
            Func<TContext, int, (IType? Type, RefKind? RefKind)> argumentGetter, 
            bool? isStatic, 
            bool declaredOnly )
        {
            var compilation = this.Compilation;

            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                foreach ( var candidate in GetCandidates( this, context, name, genericParameterCount, argumentCount, argumentGetter, isStatic, compilation ) )
                {
                    yield return candidate;
                }
            }
            else
            {
                // TODO: There should be a generic context, which changes when descending to the base type.

                INamedType? currentType = namedType;
                var collectedMethods = new HashSet<T>( SignatureEqualityComparer.Instance );

                while ( currentType != null )
                {
                    foreach ( var candidate in GetCandidates( this.GetMemberListForBaseClass( currentType ), context, name, genericParameterCount, argumentCount, argumentGetter, isStatic, compilation ) )
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
                TContext context, 
                string? name, 
                int? genericParameterCount, 
                int? argumentCount, 
                Func<TContext, int, (IType? Type, RefKind? RefKind)>? argumentGetter,
                bool? isStatic,
                CompilationModel compilation )
            {
                return instance.OfSignature( (context, argumentGetter, compilation), name, genericParameterCount, argumentCount, IsMatchingParameter, isStatic, true );
            }

            static bool IsMatchingParameter(
                (TContext OuterContext, Func<TContext, int, (IType? Type, RefKind? RefKind)>? ArgumentGetter, CompilationModel Compilation) context,
                int parameterIndex,
                IType expectedType,
                RefKind expectedRefKind )
            {
                var parameterInfo = context.ArgumentGetter?.Invoke( context.OuterContext, parameterIndex );
                if ( parameterInfo == null )
                {
                    return true;
                }

                return
                    (parameterInfo.Value.Type == null || context.Compilation.InvariantComparer.Is( parameterInfo.Value.Type, expectedType ))
                    && (parameterInfo.Value.RefKind == null || expectedRefKind == parameterInfo.Value.RefKind);
            }
        }

        protected T? OfExactSignature<TContext>( 
            TContext context, 
            string? name, 
            int genericParameterCount, 
            int parameterCount, 
            Func<TContext, int, (IType Type, RefKind RefKind)> parameterGetter, 
            bool? isStatic, 
            bool declaredOnly )
        {
            var compilation = this.Compilation;
            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                return Get( this, context, name, genericParameterCount, parameterCount, parameterGetter, isStatic, compilation );
            }
            else
            {
                // TODO: There should be a generic context, which changes when descending to the base type.

                INamedType? currentType = namedType;
                while ( currentType != null )
                {
                    var candidate = Get( this.GetMemberListForBaseClass( currentType ), context, name, genericParameterCount, parameterCount, parameterGetter, isStatic, compilation );

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
                TContext context, 
                string? name, 
                int genericParameterCount, 
                int parameterCount, 
                Func<TContext, int, (IType Type, RefKind RefKind)> parameterGetter, 
                bool? isStatic,
                CompilationModel compilation )
            {
                return
                    instance.OfSignature(
                        (context, parameterGetter, compilation),
                        name,
                        genericParameterCount,
                        parameterCount,
                        IsMatchingParameter,
                        isStatic )
                    .SingleOrDefault();
            }

            static bool IsMatchingParameter( 
                (TContext OuterContext, Func<TContext, int, (IType Type, RefKind RefKind)> ParameterGetter, CompilationModel Compilation) context, 
                int parameterIndex, 
                IType expectedType, 
                RefKind expectedRefKind )
            {
                var parameterInfo = context.ParameterGetter( context.OuterContext, parameterIndex );
                return
                    context.Compilation.InvariantComparer.Equals( expectedType, parameterInfo.Type )
                    && expectedRefKind == parameterInfo.RefKind;
            }
        }

        protected IEnumerable<T> OfSignature<TContext>( 
            TContext context, 
            string? name, 
            int? genericParameterCount, 
            int? parameterCount, 
            Func<TContext, int, IType, RefKind, bool> parameterPredicate, 
            bool? isStatic, 
            bool expandParams = false )
        {
            var compilation = this.Compilation;
            IEnumerable<T> candidates;
            if (name != null )
            {
                candidates = this.OfName(name);
            }
            else
            {
                candidates = ForCompilation( this.SourceItems, compilation );

                static IEnumerable<T> ForCompilation( ImmutableArray<MemberLink<T>> sourceItems, CompilationModel compilation)
                {
                    for (var i = 0; i < sourceItems.Length; i++ )
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

                var match = true; // Determines whether the item matched all it's parameters, with exception of params.
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

                        if ( !parameterPredicate( context, i, sourceItem.Parameters[i].ParameterType, sourceItem.Parameters[i].RefKind ) )
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
                        if ( !parameterPredicate( context, i, elementType, RefKind.None ) )
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
            public static SignatureEqualityComparer Instance { get; } = new SignatureEqualityComparer();

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

                if (x is IMethod xm)
                {
                    hashCode = HashCode.Combine( hashCode, xm.GenericParameters.Count );
                }

                for ( var i = 0; i < x.Parameters.Count; i++ )
                {
                    hashCode = HashCode.Combine(
                        hashCode,
                        x.Compilation.InvariantComparer.GetHashCode( x.Parameters[i].ParameterType ),
                        x.Parameters[i].RefKind );
                }

                return hashCode;
            }
        }
    }
}
