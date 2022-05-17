// Error LAMA0037 on `VirtualizeAttribute`: `The aspect 'Virtualize' cannot be applied to 'S' because 'S' must be class or a record class.`
namespace Metalama.Open.Virtuosity.Tests.Struct
{
    [VirtualizeAttribute]
    internal struct S
    {
        public void M() { }
    }
}
