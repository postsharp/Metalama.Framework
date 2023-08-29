// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Wraps a template provider type or instance.
/// </summary>
[CompileTime]
public readonly record struct TemplateProvider
{
    private readonly object? _value;

    private TemplateProvider( object instance )
    {
#if DEBUG

        // Frequent mistake.
        if ( instance is TemplateProvider )
        {
            throw new ArgumentOutOfRangeException();
        }
#endif
        this._value = instance;
    }

    public bool IsNull => this._value == null;

    private static void Verify( Type type )
    {
        for ( var t = type; t != null; t = t.BaseType )
        {
            if ( t.IsDefined( typeof(TemplateProviderAttribute), true ) )
            {
                return;
            }
        }

        if ( type.GetInterfaces().Any( t => t.IsDefined( typeof(TemplateProviderAttribute), true ) ) )
        {
            return;
        }

        throw new ArgumentOutOfRangeException( $"The type '{type}' must be annotated with {nameof(TemplateProviderAttribute)}." );
    }

    /// <summary>
    /// Creates a <see cref="TemplateProvider"/> from an object instance.
    /// </summary>
    /// <param name="instance">An instance of a type annotated with <see cref="TemplateProviderAttribute"/>.</param>
    public static TemplateProvider FromInstance( object instance )
    {
        Verify( instance.GetType() );

        return new TemplateProvider( instance );
    }

    internal static TemplateProvider FromInstanceUnsafe( object instance ) => new( instance );

    /// <summary>
    /// Creates a <see cref="TemplateProvider"/> from a type.
    /// </summary>
    /// <param name="type">A type annotated with <see cref="TemplateProviderAttribute"/>.</param>
    public static TemplateProvider FromType( Type type )
    {
        Verify( type );

        return new TemplateProvider( type );
    }

    internal object? Object => this._value is Type ? null : this._value ?? throw new ArgumentNullException();

    internal Type Type => this._value is Type type ? type : this._value?.GetType() ?? throw new ArgumentNullException();
}