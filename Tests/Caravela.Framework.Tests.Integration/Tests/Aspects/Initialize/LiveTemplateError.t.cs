// Error CR0042 on `builder.IsLiveTemplate = true;`: `'Aspect.BuildAspectClass' threw 'InvalidOperationException': The aspect type must have a default constructor to be able to be a live template. Exception details are in '(none)'. To attach a debugger to the compiler, use the  '-p:DebugCaravela=True' command-line option.`
internal class Target
    {
        [Aspect( 0 )]
        private void M() { }
    }