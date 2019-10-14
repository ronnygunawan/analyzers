# RG.CodeAnalyzer

## Analyzers:
### 1. Do not await asynchronous operation inside a loop
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
