// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    // This needs to be public because the type is instantiated from an activator in client assemblies.
    /// <exclude/>
    public sealed class ListSerializer<T> : ReferenceTypeMetaSerializer
    {
        private const string keyName = "_";

        /// <exclude/>
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            var values = constructorArguments.GetValue<T[]>( keyName );
            var list = new List<T>( values.Length );
            list.AddRange( values );

            return list;
        }

        /// <exclude/>
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var list = (List<T>) obj;

            // we need to save arrays in constructorArguments because objects from initializationArguments can be not fully deserialized when DeserializeFields is called
            constructorArguments.SetValue( keyName, list.ToArray() );
        }

        /// <exclude/>
        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
        }
    }
}