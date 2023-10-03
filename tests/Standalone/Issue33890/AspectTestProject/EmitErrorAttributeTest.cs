using AspectLibraryProject;

namespace AspectTestProject.EmitErrorAttributeTests
{
    internal class EmitErrorAttributeTest
    {
        // The error should be emitted when running the test, but not when building the test suite.
        [EmitError]
        private void MyMethod()
        {
            Console.WriteLine("Hello, world");
        }
    }
}