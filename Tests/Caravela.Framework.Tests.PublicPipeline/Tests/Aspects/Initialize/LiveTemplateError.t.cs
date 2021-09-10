// Error CR0026 on ``: `The aspect method 'Aspect.BuildAspectClass' has thrown an exception of type 'InvalidOperationException': System.InvalidOperationException: The aspect type must have a default constructor to be able to be a live template.`
class Target 
    {
    
        [Aspect(0)]
        void M() {}
    }
