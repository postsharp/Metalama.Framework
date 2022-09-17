using System;

class Target : ITest
{
    int ITest.Foo
    {
        get
        {
            return this.Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Foo_Source;


        }
        set
        {
            this.Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Foo_Source = value;

        }
    }

    private int Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Foo_Source
    { get; set; }
    int ITest.Bar()
    {
        return this.Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Bar_Source();
    }

    private int Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Bar_Source()
    {
        return 42;
    }


    event EventHandler ITest.Quz
    {
        add
        {
            this.Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Quz_Source += value;


        }
        remove
        {
            this.Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Quz_Source -= value;

        }
    }

    private event EventHandler Metalama_Framework_Tests_Integration_Tests_Linker_Naming_InterfaceMembers_ITest_Quz_Source
    {
        add { }
        remove { }
    }

}