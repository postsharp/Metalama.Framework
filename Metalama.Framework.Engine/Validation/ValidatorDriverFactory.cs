// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Validation;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ValidatorDriverFactory : IValidatorDriverFactory, IDisposable
{
    private static readonly WeakCache<Type, ClassBasedReferenceValidatorDriver> _classBasedDrivers = new();
    private static readonly WeakCache<Type, ValidatorDriverFactory> _instances = new();

    private readonly Type _aspectOrFabricType;
    private readonly WeakCache<MethodInfo, MethodBasedReferenceValidatorDriver> _methodBasedDrivers = new();

    public static ValidatorDriverFactory GetInstance( Type aspectOrFabricType )
    {
        // The factory method is static, and does not depend on IServiceProvider nor is provided by IServiceProvider, because we want
        // to share driver instances across compilations and projects given the high cost of instantiating them.

        return _instances.GetOrAdd( aspectOrFabricType, t => new ValidatorDriverFactory( t ) );
    }

    private ValidatorDriverFactory( Type aspectOrFabricType )
    {
        this._aspectOrFabricType = aspectOrFabricType;
    }

    public MethodBasedReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod )
        => this._methodBasedDrivers.GetOrAdd( validateMethod, this.GetMethodBasedReferenceValidatorDriverCore );

    public ClassBasedReferenceValidatorDriver GetReferenceValidatorDriver( Type type )
        => _classBasedDrivers.GetOrAdd( type, t => ClassBasedReferenceValidatorDriver.Instance );

    public DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate ) => new( validate );

    private MethodBasedReferenceValidatorDriver GetMethodBasedReferenceValidatorDriverCore( MethodInfo method )
    {
        var instanceParameter = Expression.Parameter( typeof(object), "instance" );
        var contextParameter = Expression.Parameter( typeof(ReferenceValidationContext).MakeByRefType(), "context" );

        var invocation = method.IsStatic
            ? Expression.Call( method, contextParameter )
            : Expression.Call( Expression.Convert( instanceParameter, this._aspectOrFabricType ), method, contextParameter );

        var lambda = Expression.Lambda<InvokeValidatorDelegate<ReferenceValidationContext>>( invocation, instanceParameter, contextParameter );
        var compiled = lambda.Compile();

        return new MethodBasedReferenceValidatorDriver( method, compiled );
    }

    public void Dispose() => this._methodBasedDrivers.Dispose();
}