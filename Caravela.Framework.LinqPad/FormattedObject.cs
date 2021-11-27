// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using LINQPad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.LinqPad
{
    internal class FormattedObject : ICustomMemberProvider
    {
        private static readonly ConditionalWeakTable<object, FormattedObject> _instances = new();
        private readonly FormattedObjectType _type;
        private readonly object _instance;

        public static FormattedObject? Get( object? instance )
        {
            var isInlineType = instance == null || instance is IEnumerable || instance is string || instance.GetType().IsPrimitive
                               || instance is DumpContainer || instance is Hyperlinq;

            if ( isInlineType )
            {
                return null;
            }
            else
            {
                if ( !_instances.TryGetValue( instance!, out var proxy ) )
                {
                    proxy = new FormattedObject( FormattedObjectType.GetFormatterType( instance!.GetType() ), instance );
                    _instances.AddOrUpdate( instance, proxy );
                }

                return proxy;
            }
        }

        public FormattedObject( FormattedObjectType type, object instance )
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
                    if ( property.IsLazy )
                    {
                        value = PropertyValueFormatter.FormatLazyPropertyValue( this._instance, property.Getter );
                    }
                    else
                    {
                        var rawValue = property.Getter.Invoke( this._instance, null );
                        value = PropertyValueFormatter.FormatPropertyValue( rawValue, property.Getter );
                    }
                }
                catch ( TargetInvocationException e )
                {
                    value = PropertyValueFormatter.FormatException( e.InnerException! );
                }

                yield return value;
            }
        }
    }
}