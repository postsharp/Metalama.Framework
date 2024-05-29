using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces
{
    public interface IBaseInterface
    {
        void BaseMethod();

        int BaseProperty { get; set; }

        event EventHandler? BaseEvent;
    }

    public interface IInterface : IBaseInterface
    {
        void Method();

        int Property { get; set; }

        event EventHandler? Event;
    }

    [CompileTime]
    public class AdviceResultTemplates : ITemplateProvider
    {
        [Template]
        public void WitnessTemplate(
            [CompileTime] IReadOnlyCollection<IInterfaceImplementationResult> types,
            [CompileTime] IReadOnlyCollection<IInterfaceMemberImplementationResult>? members )
        {
            foreach (var type in types)
            {
                Console.WriteLine( $"InterfaceType: {type.InterfaceType}, Action: {type.Outcome}" );
            }

            foreach (var member in members ?? [])
            {
                Console.WriteLine( $"Member: {member.InterfaceMember}, Action: {member.Outcome}, Target: {member.TargetMember}" );
            }
        }
    }
}