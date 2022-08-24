using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictNew_FinalInvoker;

[assembly: AspectOrder(typeof(TestAttribute), typeof(IntroductionAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictNew_FinalInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler BaseClassVirtualEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }


        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler BaseClassAbstractEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler DerivedClassEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler DerivedClassVirtualEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var targetEvent in builder.Target.Events)
            {
                builder.Advice.OverrideAccessors(
                    targetEvent,
                    nameof(AddTemplate),
                    nameof(RemoveTemplate),
                    null);
            }
        }

        [Template]
        public void AddTemplate(dynamic value)
        {
            meta.Target.Event.Invokers.Base!.Add(meta.This, value);
        }

        [Template]
        public void RemoveTemplate(dynamic value)
        {
            meta.Target.Event.Invokers.Base!.Remove(meta.This, value);
        }
    }

    internal abstract class BaseClass
    {
        public event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public virtual event EventHandler BaseClassVirtualEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public abstract event EventHandler BaseClassAbstractEvent;
    }

    internal class DerivedClass : BaseClass
    {
        public event EventHandler DerivedClassEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public event EventHandler DerivedClassVirtualEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public new event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public override event EventHandler BaseClassAbstractEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }
    }

    // <target>
    [Introduction]
    [Test]
    internal class TargetClass : DerivedClass
    {
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }

        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                Console.WriteLine("This is the original add.");
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
            }
        }
    }
}