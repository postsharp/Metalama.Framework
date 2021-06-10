namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTime
{
    partial class TargetClass
    {
        public global::System.Int32 InterfaceMethod()
        {
            return default(global::System.Int32);
        }

        public event global::System.EventHandler Event;
        public abstract global::System.Int32 __InterfaceImpl__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Introductions_Interfaces_DesignTime_IInterface__InterfaceMethod__By__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Introductions_Interfaces_DesignTime_IntroductionAttribute()
        {
            return this.InterfaceMethod();
        }

        public abstract event global::System.EventHandler __InterfaceImpl__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Introductions_Interfaces_DesignTime_IInterface__Event__By__Caravela_Framework_Tests_Integration_TestInputs_Aspects_Introductions_Interfaces_DesignTime_IntroductionAttribute
        {
            add
            {
                this.Event += value;
            }

            remove
            {
                this.Event -= value;
            }
        }
    }
}
