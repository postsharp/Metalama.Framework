// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects;

public partial class InheritableAspectInstance
{
    // ReSharper disable once UnusedType.Local
    private class Serializer : ReferenceTypeSerializer
    {
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments ) => new InheritableAspectInstance();

        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var instance = (InheritableAspectInstance) obj;
            initializationArguments.SetValue( nameof(instance.TargetDeclaration), instance.TargetDeclaration );
            initializationArguments.SetValue( nameof(instance.Aspect), instance.Aspect );
            initializationArguments.SetValue( nameof(instance.SecondaryInstances), instance.SecondaryInstances );
            initializationArguments.SetValue( nameof(instance.TargetTag), instance.TargetTag );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            var instance = (InheritableAspectInstance) obj;
            instance.TargetDeclaration = initializationArguments.GetValue<IRef<IDeclaration>>( nameof(instance.TargetDeclaration) )!;
            instance.Aspect = initializationArguments.GetValue<IAspect>( nameof(instance.Aspect) )!;
            instance.SecondaryInstances = initializationArguments.GetValue<ImmutableArray<IAspectInstance>>( nameof(instance.SecondaryInstances) );
            instance.TargetTag = initializationArguments.GetValue<object>( nameof(instance.TargetTag) );
        }
    }
}