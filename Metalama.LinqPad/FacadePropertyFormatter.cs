// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using LINQPad;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Impl;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Metalama.LinqPad
{
    /// <summary>
    /// A helper class used by <see cref="FacadeObject"/> to format property values into LinqPad view objects.
    /// </summary>
    internal static class FacadePropertyFormatter
    {
        public static object FormatLazyPropertyValue( object owner, Type propertyType, Func<object, object?> getter )
        {
            if ( getter == null )
            {
                throw new ArgumentNullException( nameof(getter) );
            }

            if ( owner == null )
            {
                throw new ArgumentNullException( nameof(owner) );
            }

            return typeof(FacadePropertyFormatter)
                .GetMethod( nameof(CreateLazy), BindingFlags.Static | BindingFlags.NonPublic )
                .AssertNotNull()
                .MakeGenericMethod( propertyType )
                .Invoke( null, new object[] { new Func<object?>( () => getter( owner ) ) } )!;
        }

        public static object? FormatPropertyValue( object? value ) => FormatPropertyValueTestable( value ).View;

        public static (object? View, object? ViewModel) FormatPropertyValueTestable( object? value )
        {
            switch ( value )
            {
                case null:
                    return (null, null);

                case Permalink permalink:
                    return (permalink.Format(), null);

                case Severity severity:
                    {
                        var sign = severity switch
                        {
                            Severity.Warning => "\u26A0",
                            Severity.Error => "\u26D4",
                            Severity.Info => "\u2139",
                            Severity.Hidden => "\uD83D\uDEC7",
                            _ => ""
                        };

                        return (sign + " " + severity.ToString(), severity);
                    }
            }

            if ( value.GetType().IsEnum )
            {
                return (value.ToString(), null);
            }

            var groupingInterface = value.GetType().GetInterface( "System.Linq.IGrouping`2" );

            if ( groupingInterface != null )
            {
                value = typeof(FacadePropertyFormatter)
                    .GetMethod( nameof(CreateGrouping), BindingFlags.Static | BindingFlags.NonPublic )
                    .AssertNotNull()
                    .MakeGenericMethod( groupingInterface.GetGenericArguments() )
                    .Invoke( null, new[] { value } )!;
            }

            if ( IsComplexType( value ) )
            {
                return CreateHyperlinkTestable( value );
            }
            else
            {
                return (value, value);
            }
        }

        public static object? CreateHyperlink( object? value ) => value == null ? null : CreateHyperlinkTestable( value ).View;

        private static (object? View, object? ViewModel) CreateHyperlinkTestable( object value )
        {
            string? summary;
            string? cssClass = null;

            switch ( value )
            {
                case IEnumerable:
                    cssClass = "collection";

                    if ( EnumerableAccessor.Get( value.GetType() ) is { HasCount: true } accessor )
                    {
                        var count = accessor.GetCount( value );

                        switch ( count )
                        {
                            case 0:
                                // Did not managed to render this with some styling. The following did not work:
                                // Util.RawHtml, Util.WithStyle
                                return ("(empty)", value);

                            case 1:
                                summary = "(1 item)";

                                break;

                            default:
                                summary = $"({count} items)";

                                break;
                        }
                    }
                    else
                    {
                        summary = "(? items)";
                    }

                    break;

                default:
                    summary = value.ToString();

                    if ( string.IsNullOrWhiteSpace( summary ) )
                    {
                        summary = value.GetType().Name;
                    }

                    break;
            }

            var hyperlink = CreateHyperlink( value, summary, cssClass );

            return (hyperlink, value);
        }

        private static bool _alreadyAddedCss;

        public static object FormatException( Exception exception )
        {
            if ( !_alreadyAddedCss )
            {
                _alreadyAddedCss = true;
            }

            return CreateHyperlink( exception, exception.Message, "error" );
        }

        private static Lazy<T> CreateLazy<T>( Func<object?> func ) => new( () => (T) func()! );

        private static GroupingFacade<TKey, TItems> CreateGrouping<TKey, TItems>( IGrouping<TKey, TItems> group ) => new( group );

        private static object CreateHyperlink( object o, string summary, string? cssClass = null )
        {
            DumpContainer container = new();

            Hyperlinq link = new(
                () =>
                {
                    // On click, replace the link by the object content.
                    container.Content = o;
                },
                summary );

            if ( cssClass != null )
            {
                link.CssClass = cssClass;
            }

            container.Content = link;

            return container;
        }

        private static bool IsComplexType( object? obj )
        {
            if ( obj == null )
            {
                return false;
            }

            var type = obj.GetType();

            if ( type.IsPrimitive || type.IsEnum || type == typeof(string) )
            {
                return false;
            }

            return true;
        }
    }
}