# RG.CodeAnalyzer

[![NuGet](https://img.shields.io/nuget/v/RG.CodeAnalyzer.svg)](https://www.nuget.org/packages/RG.CodeAnalyzer/) [![NuGet](https://img.shields.io/nuget/v/RG.Annotations.svg)](https://www.nuget.org/packages/RG.Annotations/) [![.NET](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml)

## Installation
From Package Manager Console:
```
Install-Package RG.CodeAnalyzer
```

Best used together with [Microsoft.CodeAnalysis.NetAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.NetAnalyzers/) and [Roslynator](https://www.nuget.org/packages/Roslynator.Analyzers/)

### Q: There is nothing wrong with my code. How do I get rid of the warnings?
You can either:
1. Set the diagnostic severity to None, or
2. Suppress the warning and write your justification

### Q: I think one of the warnings is important and I don't want my developers to get away with it. How do I do it?
You can set the diagnostic severity to Error. This will prevent build from succeeding.

To learn more about changing diagnostic severity and suppressing warnings, visit: https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019

## Analyzers:
### 1. Do not `await` asynchronous operation inside a loop
```cs
for (;;) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside for loop.
}

foreach (var x in y) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside foreach loop.
}

while (true) {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside while loop.
}

do {
    await Task.Delay(1000); // RG0001: Asynchronous operation awaited inside do..while loop.
} while (true);
```

### 2. Do not return `Task` from a method that disposes object
```cs
Task Foo() { // RG0002: Method 'Foo' disposes an object and shouldn't return Task.
    using var cts = new CancellationTokenSource();
    return Task.Delay(1000, cts.Token);
}
```

### 3. Identifiers declared in Internal namespace must be internal.
```cs
namespace Foo.Internal.Models {
    public class Bar { // RG0003: Identifier 'Bar' is declared in 'Foo.Internal.Models' namespace, and thus must be declared internal.
    }
}
```

### 4. Do not access `private` fields using member access operator
```cs
class Foo {
    private int x;

    public void SetX(Foo obj, int value) {
        obj.x = value; // RG0004: Private field 'x' should not be accessed directly.
    }
}
```

### 5. Do not call `Dispose()` on static readonly fields
```cs
class Foo : IDisposable {
    private static readonly HttpClient _client = new HttpClient();_

    public void Dispose() {
        _client.Dispose(); // RG0005: Field '_client' is marked 'static readonly' and should not be disposed.
    }
}
```

### 6. Do not call `Task.Wait()` to invoke a `Task`
```cs
class Program {
    static void Main() {
        Foo.ListenAsync().Wait(); // RG0006: Calling Task.Wait() blocks current thread and is not recommended. Use await instead.
    }
}
```

### 7. Do not access Task<>.Result to invoke a Task
```cs
class Foo {
    private async Task<int> GetValueAsync() {
        return await Task.FromResult(0);
    }

    public int GetValue() {
        return GetValueAsync().Result; // RG0007: Accessing Task<>.Result blocks current thread and is not recommended. Use await instead.
    }
}
```

### 8. Tuple element names must be in PascalCase
```cs
(int firstItem, string secondItem) tuple = (1, "2"); // RG0008: 'firstItem' is not a proper name of a tuple element.
                                                     // RG0008: 'secondItem' is not a proper name of a tuple element.
(int a, string b) = (tuple.firstItem, tuple.secondItem);
```

### 9. Use overload which accepts `CancellationToken` whenever possible
```cs
public async Task<List<Product>> GetAllAsync(int id, CancellationToken cancellationToken) {
    return await _dbContext.Products.ToListAsync(); // RG0009: This method has an overload that accepts CancellationToken.
}
```

### 10. `var`'s inferred type is obsolete
```cs
var obj = GetObjOfObsoleteClass(); // RG0010: 'ObsoleteClass' is obsolete.
```

### 11. Interfaces shouldn't derive from IDisposable
```cs
interface I : IDisposable { } // RG0011: 'I' derives from IDisposable.
```

### 12. Task is unresolved
```cs
// TODO: implement this method please, Bob
// RG0012: Unresolved TODO: implement this method please, Bob
public int Foo() {
    // HACK: throw this exception until implemented
    // RG0012: Unresolved HACK: throw this exception until implemented
    throw new NotImplementedException();
}
```

### 13. `with` shouldn't be used outside its record declaration
```cs
record Foo(int X);

Foo f = new(0);
f = f with { X = 1 }; // RG0013: 'with' used outside 'MyApp.Foo'
```
[Suggested best practice](https://github.com/ronnygunawan/analyzers/issues/33)

### 14. Do not parse using `Convert`
```cs
int i = Convert.ToInt32("100"); // RG0014: Parsing 'int' using 'Convert.ToInt32'
                                // Code fix: Change to 'int.Parse'
```

### 15. Records should not contain `set` accessor
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; set; } // RG0015: 'Y' should not have set accessor because it's declared in a record
}
```

---
**MUTABLE RECORDS**

You can annotate records with `[Mutable]` attribute from [RG.Annotations](https://www.nuget.org/packages/RG.Annotations/) to skip all immutability checks (RG0015 to RG0020).

---

### 16. Records should not contain mutable field
```cs
record Foo {
    public readonly int A, B;
    public const int C;
    public int D; // RG0016: 'D' should not be mutable because it's declared in a record
}
```

### 17. Records should not contain mutable collection
```cs
record Foo(
    ImmutableArray<int> A,
    int[] B // RG0017: 'B' is a mutable collection and should not be used in a record
);
```

### 18. Records should not contain reference to `class` or `struct` type
```cs
class C { }
struct S { }
record R { }

record Foo(
    C C, // RG0018: 'C' is class type and should not be used in a record
    S S, // RG0018: 'S' is struct type and should not be used in a record
    R R
);
```

#### Banned types
- `class` (See "Allowed classes")
- `struct` (See "Allowed structs")
- `interface`
- `dynamic`
- `object`
- `Tuple`

#### Allowed types
- `record`
- `enum`
- `delegate`

#### Allowed `class`es
- `System.Uri`
- `System.Type`
- `System.Reflection.Module`
- `System.Reflection.Assembly`
- `System.Reflection.TypeInfo`
- `System.Reflection.MethodInfo`
- `System.Reflection.PropertyInfo`
- `System.Reflection.FieldInfo`
- `System.Reflection.ConstructorInfo`
- `System.Reflection.ParameterInfo`
- `System.Reflection.EventInfo`
- `System.Reflection.LocalVariableInfo`
- `System.Reflection.MemberInfo`
- `System.Reflection.ManifestResourceInfo`
- `System.Reflection.MethodBase`
- `System.Reflection.MethodBody`
- `System.Net.IPAddress`

#### Allowed `struct`s
- Primitive value types
- `struct`s in `System` namespace
- `struct`s in [`UnitsNet`](https://www.nuget.org/packages/UnitsNet/) namespace (3rd party library)

### 19. Required record property should be initialized
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; init; }

    [Required]
    public int Z { get; init; }
}

Foo foo = new() { // RG0019: 'Z' is a required property and should be initialized
    X = 0
};
```

#### Shorthand for `[Required]` attribute
You can also put an `@` prefix to record property name to mark it as a required property
```cs
record Foo {
    public int X { get; init; }
    public int Y { get; init; }
    public int @Z { get; init; }
}

Foo foo = new() { // RG0019: 'Z' is a required property and should be initialized
    X = 0
};
```

### 20. (Reserved for [#43](/../../issues/43))

### 21. Local is readonly
Put an `@` prefix to local name to mark it as a readonly local
```cs
int @max = 100;
max = 50; // RG0021: 'max' is a readonly local variable

```

---
**NOTE**

This is a work in progress and does not currently support `ref` locals, `ref readonly` locals, and `ref` expressions.

---

### 22. Parameter is readonly
```cs
void Foo(int @x) {
    x = 0; // RG0022: 'x' is a readonly parameter
}
```

### 23. Ref or out parameter cannot be readonly
```cs
void Foo(out int @x) { // RG0023: 'out' parameter 'x' cannot be readonly
}
```

### 24. (Reserved)

### 25. Casting to an incompatible enum
```cs
enum X { A, B, C }
enum Y { A, B, C, D }

X x = (X)Y.A; // RG0025: Casting to an incompatible enum; Value 3 is missing from 'X'
Y y = (Y)X.A; // no warning
```

### 26. Possibly casting to an incompatible enum: some names have different value
```cs
enum X { A, B, C }
enum Y { A, C, B }

X x = (X)Y.A; // Info: RG0026: Possibly casting to an incompatible enum; 'B' doesn't have the same value in 'X' and in 'Y'
```

### 27. Possibly casting to an incompatible enum: some values have different name
```cs
enum X { A, BB, C }
enum Y { A, B, C }

X x = (X)Y.A; // Info: RG0027: Possibly casting to an incompatible enum; Value 1 doesn't have a same name in 'X' and in 'Y'
```

### 28. All properties in protobuf message need to be initialized
```protobuf
// .proto
message X {
  int32 foo = 1;
  string bar = 2;
}
```

```cs
// .cs
X x = new() { // RG0028: 'Bar' is a required protobuf property and should be initialized
    Foo = 1
};
```

### 29. Only one of properties in a Oneof case can be initialized
```protobuf
// .proto
message X {
  oneof which {
    int32 foo = 1;
    string bar = 2;
  }
}
```

```cs
// .cs
X x = new() {
    Foo = 1,
    Bar = "Hello" // RG0029: 'Bar' cannot be initialized because 'Foo' has been initialized
};
```
