﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.LamaSerialization.Serializers
{
    // This needs to be public because the type is instantiated from an activator in client assemblies.

    /// <exclude/>
    internal sealed class ImmutableArraySerializer<T> : ValueTypeSerializer<ImmutableArray<T>>
    {
        private const string _keyName = "_";

        public override void SerializeObject( ImmutableArray<T> obj, IArgumentsWriter constructorArguments )
        {
            // we need to save arrays in constructorArguments because objects from initializationArguments can be not fully deserialized when DeserializeFields is called
            constructorArguments.SetValue( _keyName, obj.ToArray() );
        }

        public override ImmutableArray<T> DeserializeObject( IArgumentsReader constructorArguments )
        {
            var values = constructorArguments.GetValue<T[]>( _keyName ).AssertNotNull();
            return ImmutableArray.Create<T>( values );
        }

    }
}