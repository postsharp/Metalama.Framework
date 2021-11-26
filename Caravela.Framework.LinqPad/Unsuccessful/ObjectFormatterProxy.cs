using LINQPad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caravela.Framework.LinqPad
{
    internal class ObjectFormatterProxy : ICustomMemberProvider
    {
        private readonly ObjectFormatterType _type;
        private readonly object _instance;
        private static readonly LinqPadSpecializedFormatter _formatter = new();

        static ObjectFormatterProxy()
        {
            Debugger.Launch();
        }

        public static ObjectFormatterProxy? Get( object? instance )
        {
            var isInlineType = instance is IEnumerable || instance is string || instance == null || instance.GetType().IsPrimitive
                               || instance is DumpContainer || instance is Hyperlinq;

            return isInlineType
                ? null
                : new ObjectFormatterProxy( ObjectFormatterType.GetFormatterType( instance.GetType() ), instance );
        }

        public ObjectFormatterProxy( ObjectFormatterType type, object instance )
        {
            this._type = type;
            this._instance = instance;
        }

        public IEnumerable<string> GetNames() => this._type.PropertyNames;

        public IEnumerable<Type> GetTypes() => this._type.PropertyTypes;

        public IEnumerable<object?> GetValues()
        {
            foreach ( var property in this._type.Properties )
            {
                object? value;

                try
                {
                    var isLazy = false;

                    if ( property.IsLazy )
                    {
                        isLazy = true;
                        value = null;
                    }
                    else
                    {
                        value = property.Getter.Invoke( this._instance, null );

                        if ( value is ICollection { Count: > 100 } )
                        {
                            isLazy = true;
                        }
                    }

                    if ( isLazy )
                    {
                        value = _formatter.FormatLazyPropertyValue( this._instance, property.Getter );
                    }
                    else
                    {
                        value = _formatter.FormatPropertyValue( value!, property.Getter );
                    }
                }
                catch ( Exception e )
                {
                    value = _formatter.FormatException( e );
                }

                yield return value;
            }
        }
    }
}