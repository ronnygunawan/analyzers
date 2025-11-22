# RG.CodeAnalyzer

[![NuGet](https://img.shields.io/nuget/v/RG.CodeAnalyzer.svg)](https://www.nuget.org/packages/RG.CodeAnalyzer/) [![.NET](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/analyzers/actions/workflows/dotnet.yml)

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

[![NuGet](https://img.shields.io/nuget/v/RG.Annotations.svg)](https://www.nuget.org/packages/RG.Annotations/)

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

ref int @refLocal = ref someVar; // OK if someVar is readonly
ref int @refLocal2 = ref mutableVar; // RG0021: cannot assign mutable source to readonly ref local

ref readonly int @refReadonlyLocal = ref someVar; // OK, ref readonly can reference any non-ref-parameter
```

---
**NOTE**

This feature now supports `ref` locals and `ref readonly` locals. Ref reassignments (e.g., `ref x = ref y;` where x is already declared) are not yet fully implemented.

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

### 24. In argument should be readonly
```cs
void Foo(in int value) {
    // ...
}

int mutableVar = 0;
Foo(in mutableVar); // RG0024: 'in' argument 'mutableVar' should be readonly

int @readonlyVar = 0;
Foo(in readonlyVar); // OK
```

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

### 31. Do not use `dynamic` type
```cs
dynamic x = 1; // RG0031: Do not use dynamic type
```

### 32. Static classes containing extension methods should have an 'Extensions' suffix
```cs
static class Utilities { // RG0032: 'Utilities' contains extension methods and should have an 'Extensions' suffix
    public static void Foo(this string bar) {
    }
}
```
Code fix: Rename to `UtilitiesExtensions`

### 33. Use overload without CancellationToken if default or CancellationToken.None was supplied
```cs
var list = await _dbContext.Items.ToListAsync(CancellationToken.None); // RG0033: Use overload without CancellationToken instead
```
If you explicitly pass `CancellationToken.None` or `default` to a method that has an overload without the CancellationToken parameter, use the overload without it instead.

Code fix: Remove the CancellationToken argument

## Code Refactorings

### Generate GUID in empty string literal
Place your cursor on an empty string literal `""` and invoke code actions (Ctrl+. or Cmd+.) to see the "Generate GUID" refactoring option.
```cs
string id = ""; // Offers: Generate GUID
// After applying: string id = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
```

### 34. Service registered with Add{Lifetime} must have corresponding lifetime attribute
Services registered with dependency injection must be marked with the appropriate lifetime attribute.

```cs
using Microsoft.Extensions.DependencyInjection;

namespace MyApp {
    class MyService { // RG0034: Service 'MyService' registered with AddSingleton must be marked with [Singleton] attribute
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<MyService>();
        }
    }
}
```

Correct usage:
```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    [Singleton]
    class MyService {
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<MyService>();
        }
    }
}
```

### 35. Service with longer lifetime cannot depend on service with shorter lifetime
A service with a longer lifetime (Singleton > Scoped > Transient) cannot depend on a service with a shorter lifetime.

```cs
using RG.Annotations;

namespace MyApp {
    [Transient]
    class TransientService {
    }
    
    [Singleton]
    class SingletonService {
        // RG0035: Singleton service 'SingletonService' cannot depend on Transient service 'TransientService'
        public SingletonService(TransientService service) {
        }
    }
}
```

This rule also applies to property and field dependencies.

### 36. DI implementation classes should be internal
Classes registered with dependency injection should be internal to hide implementation details. The interface can remain public.

```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    public interface IMyService {
    }
    
    [Transient]
    public class MyService : IMyService { // RG0036: Class 'MyService' is registered with DI and should be internal
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddTransient<IMyService, MyService>();
        }
    }
}
```

Correct usage:
```cs
using Microsoft.Extensions.DependencyInjection;
using RG.Annotations;

namespace MyApp {
    public interface IMyService {
    }
    
    [Transient]
    internal class MyService : IMyService { // OK
    }
    
    class Startup {
        void ConfigureServices(IServiceCollection services) {
            services.AddTransient<IMyService, MyService>();
        }
    }
}
```

Code fix: Make the implementation class internal

### 38. Pending justification for suppressing code analysis message
```cs
using System.Diagnostics.CodeAnalysis;

class MyClass {
    [SuppressMessage("Category", "RG0001")] // RG0038: Justification is required for suppressing message 'RG0001'
    [SuppressMessage("Category", "RG0002", Justification = "")] // RG0038: Justification is required for suppressing message 'RG0002'
    [SuppressMessage("Category", "RG0003", Justification = "<Pending>")] // RG0038: Justification is required for suppressing message 'RG0003'
    public void MyMethod() {
    }
}
```

When using `[SuppressMessage]` attribute to suppress code analysis warnings, you must always provide the `Justification` parameter with a meaningful, non-empty explanation of why the warning is being suppressed. This ensures code suppressions are documented and justified. The analyzer detects missing justifications, empty justifications, and the default `"<Pending>"` placeholder generated by code fixes.

Valid usage:
```cs
using System.Diagnostics.CodeAnalysis;

class MyClass {
    [SuppressMessage("Category", "RG0001", Justification = "Performance-critical path, async not needed")]
    public void MyMethod() {
    }
}
```
