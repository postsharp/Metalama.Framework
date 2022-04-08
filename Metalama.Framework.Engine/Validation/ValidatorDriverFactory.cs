// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorDriverFactory : IValidatorDriverFactory
{
    private readonly Type _type;
    private readonly ConcurrentDictionary<MethodInfo, ReferenceValidatorDriver> _drivers = new();
    private static readonly ConditionalWeakTable<Type, ValidatorDriverFactory> _instances = new();

    public static ValidatorDriverFactory GetInstance( Type type )
    {
        // The factory method is static, and does not depend on IServiceProvider nor is provided by IServiceProvider, because we want
        // to share driver instances across compilations and projects given the high cost of instantiating them.

        // ReSharper disable once InconsistentlySynchronizedField
        if ( _instances.TryGetValue( type, out var instance ) )
        {
            return instance;
        }
        else
        {
            lock ( _instances )
            {
                if ( _instances.TryGetValue( type, out instance ) )
                {
                    return instance;
                }
                else
                {
                    instance = new ValidatorDriverFactory( type );
                    _instances.Add( type, instance );
                }
            }
        }

        return instance;
    }

    private ValidatorDriverFactory( Type type )
    {
        this._type = type;
    }

    public ReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod )
        => this._drivers.GetOrAdd( validateMethod, this.GetReferenceValidatorDriverImpl );

    public DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate ) => new( validate );

    private ReferenceValidatorDriver GetReferenceValidatorDriverImpl( MethodInfo method )
    {
        var instanceParameter = Expression.Parameter( typeof(object), "instance" );
        var contextParameter = Expression.Parameter( typeof(ReferenceValidationContext).MakeByRefType(), "context" );

        var invocation = method.IsStatic
            ? Expression.Call( method, contextParameter )
            : Expression.Call( Expression.Convert( instanceParameter, this._type ), method, contextParameter );

        var lambda = Expression.Lambda<InvokeValidatorDelegate<ReferenceValidationContext>>( invocation, instanceParameter, contextParameter );
        var compiled = lambda.Compile();

        return new ReferenceValidatorDriver( this._type, method, compiled );
    }
}