// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorDriverFactory : IValidatorDriverFactory
{
    private readonly Type _type;
    private readonly ConcurrentDictionary<string, ValidatorDriver> _drivers = new( StringComparer.Ordinal );
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

    public ValidatorDriver<TContext> GetValidatorDriver<TContext>( string name )
        => (ValidatorDriver<TContext>) this._drivers.GetOrAdd( name, this.GetValidatorDriverImpl<TContext> );

    private ValidatorDriver<TContext> GetValidatorDriverImpl<TContext>( string name )
    {
        var method = this._type.GetMethod( name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        if ( method == null )
        {
            // This should have been validated before. (TODO)
            throw new AssertionFailedException();
        }

        var instanceParameter = Expression.Parameter( typeof(object), "instance" );
        var contextParameter = Expression.Parameter( typeof(TContext).MakeByRefType(), "context" );

        var invocation = method.IsStatic
            ? Expression.Call( method, contextParameter )
            : Expression.Call( Expression.Convert( instanceParameter, this._type ), method, contextParameter );

        var lambda = Expression.Lambda<InvokeValidatorDelegate<TContext>>( invocation, instanceParameter, contextParameter );
        var compiled = lambda.Compile();

        return new ValidatorDriver<TContext>( this._type, method, compiled );
    }
}