// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl;
using LINQPad;
using System;
using System.Collections;
using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal static class PropertyValueFormatter
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

            return typeof(PropertyValueFormatter)
                .GetMethod( nameof(CreateLazy), BindingFlags.Static | BindingFlags.NonPublic )
                .AssertNotNull()
                .MakeGenericMethod( propertyType )
                .Invoke( null, new object[] { new Func<object?>( () => getter( owner ) ) } )!;
        }

        public static object? FormatPropertyValue( object? value )
        {
            if ( value is Permalink permalink )
            {
                return permalink.Format();
            }
            else if ( value is Severity severity )
            {
                var sign = severity switch
                {
                    Severity.Warning => "\u26A0",
                    Severity.Error => "\uD83D\uDEC7",
                    Severity.Info => "\u2139",
                    Severity.Hidden => "",
                    _ => ""
                };

                return sign + " " + severity.ToString();
            }

            if ( IsComplexType( value ) )
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
                                    return "(empty)";

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
                        summary = value!.ToString();

                        break;
                }

                return CreateSummary( value, summary!, cssClass );
            }
            else
            {
                return value;
            }
        }

        private static bool _alreadyAddedCss;

        public static object FormatException( Exception exception )
        {
            if ( !_alreadyAddedCss )
            {
                _alreadyAddedCss = true;
            }

            return CreateSummary( exception, exception.Message, "error" );
        }

        private static Lazy<T> CreateLazy<T>( Func<object?> func ) => new( () => (T) func()! );

        private static object CreateSummary( object o, string summary, string? cssClass = null )
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