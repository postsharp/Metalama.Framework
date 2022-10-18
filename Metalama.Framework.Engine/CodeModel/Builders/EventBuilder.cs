// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class EventBuilder : MemberBuilder, IEventBuilder, IEventImpl
    {
        public IObjectReader InitializerTags { get; }

        public bool IsEventField { get; }

        public EventBuilder(
            INamedType targetType,
            string name,
            bool isEventField,
            IObjectReader initializerTags,
            Advice advice )
            : base( targetType, name, advice )
        {
            this.InitializerTags = initializerTags;
            this.IsEventField = isEventField;
            this.Type = (INamedType) targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(EventHandler) );
        }

        public INamedType Type { get; set; }

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethodBuilder AddMethod => new AccessorBuilder( this, MethodKind.EventAdd, this.IsEventField );

        [Memo]
        public IMethodBuilder RemoveMethod => new AccessorBuilder( this, MethodKind.EventRemove, this.IsEventField );

        public IMethodBuilder? RaiseMethod => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>(
                ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ),
                this.OverriddenEvent != null );

        public IEvent? OverriddenEvent { get; set; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        INamedType IEvent.Type => this.Type;

        IMethod IEvent.AddMethod => this.AddMethod;

        IMethod IEvent.RemoveMethod => this.RemoveMethod;

        IMethod? IEvent.RaiseMethod => this.RaiseMethod;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IEvent>();

        public IExpression? InitializerExpression { get; set; }

        public TemplateMember<IEvent>? InitializerTemplate { get; set; }

        public EventInfo ToEventInfo() => CompileTimeEventInfo.Create( this );

        public void SetExplicitInterfaceImplementation( IEvent interfaceEvent ) => this.ExplicitInterfaceImplementations = new[] { interfaceEvent };

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenEvent;

        public override IIntroduceMemberTransformation ToTransformation( Advice advice ) => new IntroduceEventTransformation( advice, this );

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.EventAdd => this.AddMethod,
                MethodKind.EventRaise => this.RaiseMethod,
                MethodKind.EventRemove => this.RemoveMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                yield return this.AddMethod;
                yield return this.RemoveMethod;

                if ( this.RaiseMethod != null )
                {
                    yield return this.RaiseMethod;
                }
            }
        }

        IType IHasType.Type => this.Type;

        public override void Freeze()
        {
            base.Freeze();

            ((DeclarationBuilder?) this.AddMethod)?.Freeze();
            ((DeclarationBuilder?) this.RemoveMethod)?.Freeze();
        }
    }
}