// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Serialization;
using Metalama.Framework.Validation;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

public sealed class TransitiveValidatorInstance : ICompileTimeSerializable
{
    internal TransitiveValidatorInstance( ReferenceValidatorInstance instance )
    {
        this.ValidatedDeclaration = instance.ValidatedDeclaration.ToRef();
        this.ReferenceKinds = instance.ReferenceKinds;
        this.IncludeDerivedTypes = instance.IncludeDerivedTypes;
        this.MethodName = instance.Driver.MethodName;
        this.Object = instance.Implementation.Implementation;
        this.State = instance.Implementation.State;
        this.DiagnosticSourceDescription = instance.DiagnosticSourceDescription;
    }

    public TransitiveValidatorInstance(
        IRef<IDeclaration> validatedDeclaration,
        ReferenceKinds referenceKinds,
        bool includeDerivedTypes,
        object obj,
        IAspectState? state,
        string? methodName,
        string diagnosticSourceDescription )
    {
        this.ValidatedDeclaration = validatedDeclaration;
        this.IncludeDerivedTypes = includeDerivedTypes;
        this.ReferenceKinds = referenceKinds;
        this.Object = obj;
        this.State = state;
        this.MethodName = methodName;
        this.DiagnosticSourceDescription = diagnosticSourceDescription;
    }

    private TransitiveValidatorInstance()
    {
        // This to make code analysis happy. All properties are actually set by the deserializer.
        this.ValidatedDeclaration = null!;
        this.ReferenceKinds = default;
        this.MethodName = null!;
        this.Object = null!;
        this.DiagnosticSourceDescription = null!;
    }

    public IRef<IDeclaration> ValidatedDeclaration { get; private set; }

    public ReferenceKinds ReferenceKinds { get; private set; }

    public bool IncludeDerivedTypes { get; }

    public object Object { get; private set; }

    public IAspectState? State { get; private set; }

    public string? MethodName { get; private set; }

    internal string DiagnosticSourceDescription { get; }

    internal ValidatorDriver GetReferenceValidatorDriver()
    {
        var type = this.Object.GetType();

        if ( this.MethodName != null )
        {
            var method = type.GetMethod( this.MethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

            if ( method == null )
            {
                throw new InvalidOperationException( $"Cannot find a method named '{this.MethodName}' in '{type}'." );
            }

            return ValidatorDriverFactory.GetInstance( type )
                .GetReferenceValidatorDriver( method );
        }
        else
        {
            return ValidatorDriverFactory.GetInstance( type ).GetReferenceValidatorDriver( type );
        }
    }

    // ReSharper disable once UnusedType.Local
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