// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// Builds and represents the list of properties of a <see cref="ObjectReader"/>.
/// </summary>
internal sealed class ObjectReaderTypeAdapter
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly ImmutableDictionary<string, Func<object, object?>> _properties;
    private readonly UserCodeInvoker _userCodeInvoker;

    public IEnumerable<string> Properties => this._properties.Keys;

    public int PropertyCount => this._properties.Count;

    public bool TryGetValue( string key, object obj, out object? value )
    {
        if ( !this._properties.TryGetValue( key, out var property ) )
        {
            value = null;

            return false;
        }

        value = this._userCodeInvoker.Invoke(
            () => property( obj ),
            new UserCodeExecutionContext(
                this._serviceProvider,
                UserCodeDescription.Create( "evaluating the {0} field or property", key ) ) );

        return true;
    }

    internal ObjectReaderTypeAdapter( in ProjectServiceProvider serviceProvider, Type type )
    {
        this._serviceProvider = serviceProvider;
        this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();

        Dictionary<string, Func<object, object?>> properties = new();

        if ( type.IsInterface )
        {
            throw new ArgumentOutOfRangeException( nameof(type), "The type cannot be an interface." );
        }

        foreach ( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
        {
            var getter = property.GetMethod;

            if ( getter == null || getter.GetParameters().Length != 0 )
            {
                continue;
            }

            properties[property.Name] = CreateCompiledGetter( property );
        }

        foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.Instance ) )
        {
            properties[field.Name] = CreateCompiledGetter( field );
        }

        this._properties = properties.ToImmutableDictionary();
    }

    private static Func<object, object?> CreateCompiledGetter( MemberInfo member )
    {
        var parameter = Expression.Parameter( typeof(object) );
        var castParameter = Expression.Convert( parameter, member.DeclaringType! );
        var getProperty = Expression.Convert( Expression.PropertyOrField( castParameter, member.Name ), typeof(object) );
        var lambda = Expression.Lambda<Func<object, object?>>( getProperty, parameter ).Compile();

        return lambda;
    }

    public bool ContainsProperty( string key ) => this._properties.ContainsKey( key );
}