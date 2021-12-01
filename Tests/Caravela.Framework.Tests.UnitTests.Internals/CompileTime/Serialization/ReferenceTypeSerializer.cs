// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public abstract class ReferenceTypeSerializer<T> : ReferenceTypeMetaSerializer
    {
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            this.SerializeObject( (T)obj, constructorArguments, initializationArguments );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            this.DeserializeFields( (T)obj, initializationArguments );
        }

        public abstract void SerializeObject( T obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments );

        public abstract void DeserializeFields( T obj, IArgumentsReader initializationArguments );
    }
}