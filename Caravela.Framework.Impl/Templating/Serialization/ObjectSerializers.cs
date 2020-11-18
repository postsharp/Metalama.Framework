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
    public class ObjectSerializers
    {
        private ConcurrentDictionary<Type, ObjectSerializer> serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
        private EnumSerializer _enumSerializer = new EnumSerializer();

        public ObjectSerializers()
        {
            // Primitive types
            this.serializers.TryAdd( typeof(char),   new CharSerializer() );
            this.serializers.TryAdd( typeof(bool),   new BoolSerializer() );
            this.serializers.TryAdd( typeof(byte),   new ByteSerializer() );
            this.serializers.TryAdd( typeof(sbyte),  new SByteSerializer() );
            this.serializers.TryAdd( typeof(ushort), new UShortSerializer() );
            this.serializers.TryAdd( typeof(short),  new ShortSerializer() );
            this.serializers.TryAdd( typeof(uint),   new UIntSerializer() );
            this.serializers.TryAdd( typeof(int),    new IntSerializer() );
            this.serializers.TryAdd( typeof(ulong),  new ULongSerializer() );
            this.serializers.TryAdd( typeof(long),   new LongSerializer() );
            this.serializers.TryAdd( typeof(float),  new FloatSerializer() );
            this.serializers.TryAdd( typeof(double), new DoubleSerializer() );
            this.serializers.TryAdd( typeof(decimal),new DecimalSerializer() );
            this.serializers.TryAdd( typeof(UIntPtr),new UIntPtrSerializer() );
            this.serializers.TryAdd( typeof(IntPtr), new IntPtrSerializer() );
            // String
            this.serializers.TryAdd( typeof(string), new StringSerializer() );
            // Known simple system types
            this.serializers.TryAdd( typeof(DateTime), new DateTimeSerializer() );// TODO Implement
            this.serializers.TryAdd( typeof(Guid), new GuidSerializer() );// TODO Implement
            this.serializers.TryAdd( typeof(TimeSpan), new TimeSpanSerializer() );// TODO Implement
            this.serializers.TryAdd( typeof(CultureInfo), new CultureInfoSerializer() ); // TODO Implement
            // Nullable types
            this.serializers.TryAdd( typeof(Nullable<>), new NullableSerializer(this) ); // TODO Implement nullable
            // Collections
            this.serializers.TryAdd( typeof(List<>), new ListSerializer(this) ); // TODO Implement
            this.serializers.TryAdd( typeof(Dictionary<,>), new DictionarySerializer(this) ); // TODO Implement
            // TODO dictionary's string comparison
            // Reflection types
            // TODO reflection types
        }

        public ExpressionSyntax SerializeToRoslynCreationExpression( object? o )
        {
            if ( o == null )
            {
                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }
            // TODO enums
            if ( o is Enum e )
            {
                return this._enumSerializer.Serialize( e );
            }
            // TODO array
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
            if (!this.serializers.TryGetValue( mainType, out ObjectSerializer serializer))
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedSerialization, mainType );
            }
            return serializer.Serialize( o );
        }
    }
}