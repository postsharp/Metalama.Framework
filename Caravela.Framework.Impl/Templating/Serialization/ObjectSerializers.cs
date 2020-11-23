using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// Serializes objects into Roslyn creation expressions that would create those objects. You can register additional serializers with an instance of this class
    /// to support additional types.
    /// </summary>
    public class ObjectSerializers
    {
        private readonly ConcurrentDictionary<Type, ObjectSerializer> _serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
        private readonly EnumSerializer _enumSerializer;
        private readonly ArraySerializer _arraySerializer;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectSerializers"/>.
        /// </summary>
        public ObjectSerializers()
        {
            // Arrays, enums
            this._enumSerializer = new EnumSerializer();
            this._arraySerializer = new ArraySerializer(this);
            
            // Primitive types
            this._serializers.TryAdd( typeof(char),   new CharSerializer() );
            this._serializers.TryAdd( typeof(bool),   new BoolSerializer() );
            this._serializers.TryAdd( typeof(byte),   new ByteSerializer() );
            this._serializers.TryAdd( typeof(sbyte),  new SByteSerializer() );
            this._serializers.TryAdd( typeof(ushort), new UShortSerializer() );
            this._serializers.TryAdd( typeof(short),  new ShortSerializer() );
            this._serializers.TryAdd( typeof(uint),   new UIntSerializer() );
            this._serializers.TryAdd( typeof(int),    new IntSerializer() );
            this._serializers.TryAdd( typeof(ulong),  new ULongSerializer() );
            this._serializers.TryAdd( typeof(long),   new LongSerializer() );
            this._serializers.TryAdd( typeof(float),  new FloatSerializer() );
            this._serializers.TryAdd( typeof(double), new DoubleSerializer() );
            this._serializers.TryAdd( typeof(decimal),new DecimalSerializer() );
            this._serializers.TryAdd( typeof(UIntPtr),new UIntPtrSerializer() );
            this._serializers.TryAdd( typeof(IntPtr), new IntPtrSerializer() );
            
            // String
            this._serializers.TryAdd( typeof(string), new StringSerializer() );
            
            // Known simple system types
            this.RegisterSerializer( typeof(DateTime), new DateTimeSerializer() );
            this.RegisterSerializer( typeof(Guid), new GuidSerializer() );
            this.RegisterSerializer( typeof(TimeSpan), new TimeSpanSerializer() );
            this.RegisterSerializer( typeof(CultureInfo), new CultureInfoSerializer() ); 
            
            // Collections
            this._serializers.TryAdd( typeof(List<>), new ListSerializer(this) );
            this._serializers.TryAdd( typeof(Dictionary<,>), new DictionarySerializer(this) ); 
            
            // Reflection types
            this.RegisterSerializer( typeof(CaravelaType), new CaravelaTypeSerializer() );      
            this.RegisterSerializer( typeof(CaravelaMethodInfo), new CaravelaMethodInfoSerializer() );
            // TODO reflection types
        }
        
        /// <summary>
        /// Registers an additional serializer. See Remarks for generics.
        /// </summary>
        /// <remarks>
        /// For generic types, register the type without generic arguments, for example "List&lt;&gt;" rather than "List&lt;int&gt;". The serializer will handle
        /// lists of any element.
        /// </remarks>
        /// <param name="type">The specific type that this serializer supports.</param>
        /// <param name="serializer">A new serializer that supports that type.</param>
        public void RegisterSerializer( Type type, ObjectSerializer serializer )
        {
            this._serializers.TryAdd( type, serializer );
        }
        
        /// <summary>
        /// Serializes an object into a Roslyn expression that would create it. For example, serializes a list containing "4" and "8" into <c>new System.Collections.Generic.List&lt;System.Int32&gt;{4, 8}</c>.
        /// </summary>
        /// <param name="o">An object to serialize.</param>
        /// <returns>An expression that would create the object.</returns>
        /// <exception cref="CaravelaException">When the object can't be serialized, for example if it's of an unsupported type.</exception>
        public ExpressionSyntax SerializeToRoslynCreationExpression( object? o )
        {
            if ( o == null )
            {
                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }
            if ( o is Enum e )
            {
                return this._enumSerializer.Serialize( e );
            }

            if ( o is Array a )
            {
                return this._arraySerializer.Serialize( a );
            }
            Type t = o.GetType();
            Type mainType;
            if ( t.IsGenericType )
            {
                mainType = t.GetGenericTypeDefinition();
            }
            else
            {
                mainType = t;
            }
            if (!this._serializers.TryGetValue( mainType, out ObjectSerializer serializer))
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedSerialization, mainType );
            }
            return serializer.SerializeObject( o );
        }
    }

    internal class CaravelaMethodInfoSerializer : TypedObjectSerializer<CaravelaMethodInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaMethodInfo o )
        {
            string documentationId = DocumentationCommentId.CreateDeclarationId( o.Symbol );
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "System" ),
                                IdentifierName( "Reflection" ) ),
                            IdentifierName( "MethodBase" ) ),
                        IdentifierName( "GetMethodFromHandle" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName( "Caravela" ),
                                                    IdentifierName( "Compiler" ) ),
                                                IdentifierName( "Intrinsics" ) ),
                                            IdentifierName( "GetRuntimeMethodHandle" ) ) )
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(documentationId) ) ) ) ) ) ) ) ) )
                .NormalizeWhitespace();
        }
    }

    internal class CaravelaTypeSerializer : TypedObjectSerializer<CaravelaType>
    {
        public override ExpressionSyntax Serialize( CaravelaType o )
        {
            return default;
        }
    }
}