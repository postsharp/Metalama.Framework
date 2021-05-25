// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class EventBuilder : MemberBuilder, IEventBuilder
    {
        public EventBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool hasAdder,
            bool hasRemover,
            AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name ) { }

        public INamedType EventType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMethodBuilder Adder => throw new NotImplementedException();

        public IMethodBuilder Remover => throw new NotImplementedException();

        public IMethodBuilder? Raiser => throw new NotImplementedException();

        public override MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();

        public override DeclarationKind DeclarationKind => throw new NotImplementedException();

        IType IEvent.EventType => throw new NotImplementedException();

        IMethod IEvent.Adder => throw new NotImplementedException();

        IMethod IEvent.Remover => throw new NotImplementedException();

        IMethod? IEvent.Raiser => throw new NotImplementedException();

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => Array.Empty<IEvent>();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public EventInfo ToEventInfo()
        {
            throw new NotImplementedException();
        }
    }
}