// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Reflection;

namespace Caravela.Framework.Impl.Utilities.Dump
{
    public class LinqPadFormatter : IDumpFormatter
    {
        public virtual object FormatLazyPropertyValue( object owner, MethodInfo getter )
        {
            if ( getter == null )
            {
                throw new ArgumentNullException( nameof(getter) );
            }

            if ( owner == null )
            {
                throw new ArgumentNullException( nameof(owner) );
            }

            return typeof(LinqPadFormatter)
                .GetMethod( nameof(CreateLazy), BindingFlags.Static | BindingFlags.NonPublic )
                .AssertNotNull()
                .MakeGenericMethod( getter.ReturnType )
                .Invoke( this, new object[] { new Func<object>( () => getter.Invoke( owner, null ) ) } );
        }

        public virtual object FormatPropertyValue( object value, MethodInfo getter )
        {
            if ( getter == null )
            {
                throw new ArgumentNullException( nameof(getter) );
            }

            if ( IsComplexType( value ) )
            {
                string? summary;

                switch ( value )
                {
                    case IEnumerable:

                        if ( EnumerableAccessor.Get( value.GetType() ) is { HasCount: true } accessor )
                        {
                            var count = accessor.GetCount( value );

                            switch ( count )
                            {
                                case 0:
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
                        summary = value.ToString();

                        break;
                }

                return typeof(LinqPadFormatter)
                    .GetMethod( nameof(this.CreateSummary), BindingFlags.Instance | BindingFlags.NonPublic )
                    .AssertNotNull()
                    .MakeGenericMethod( getter.ReturnType )
                    .Invoke( this, new[] { value, summary } );
            }
            else
            {
                return value;
            }
        }

        public object FormatException( Exception exception ) => this.CreateSummary<Exception>( exception, exception.Message );

        private static Lazy<T> CreateLazy<T>( Func<object> func ) => new( () => (T) func() );

        protected virtual object CreateSummary<T>( object o, string summary )
            where T : notnull
        {
            return new Summary<T>( (T) o, summary );
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

        private class Summary<T>
            where T : notnull
        {
            private readonly string _summary;

            public override string ToString() => this._summary;

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public Lazy<T> Details { get; }

            public Summary( T detail, string summary )
            {
                this._summary = summary;
                this.Details = new Lazy<T>( () => detail );
            }
        }
    }
}