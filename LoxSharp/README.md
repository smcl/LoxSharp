# LoxSharp

My version of [Crafting Interpreters](https://craftinginterpreters.com) but written in C# instead of Java. Still in-progress.

TODO:
- implement a few more builtins/libs
- write a C# FFI
- implement arrow functions!
- pairs!
- list comprehensions?
- there's a couple of generic types where we don't care about the result (ExprVisitor<T>, StmtVisitor<T>) and just use ExprVisitor<object> and return `null`. Since we generate them, we might as well generate a non-generic class that always has `void` return type.
- a few generators in one "GrammarGenerator" file, should split it
- GenerateClass() has code + string all mixed up. could easily rework nicer