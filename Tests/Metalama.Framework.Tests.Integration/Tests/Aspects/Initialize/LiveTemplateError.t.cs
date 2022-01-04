// Error CR0045 on `LiveTemplate`: `The class 'Aspect' must have a default constructor because of the [LiveTemplate] attribute.`
internal class Target
    {
        [Aspect( 0 )]
        private void M() { }
    }
