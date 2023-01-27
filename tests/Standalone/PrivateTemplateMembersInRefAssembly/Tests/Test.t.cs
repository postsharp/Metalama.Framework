 using Dependency;
 namespace Tests;
 partial class Program
 {
   [IntroducePrivateMembers]
   public void Foo()
   {
     Method(_field);
     global::System.Console.WriteLine(AutoProperty);
     global::System.Console.WriteLine(Property);
     FieldLikeEvent += (s, e) => global::System.Console.WriteLine(e);
     Event += (s_1, e_1) => global::System.Console.WriteLine(e_1);
     return;
   }
   private readonly global::Dependency.RunTimeOnlyClass _field;
   [field: global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]
   private global::Dependency.RunTimeOnlyClass AutoProperty { get; set; }
   internal global::Dependency.RunTimeOnlyClass Property
   {
     get
     {
       return (global::Dependency.RunTimeOnlyClass)(new());
     }
     private set
     {
     }
   }
   private void Method(global::Dependency.RunTimeOnlyClass arg)
   {
   }
   private event global::System.EventHandler<global::Dependency.RunTimeOnlyClass> Event
   {
     add
     {
     }
     remove
     {
     }
   }
   private event global::System.EventHandler<global::Dependency.RunTimeOnlyClass> FieldLikeEvent;
 }