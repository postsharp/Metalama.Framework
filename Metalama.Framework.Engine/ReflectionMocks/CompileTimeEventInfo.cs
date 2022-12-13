// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeEventInfo : EventInfo, ICompileTimeReflectionObject<IEvent>
    {
        public ISdkRef<IEvent> Target { get; }

        public CompileTimeEventInfo( IEvent @event )
        {
            this.Target = @event.ToTypedRef();
        }

        public static EventInfo Create( IEvent @event )
        {
            return new CompileTimeEventInfo( @event );
        }

        private static Exception CreateNotSupportedException() => CompileTimeMocksHelper.CreateNotSupportedException( "EventInfo" );

        public override object[] GetCustomAttributes( bool inherit ) => throw CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override Type DeclaringType => throw CreateNotSupportedException();

        public override string Name => throw CreateNotSupportedException();

        public override Type ReflectedType => throw CreateNotSupportedException();

        public override MethodInfo GetAddMethod( bool nonPublic ) => throw CreateNotSupportedException();

        public override MethodInfo GetRaiseMethod( bool nonPublic ) => throw CreateNotSupportedException();

        public override MethodInfo GetRemoveMethod( bool nonPublic ) => throw CreateNotSupportedException();

        public override EventAttributes Attributes => throw CreateNotSupportedException();
    }
}