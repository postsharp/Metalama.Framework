using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictNew_FinalInvoker;
using Newtonsoft.Json.Linq;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_ExistingConflictNew_FinalInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler BaseClassVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler BaseClassAbstractEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler DerivedClassEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler DerivedClassVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
            }

            remove
            {
                Console.WriteLine( "This is introduced event." );
                meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
            }
        }
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var e in builder.Target.Events)
            {
                builder.Advice.OverrideAccessors( e, nameof(OverrideAdd), nameof(OverrideRemove) );
            }
        }

        [Template]
        public void OverrideAdd()
        {
            Console.WriteLine( "Override." );
            meta.Target.Event.Add( meta.This, meta.Target.Parameters[0].Value );
        }

        [Template]
        public void OverrideRemove()
        {
            Console.WriteLine( "Override." );
            meta.Target.Event.Remove( meta.This, meta.Target.Parameters[0].Value );
        }
    }

    internal abstract class BaseClass
    {
        public event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }

        public virtual event EventHandler BaseClassVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
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
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }

        public event EventHandler DerivedClassVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }

        public new event EventHandler BaseClassEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }

        public override event EventHandler BaseClassAbstractEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass : DerivedClass
    {
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }

        public event EventHandler ExistingVirtualEvent
        {
            add
            {
                Console.WriteLine( "This is the original add." );
            }

            remove
            {
                Console.WriteLine( "This is the original remove." );
            }
        }
    }
}