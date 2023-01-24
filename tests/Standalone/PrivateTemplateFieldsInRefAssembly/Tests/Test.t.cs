 using Dependency;
 namespace Tests;
 partial class Program
 {
   [Err]
   public void Foo()
   {
     global::System.Console.WriteLine(_field);
     return;
   }
   private readonly global::System.Int32 _field;
   [field: global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]
   private global::System.Int32 AutoProperty { get; set; }
   internal global::System.Int32 Property
   {
     get
     {
       return (global::System.Int32)42;
     }
     private set
     {
     }
   }
   private void Method(global::System.Int32 arg)
   {
   }
   private event global::System.EventHandler Event
   {
     add
     {
     }
     remove
     {
     }
   }
   private event global::System.EventHandler FieldLikeEvent;
 }