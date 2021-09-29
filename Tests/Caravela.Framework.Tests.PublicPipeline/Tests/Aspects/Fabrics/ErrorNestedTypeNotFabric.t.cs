// Error CR0231 on `E`: `The compile-time type 'TargetCode.E' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
// Error CR0231 on `I`: `The compile-time type 'TargetCode.I' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
// Error CR0231 on `S`: `The compile-time type 'TargetCode.S' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
// Error CR0231 on `D`: `The compile-time type 'TargetCode.D' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
// Error CR0231 on `R`: `The compile-time type 'TargetCode.R' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
// Error CR0231 on `C`: `The compile-time type 'TargetCode.C' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing 'Caravela.Framework.Fabrics.ITypeFabric'.`
class TargetCode
    {
        
        [CompileTimeOnly]
        enum E {}
        
        [CompileTimeOnly]
        interface I {}
        
        [CompileTimeOnly]
        struct S {}
        
        [CompileTimeOnly]
        delegate void D();
        
        [CompileTimeOnly]
        record R ( int x );
        
    }
