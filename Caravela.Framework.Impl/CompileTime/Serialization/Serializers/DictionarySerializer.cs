// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic; 

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    /// <exclude/>
    // This needs to be public because the type is instantiated from an activator in client assemblies.
    public sealed class DictionarySerializer<TKey, TValue> : ReferenceTypeMetaSerializer
    {
        private const string comparerCodeName = "c";

        private const string comparerName = "d";

        private const string keysName = "k";

        private const string valuesName = "v";

        /// <exclude/>
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            byte comparerCode = constructorArguments.GetValue<byte>( comparerCodeName );

            IEqualityComparer<TKey> comparer = constructorArguments.GetValue<IEqualityComparer<TKey>>( comparerName ) ??
                                            (IEqualityComparer<TKey>)ComparerExtensions.GetComparerFromCode( comparerCode );

            Dictionary<TKey, TValue> dictionary;

            if ( comparerCode == 0 && comparer == null )
            {
                dictionary = new Dictionary<TKey, TValue>();
            }
            else
            {
                dictionary = new Dictionary<TKey, TValue>( comparer );
            }

            TKey[] keys = constructorArguments.GetValue<TKey[]>( keysName );
            TValue[] values = constructorArguments.GetValue<TValue[]>( valuesName );

            for ( int idx = 0; idx < keys.Length; idx++ )
            {
                dictionary[keys[idx]] = values[idx];
            }

            return dictionary;
        }

        /// <exclude/>
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            Dictionary<TKey, TValue> dictionary = (Dictionary<TKey, TValue>)obj;
            TKey[] keys = new TKey[dictionary.Count];
            TValue[] values = new TValue[dictionary.Count];
            dictionary.Keys.CopyTo( keys, 0 );
            dictionary.Values.CopyTo( values, 0 );

            byte comparerCode = ComparerExtensions.GetComparerCode( dictionary.Comparer );

            // byte.MaxValue is a flag for custom comparer
            if ( comparerCode != byte.MaxValue )
            {
                constructorArguments.SetValue( comparerCodeName, comparerCode );
            }
            else
            {
                constructorArguments.SetValue( comparerName, dictionary.Comparer );
            }

            // we need to save arrays in constructorArguments because objects from initializationArguments can be not fully deserialized when DeserializeFields is called
            constructorArguments.SetValue( keysName, keys );
            constructorArguments.SetValue( valuesName, values );
        }

        /// <exclude/>
        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
        }
    }
}