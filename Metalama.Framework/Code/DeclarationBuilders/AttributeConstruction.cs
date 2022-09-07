// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Encapsulates the information necessary to create a custom attribute. 
    /// </summary>
    public sealed class AttributeConstruction : IAttributeData
    {
        /// <summary>
        /// Gets the attribute constructor.
        /// </summary>
        public IConstructor Constructor { get; }

        /// <summary>
        /// Gets the attribute type.
        /// </summary>
        public INamedType Type => this.Constructor.DeclaringType;

        IType IHasType.Type => this.Type;

        /// <summary>
        /// Gets the constructor arguments.
        /// </summary>
        public ImmutableArray<TypedConstant> ConstructorArguments { get; }

        /// <summary>
        /// Gets the named arguments, i.e. the assigned fields and properties.
        /// Note that the order may be important in case of non-trivial property setters.
        /// </summary>
        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments { get; }

        private AttributeConstruction(
            IConstructor constructor,
            ImmutableArray<TypedConstant> constructorArguments,
            ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments )
        {
            this.Constructor = constructor;
            this.ConstructorArguments = constructorArguments;
            this.NamedArguments = namedArguments;
        }

        /// <summary>
        /// Creates a new <see cref="AttributeConstruction"/> by explicitly specifying the constructor and strongly-typed arguments.
        /// </summary>
        public static AttributeConstruction Create(
            IConstructor constructor,
            ImmutableArray<TypedConstant> constructorArguments = default,
            ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = default )
            => new(
                constructor,
                constructorArguments.IsDefault ? ImmutableArray<TypedConstant>.Empty : constructorArguments,
                namedArguments.IsDefault ? ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty : namedArguments );

        /// <summary>
        /// Creates a new <see cref="AttributeConstruction"/> by specifying the reflection <see cref="System.Type"/> of the attribute.
        /// The method will attempt to find a suitable constructor.
        /// </summary>
        public static AttributeConstruction Create(
            Type attributeType,
            IReadOnlyList<object?>? constructorArguments = null,
            IReadOnlyList<KeyValuePair<string, object?>>? namedArguments = null )
            => Create(
                (INamedType) TypeFactory.GetType( attributeType ),
                constructorArguments,
                namedArguments );

        /// <summary>
        /// Creates a new <see cref="AttributeConstruction"/> by specifying the <see cref="INamedType"/> of the attribute.
        /// The method will attempt to find a suitable constructor.
        /// </summary>
        public static AttributeConstruction Create(
            INamedType attributeType,
            IReadOnlyList<object?>? constructorArguments = null,
            IReadOnlyList<KeyValuePair<string, object?>>? namedArguments = null )
        {
            constructorArguments ??= ImmutableArray<object?>.Empty;
            namedArguments ??= ImmutableArray<KeyValuePair<string, object?>>.Empty;

            // Translate provided IType - typed parameters to System.Reflection.Type to get the correct constructor.
            var constructorArgumentTypes =
                constructorArguments
                    .Select( x => x?.GetType() )
                    .Select( x => x == null ? null : typeof(IType).IsAssignableFrom( x ) ? typeof(Type) : x )
                    .ToArray();

            var constructors = attributeType.Constructors.OfCompatibleSignature( constructorArgumentTypes ).ToList();

            switch ( constructors.Count )
            {
                case 0:
                    throw new ArgumentOutOfRangeException( nameof(constructorArguments), "Cannot find a constructor that is compatible with these arguments." );

                case > 1:
                    throw new ArgumentOutOfRangeException( nameof(constructorArguments), "Found more than one constructor compatible with these arguments." );
            }

            var constructor = constructors[0];

            // Map constructor arguments.
            var typedConstructorArguments = ImmutableArray.CreateBuilder<TypedConstant>( constructor.Parameters.Count );

            for ( var i = 0; i < constructor.Parameters.Count; i++ )
            {
                typedConstructorArguments.Add( new TypedConstant( constructor.Parameters[i].Type, constructorArguments[i] ) );
            }

            // Map named arguments.
            var typedNamedArguments = ImmutableArray.CreateBuilder<KeyValuePair<string, TypedConstant>>( namedArguments.Count );

            foreach ( var argument in namedArguments )
            {
                // TODO: inherited members
                var name = argument.Key;
                var fieldOrProperty = constructor.DeclaringType.FieldsAndProperties.OfName( name ).SingleOrDefault();

                if ( fieldOrProperty == null )
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(namedArguments),
                        $"The type '{constructor.DeclaringType.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage )}' does not contain a field or property named '{name}'." );
                }

                typedNamedArguments.Add( new KeyValuePair<string, TypedConstant>( argument.Key, new TypedConstant( fieldOrProperty.Type, argument.Value ) ) );
            }

            return new AttributeConstruction( constructor, typedConstructorArguments.MoveToImmutable(), typedNamedArguments.MoveToImmutable() );
        }
    }
}