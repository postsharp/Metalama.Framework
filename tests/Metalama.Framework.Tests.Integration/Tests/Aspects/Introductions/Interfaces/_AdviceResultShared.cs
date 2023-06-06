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
        public void WitnessTemplate([CompileTime] IReadOnlyCollection<ImplementedInterface> types, [CompileTime] IReadOnlyCollection<ImplementedInterfaceMember> members)
        {
            foreach (var type in types)
            {
                Console.WriteLine($"Interface: {type.Interface}, Action: {type.Action}");
            }

            foreach (var member in members)
            {
                Console.WriteLine($"Member: {member.InterfaceMember}, Action: {member.Action}, Target: {member.TargetMember}");
            }
        }
    }
}
