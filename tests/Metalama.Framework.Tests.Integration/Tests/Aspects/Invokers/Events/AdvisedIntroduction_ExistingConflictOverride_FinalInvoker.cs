﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictOverride_FinalInvoker;

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroductionAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictOverride_FinalInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler BaseClassVirtualEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Add(meta.This, value);
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Remove(meta.This, value);
            }
        }


        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Add(meta.This, value);
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Remove(meta.This, value);
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Add(meta.This, value);
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Target.Event.Invokers.Final.Remove(meta.This, value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OverrideAttribute : TypeAspect
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
            meta.Target.Event.Invokers.Final.Add(meta.This, value);
        }

        [Template]
        public void RemoveTemplate(dynamic value)
        {
            meta.Target.Event.Invokers.Final.Remove(meta.This, value);
        }
    }

    internal abstract class BaseClass
    {

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

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
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