// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization
{
    public abstract class ReferenceTypeSerializer<T> : ReferenceTypeSerializer
    {
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            this.SerializeObject( (T) obj, constructorArguments, initializationArguments );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            this.DeserializeFields( (T) obj, initializationArguments );
        }

        public abstract void SerializeObject( T obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments );

        public abstract void DeserializeFields( T obj, IArgumentsReader initializationArguments );
    }
}