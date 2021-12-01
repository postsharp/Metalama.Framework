// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

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
            T[] values = constructorArguments.GetValue<T[]>( keyName );
            List<T> list = new List<T>(values.Length);
            list.AddRange( values );

            return list;
        }

        /// <exclude/>
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            List<T> list = (List<T>)obj;

            // we need to save arrays in constructorArguments because objects from initializationArguments can be not fully deserialized when DeserializeFields is called
            constructorArguments.SetValue( keyName, list.ToArray() );
        }

        /// <exclude/>
        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
        }
    }
}