The `*Builder` classes have references to the CompilationModel that was current when the builders were created, not when
they are consumed. Therefore, they must be mapped for consumption, just like Roslyn symbols must be mapped.

The `Built*` classes are the facades that reference the consuming CompilationModel.