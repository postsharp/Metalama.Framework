// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeEventInfo : EventInfo, ICompileTimeReflectionObject<IEvent>
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

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetAddMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetRaiseMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetRemoveMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}