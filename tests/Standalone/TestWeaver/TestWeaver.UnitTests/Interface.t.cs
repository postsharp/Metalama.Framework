// Error LAMA0037 on `VirtualizeAttribute`: `The aspect 'Virtualize' cannot be applied to 'I' because 'I' must be class or a record class.`
namespace Metalama.Open.Virtuosity.Tests.Interface
{
    [VirtualizeAttribute]
    internal interface I
    {
        public void M() { }
    }
}
