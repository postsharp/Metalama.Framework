// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MemberList<IMethod, MemberLink<IMethod>>, IMethodList
    {

        public static MethodList Empty { get; } = new MethodList();

        private MethodList()
        {
        }

        public MethodList( CodeElement? containingElement, IEnumerable<MemberLink<IMethod>> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        public IEnumerable<IMethod> OfCompatibleSignature( string name, int? genericParameterCount, IReadOnlyList<Type?>? argumentTypes, bool declaredOnly = true )
        {
            return this.OfCompatibleSignature( (argumentTypes, this.ContainingElement.AssertNotNull().Compilation), name, genericParameterCount, argumentTypes?.Count, GetParameter, declaredOnly );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<Type?>? ArgumentTypes, ICompilation Compilation) context, int index )
                => context.ArgumentTypes != null && context.ArgumentTypes[index] != null
                   ? (context.Compilation.TypeFactory.GetTypeByReflectionType( context.ArgumentTypes[index].AssertNotNull() ), null)
                   : (null, null);
        }

        public IEnumerable<IMethod> OfCompatibleSignature( string name, int? genericParameterCount = null, IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null, bool declaredOnly = true )
        {
            return this.OfCompatibleSignature( (argumentTypes, refKinds), name, genericParameterCount, argumentTypes?.Count, GetParameter, declaredOnly );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<IType?>? ArgumentTypes, IReadOnlyList<RefKind?>? RefKinds) context, int index )
                => (context.ArgumentTypes?[index], context.RefKinds?[index]);
        }

        private IEnumerable<IMethod> OfCompatibleSignature<TContext>( TContext context, string name, int? genericParameterCount, int? argumentCount, Func<TContext, int, (IType? Type, RefKind? RefKind)> argumentGetter, bool declaredOnly )
        {
            var compilation = this.Compilation;

            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                foreach ( var candidate in GetCandidates( this, context, name, genericParameterCount, argumentCount, argumentGetter, compilation ) )
                {
                    yield return candidate;
                }
            }
            else
            {
                INamedType? currentType = namedType;
                var collectedMethods = new HashSet<IMethod>( SignatureEqualityComparer.Instance );

                while ( currentType != null )
                {
                    foreach ( var candidate in GetCandidates( this, context, name, genericParameterCount, argumentCount, argumentGetter, compilation ) )
                    {
                        if ( collectedMethods.Add( candidate ) )
                        {
                            yield return candidate;
                        }
                    }

                    currentType = currentType.BaseType;
                }
            }

            static IEnumerable<IMethod> GetCandidates( MethodList instance, TContext context, string name, int? genericParameterCount, int? argumentCount, Func<TContext, int, (IType? Type, RefKind? RefKind)>? argumentGetter, CompilationModel compilation )
            {
                return instance.OfSignature( (context, argumentGetter, compilation), name, genericParameterCount, argumentCount, IsMatchingParameter, true );
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

        public IMethod? OfExactSignature( IMethod signatureTemplate, bool declaredOnly = true )
        {
            return this.OfExactSignature( signatureTemplate, signatureTemplate.Name, signatureTemplate.GenericParameters.Count, signatureTemplate.Parameters.Count, GetParameter, declaredOnly );

            static (IType Type, RefKind RefKind) GetParameter( IMethod context, int index )
                => (context.Parameters[index].ParameterType, context.Parameters[index].RefKind);
        }

        public IMethod? OfExactSignature( string name, int genericParameterCount, IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null, bool declaredOnly = true )
        {
            return this.OfExactSignature( (parameterTypes, refKinds), name, genericParameterCount, parameterTypes.Count, GetParameter, declaredOnly );

            static (IType Type, RefKind RefKind) GetParameter( (IReadOnlyList<IType> ParameterTypes, IReadOnlyList<RefKind>? RefKinds) context, int index )
                => (context.ParameterTypes[index], context.RefKinds?[index] ?? RefKind.None);
        }

        private IMethod? OfExactSignature<TContext>( TContext context, string name, int genericParameterCount, int parameterCount, Func<TContext, int, (IType Type, RefKind RefKind)> parameterGetter, bool declaredOnly )
        {
            var compilation = this.Compilation;
            if ( declaredOnly || this.ContainingElement is not NamedType namedType || namedType.BaseType == null )
            {
                return Get( this, context, name, genericParameterCount, parameterCount, parameterGetter, compilation );
            }
            else
            {
                INamedType? currentType = namedType;
                while ( currentType != null )
                {
                    var candidate = Get( (MethodList)currentType.Methods, context, name, genericParameterCount, parameterCount, parameterGetter, compilation );

                    if ( candidate != null )
                    {
                        return candidate;
                    }

                    currentType = currentType.BaseType;
                }

                return null;
            }

            static IMethod? Get( MethodList instance, TContext context, string name, int genericParameterCount, int parameterCount, Func<TContext, int, (IType Type, RefKind RefKind)> parameterGetter, CompilationModel compilation )
            {
                return
                    instance.OfSignature(
                        (context, parameterGetter, compilation),
                        name,
                        genericParameterCount,
                        parameterCount,
                        IsMatchingParameter )
                    .SingleOrDefault();
            }

            static bool IsMatchingParameter( (TContext OuterContext, Func<TContext, int, (IType Type, RefKind RefKind)> ParameterGetter, CompilationModel Compilation) context, int parameterIndex, IType expectedType, RefKind expectedRefKind )
            {
                var parameterInfo = context.ParameterGetter( context.OuterContext, parameterIndex );
                return
                    context.Compilation.InvariantComparer.Equals( expectedType, parameterInfo.Type )
                    && expectedRefKind == parameterInfo.RefKind;
            }
        }

        private IEnumerable<IMethod> OfSignature<TContext>( TContext context, string name, int? genericParameterCount, int? parameterCount, Func<TContext, int, IType, RefKind, bool> parameterPredicate, bool expandParams = false )
        {            
            var compilation = this.Compilation;
            foreach ( var sourceItem in this.SourceItems )
            {
                var projectedItem = sourceItem.GetForCompilation( compilation );

                if ( projectedItem.Name != name
                    || (genericParameterCount != null && projectedItem.GenericParameters.Count != genericParameterCount)
                    || (parameterCount != null && !expandParams && projectedItem.Parameters.Count != parameterCount)
                    || (parameterCount != null && expandParams && projectedItem.Parameters.Count > parameterCount + 1) )
                {
                    continue;
                }

                if ( parameterCount == null )
                {
                    yield return projectedItem;
                    continue;
                }

                var match = true;
                var tryMatchParams = false;

                if ( projectedItem.Parameters.Count > 0 )
                {
                    for ( var i = 0; i < projectedItem.Parameters.Count; i++ )
                    {
                        if ( projectedItem.Parameters[i].IsParams && expandParams && match )
                        {
                            if ( i != projectedItem.Parameters.Count - 1 || projectedItem.Parameters[i].ParameterType.TypeKind != TypeKind.Array )
                            {
                                throw new AssertionFailedException();
                            }

                            if ( expandParams )
                            {
                                tryMatchParams = true;
                            }

                            break;
                        }

                        if ( i >= parameterCount )
                        {
                            match = false;
                            break;
                        }

                        if ( !parameterPredicate( context, i, projectedItem.Parameters[i].ParameterType, projectedItem.Parameters[i].RefKind ) )
                        {
                            match = false;
                            break;
                        }
                    }
                }

                if (match && !tryMatchParams && parameterCount != projectedItem.Parameters.Count)
                {
                    // Not be matching params and parameter counts don't match.
                    continue;
                }

                if ( match )
                {
                    yield return projectedItem;
                }
                else if ( tryMatchParams )
                {
                    // Attempt to match C# params - all remaining parameter types should match the array element type.
                    var elementType = ((IArrayType) projectedItem.Parameters[projectedItem.Parameters.Count - 1].ParameterType).ElementType.AssertNotNull();
                    var paramsMatch = true;

                    for ( var i = projectedItem.Parameters.Count - 1; i < parameterCount; i++ )
                    {
                        if (!parameterPredicate(context, i, elementType, RefKind.None))
                        {
                            paramsMatch = false;
                            break;
                        }
                    }

                    if (paramsMatch)
                    {
                        yield return projectedItem;
                    }
                }
            }
        }

        private class SignatureEqualityComparer : IEqualityComparer<IMethod>
        {
            public static SignatureEqualityComparer Instance { get; } = new SignatureEqualityComparer();

            public bool Equals( IMethod x, IMethod y )
            {
                if (!StringComparer.Ordinal.Equals(x.Name, y.Name)
                    || x.GenericParameters.Count != y.GenericParameters.Count
                    || x.Parameters.Count != y.Parameters.Count)
                {
                    return false;
                }

                for (var i = 0; i < x.Parameters.Count; i++ )
                {
                    if ( !x.Compilation.InvariantComparer.Equals( x.Parameters[i].ParameterType, y.Parameters[i].ParameterType )
                        || x.Parameters[i].RefKind != y.Parameters[i].RefKind )
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode( IMethod x )
            {
                var hashCode = HashCode.Combine( x.Name, x.GenericParameters.Count, x.Parameters.Count );

                for ( var i = 0; i < x.Parameters.Count; i++ )
                {
                    hashCode = HashCode.Combine(
                        x.Compilation.InvariantComparer.GetHashCode( x.Parameters[i].ParameterType ),
                        x.Parameters[i].RefKind );
                }

                return hashCode;
            }
        }
    }
}