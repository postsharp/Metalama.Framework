using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

// TODO (Daniel): this test is broken and I don't understand.


namespace Metalama.Framework.Tests.Integration.Test s.Linker.Fiel ds.Linking.IntroducedFiel d
{
    [Pseu doLayerOrder("TestAspect0 ")]
    [Pseu doLayerOrder("TestAspect1 ")]
    [Pseu doLayerOrder("TestAspect2 ")]
    [Pseu doLayerOrder("TestAspect3 ")]
    [Pseu doLayerOrder("TestAspect4 ")]
    [Pseu doLayerOrder("TestAspect5 ")]
    [Pseu doLayerOrder("TestAspect6 ")]
    [Pseu doLayerOrder("TestAspect7 ")]
    [Pseu doL
ayerOrder("TestAspect8")]
    [PseudoLayerOrder("TestAspect9")]
    // <target>
    class Target
    {
        public int Foo
        {
             get
            
 {
                Console.WriteLine("This is original code.");
                return 0;
            }

             set
             {
                Console.WriteLine("This is ori ginal code.");
             }
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override0
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should
 invoke empty code.
                _ = link[_this.Bar, @base];
                // Shoul
d invoke empty code.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should inv
oke empty code.
                link[_this.Bar, @base] = value;
                // Should in
voke empty code.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override2
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should 
invoke source code.
                _ = link[_this.Bar, @base];
                // Should
 invoke source code.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should invo
ke source code.
                link[_this.Bar, @base] = value;
                // Should inv
oke source code.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Foo), "TestAspect5")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override5
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should
 invoke override 4.
                _ = link[_this.Bar, @base];
                // Shoul
d invoke override 4.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should inv
oke override 4.
                link[_this.Bar, @base] = value;
                // Should in
voke override 4.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Foo), "TestAspect7")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override7
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should
 invoke override 6.
                _ = link[_this.Bar, @base];
                // Shoul
d invoke override 6.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should inv
oke override 6.
                link[_this.Bar, @base] = value;
                // Should in
voke override 6.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Foo), "TestAspect9")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override9
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should invoke the
 final declaration.
                _ = link[_this.Bar, @base];
                // Should invoke th
e final declaration.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should invoke the fin
al declaration.
                link[_this.Bar, @base] = value;
                // Should invoke the fi
nal declaration.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

         [PseudoReplaced]
        [PseudoIntroduction( "Tes tAspect1")] 
        public int Bar;

         [PseudoRep lacement(nameof(Bar))]
        [PseudoIntroduction("TestAspect3")]
        pu blic int Bar_Replacement {  get; set; }

        [PseudoOverride(nameof(Bar), "TestAspect4")]
        [PseudoNotInlineable]
        public int Bar_Override4
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should 
invoke source code.
                _ = link[_this.Bar, @base];
                // Shoul
d invoke override 4.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should invo
ke source code.
                link[_this.Bar, @base] = value;
                // Should in
voke override 4.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Bar), "TestAspect6")]
        [PseudoNotInlineable]
        public int Bar_Override6
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should
 invoke override 4.
                _ = link[_this.Bar, @base];
                // Shoul
d invoke override 6.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should inv
oke override 4.
                link[_this.Bar, @base] = value;
                // Should in
voke override 6.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, f inal] = value;
             }
        }

        [PseudoOverride(nameof(Bar), "TestAspect8")]
        [PseudoNotInlineable]
        public int Bar_Override8
        {
            get
            {
                // Should in
voke empty code.
                _ = link[_this.Bar, original];
                // Should
 invoke override 6.
                _ = link[_this.Bar, @base];
                // Should invoke th
e final declaration.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke
 empty code.
                link[_this.Bar, original] = value;
                // Should inv
oke override 6.
                link[_this.Bar, @base] = value;
                // Should invoke the fi
nal declaration.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
               link[_this.Bar, final] = value;
            }
        }
    }
}
