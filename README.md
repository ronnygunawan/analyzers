# RG.CodeAnalyzer

[![NuGet](https://img.shields.io/nuget/v/RG.CodeAnalyzer.svg)](https://www.nuget.org/packages/RG.CodeAnalyzer/)

## Installation
From Package Manager Console:
```
Install-Package RG.CodeAnalyzer
```

Best used together with [Microsoft.CodeAnalysis.FxCopAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/) and [Roslynator](https://www.nuget.org/packages/Roslynator.Analyzers/)

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