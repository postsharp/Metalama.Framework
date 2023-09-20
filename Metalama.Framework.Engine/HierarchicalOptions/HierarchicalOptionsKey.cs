// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.HierarchicalOptions;

public record struct HierarchicalOptionsKey( string OptionType, SerializableDeclarationId DeclarationId )
{
    private class Serializer : ValueTypeSerializer<HierarchicalOptionsKey>
    {
        public override void SerializeObject( HierarchicalOptionsKey obj, IArgumentsWriter constructorArguments )
        {
            constructorArguments.SetValue( nameof(obj.OptionType), obj.OptionType );
            constructorArguments.SetValue( nameof(obj.DeclarationId), obj.DeclarationId );
        }

        public override HierarchicalOptionsKey DeserializeObject( IArgumentsReader constructorArguments )
        {
            return new HierarchicalOptionsKey(
#pragma warning disable SA1101
                constructorArguments.GetValue<string>( nameof(OptionType) )!,
                constructorArguments.GetValue<SerializableDeclarationId>( nameof(DeclarationId) ) );
#pragma warning restore SA1101
        }
    }
}