// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using LINQPad;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.LinqPad
{
    internal class ObjectFacade : ICustomMemberProvider
    {
        public ObjectFacadeType Type { get; }

        private readonly object _instance;

        internal ObjectFacade( ObjectFacadeType facadeType, object instance )
        {
            this.Type = facadeType;
            this._instance = instance;
        }

        public IEnumerable<string> GetNames() => this.Type.PropertyNames;

        public IEnumerable<Type> GetTypes() => this.Type.PropertyTypes;

        public IEnumerable<object?> GetValues()
        {
            foreach ( var property in this.Type.Properties )
            {
                object? value;

                try
                {
                    if ( property.IsLazy )
                    {
                        value = FacadePropertyFormatter.FormatLazyPropertyValue( this._instance, property.Type, property.GetFunc );
                    }
                    else
                    {
                        var rawValue = property.GetFunc( this._instance );
                        value = FacadePropertyFormatter.FormatPropertyValue( rawValue );
                    }
                }
                catch ( Exception e )
                {
                    value = FacadePropertyFormatter.FormatException( e );
                }

                yield return value;
            }
        }
    }
}