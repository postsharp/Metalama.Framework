// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorDriverFactory : IValidatorDriverFactory
{
    private readonly Type _type;
    private readonly ConcurrentDictionary<string, ValidatorDriver> _drivers = new( StringComparer.Ordinal );

    public ValidatorDriverFactory( Type type )
    {
        this._type = type;
    }

    public ValidatorDriver GetValidatorDriver( string name, ValidatorKind kind )
    {
        switch ( kind )
        {
            case ValidatorKind.Definition:
                return this._drivers.GetOrAdd( name, this.GetDefinitionValidationDriver );

            case ValidatorKind.Reference:
                return this._drivers.GetOrAdd( name, this.GetReferenceValidationDriver );

            default:
                throw new AssertionFailedException();
        }
    }

    private ValidatorDriver GetReferenceValidationDriver( string name )
    {
        var method = this._type.GetMethod( name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        if ( method == null )
        {
            // This should have been validated before.
            throw new AssertionFailedException();
        }

        var instanceParameter = Expression.Parameter( typeof(object), "instance" );
        var contextParameter = Expression.Parameter( typeof(ValidateReferenceContext).MakeByRefType(), "context" );
        MethodCallExpression invocation;

        if ( method.IsStatic )
        {
            invocation = Expression.Call( method, contextParameter );
        }
        else
        {
            invocation = Expression.Call( Expression.Convert( instanceParameter, this._type ), method, contextParameter );
        }

        var lambda = Expression.Lambda<InvokeReferenceValidatorDelegate>( invocation, instanceParameter, contextParameter );
        var compiled = lambda.Compile();

        return new ReferenceValidatorDriver( compiled );
    }

    private ValidatorDriver GetDefinitionValidationDriver( string name )
    {
        throw new NotImplementedException();
    }
}