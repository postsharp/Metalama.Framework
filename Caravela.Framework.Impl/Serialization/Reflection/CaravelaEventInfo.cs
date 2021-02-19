using System;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaEventInfo : EventInfo
    {
        public CaravelaEventInfo( ISymbol symbol, IType containingType )
        {
            this.Symbol = symbol;
            this.ContainingType = containingType;
        }

        public static CaravelaEventInfo Create( IEvent @event )
        {
            var fullEvent = (Event) @event;
            return new CaravelaEventInfo( fullEvent.Symbol, fullEvent.DeclaringType );
        }

        public ISymbol Symbol { get; }

        public IType ContainingType { get; }

        public override object[] GetCustomAttributes( bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string Name => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodInfo GetAddMethod( bool nonPublic ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodInfo GetRaiseMethod( bool nonPublic ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodInfo GetRemoveMethod( bool nonPublic ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override EventAttributes Attributes => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();
    }
}