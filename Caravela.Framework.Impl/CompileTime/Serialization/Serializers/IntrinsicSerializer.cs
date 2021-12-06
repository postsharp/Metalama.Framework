// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal abstract class IntrinsicSerializer<T> : IMetaSerializer
    {
        public abstract object Convert( object value, Type targetType );

        object IMetaSerializer.CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            return constructorArguments.GetValue<T>( "_" )!;
        }

        void IMetaSerializer.DeserializeFields( ref object obj, IArgumentsReader initializationArguments )
        {
        }

        void IMetaSerializer.SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter? initializationArguments )
        {
            constructorArguments.SetValue( "_", (T) obj );
        }

        public bool IsTwoPhase => false;

        protected static void WriteType( SerializationBinaryWriter writer, SerializationIntrinsicType type )
        {
            writer.WriteByte( (byte) type );
        }
    }
}