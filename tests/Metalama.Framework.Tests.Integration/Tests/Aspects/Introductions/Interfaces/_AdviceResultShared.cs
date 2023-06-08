using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void WitnessTemplate([CompileTime] IReadOnlyCollection<InterfaceImplementationResult> types, [CompileTime] IReadOnlyCollection<InterfaceMemberImplementationResult> members)
        {
            foreach (var type in types)
            {
                Console.WriteLine($"Interface: {type.Interface}, Action: {type.Outcome}");
            }

            foreach (var member in members)
            {
                Console.WriteLine($"Member: {member.InterfaceMember}, Action: {member.Outcome}, Target: {member.TargetMember}");
            }
        }
    }
}
