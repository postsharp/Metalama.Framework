// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Serialization;
using Metalama.Framework.Validation;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

public class TransitiveValidatorInstance : ILamaSerializable
{
    internal TransitiveValidatorInstance( ReferenceValidatorInstance instance )
    {
        this.ValidatedDeclaration = instance.ValidatedDeclaration.ToRef();
        this.ReferenceKinds = instance.ReferenceKinds;
        this.MethodName = instance.Source.MethodName;
        this.Object = instance.Object;
        this.State = instance.State;
    }

    private TransitiveValidatorInstance()
    {
        // This to make code analysis happy. All properties are actually set by the deserializer.
        this.ValidatedDeclaration = null!;
        this.ReferenceKinds = default;
        this.MethodName = null!;
        this.Object = null!;
    }

    public IRef<IDeclaration> ValidatedDeclaration { get; private set; }

    public ReferenceKinds ReferenceKinds { get; private set; }

    public object Object { get; private set; }

    public IAspectState? State { get; private set; }

    public string MethodName { get; private set; }

    private sealed class Serializer : ReferenceTypeSerializer
    {
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments ) => new TransitiveValidatorInstance();

        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var instance = (TransitiveValidatorInstance) obj;
            initializationArguments.SetValue( nameof(instance.ValidatedDeclaration), instance.ValidatedDeclaration );
            initializationArguments.SetValue( nameof(instance.ReferenceKinds), instance.ReferenceKinds );
            initializationArguments.SetValue( nameof(instance.Object), instance.Object );
            initializationArguments.SetValue( nameof(instance.State), instance.State );
            initializationArguments.SetValue( nameof(instance.MethodName), instance.MethodName );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            var instance = (TransitiveValidatorInstance) obj;
            instance.ValidatedDeclaration = initializationArguments.GetValue<IRef<IDeclaration>>( nameof(instance.ValidatedDeclaration) )!;
            instance.ReferenceKinds = initializationArguments.GetValue<ReferenceKinds>( nameof(instance.ReferenceKinds) );
            instance.Object = initializationArguments.GetValue<object>( nameof(instance.Object) )!;
            instance.State = initializationArguments.GetValue<IAspectState>( nameof(instance.State) );
            instance.MethodName = initializationArguments.GetValue<string>( nameof(instance.MethodName) )!;
        }
    }
}