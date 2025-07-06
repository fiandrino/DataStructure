# DynamicArray<T>

> A high‑performance, single‑file replacement for `List<T>` with span‑aware APIs, fine‑grained capacity control and a battery‑included toolbox of **90+** methods. Targets .NET 6, 7, 8 and newer.

---

## 📑 Contents

1. [Why DynamicArray?](#-why-dynamicarray)
2. [Features](#-features)
3. [Installation](#-installation)
4. [Quick Start](#-quick-start)
5. [API Overview](#-api-overview)
6. [Complexity Analysis](#-complexity-analysis)
7. [Benchmarks](#-benchmarks)
8. [Memory & Growth Strategy](#-memory--growth-strategy)
9. [Roadmap](#-roadmap)
10. [Contributing](#-contributing)
11. [License](#-license)
12. [Changelog](#-changelog)

---

## ❓ Why DynamicArray?

`List<T>` covers 80 % of use‑cases but leaves power‑users wanting when they need:

* **Span‑friendly bulk APIs** (`AddRange(ReadOnlySpan<T>)`, in‑place compaction, etc.)
* **Predictable performance** measured in *nanoseconds*, matching raw array access
* **Advanced manipulations** (rotate, shuffle, flip, distinct, set ops…)
* **Statistics helpers** (sum, average, median, min, max)
* Full control over **capacity management** (trim, compact, ensure) *without* reflection hacks

DynamicArray delivers all that in a single, dependency‑free C# file you can drop anywhere.

---

## ✨ Features

| Category            | Highlight                                                                          |
| ------------------- | ---------------------------------------------------------------------------------- |
| **Performance**     | Inlined fast‑paths for `Add`, `Insert`, `Remove` rival raw array speed (≤ 1 ns/op) |
| **Bulk operations** | `AddRange`/`InsertRange` on `IEnumerable<T>` *and* `ReadOnlySpan<T>`               |
| **Math helpers**    | `Sum`, `Average`, `Median`, `Min`, `Max` for numeric types                         |
| **Algorithms**      | `Shuffle` (Fisher‑Yates), `Rotate`, `Flip`, `Reverse`, `Sort`                      |
| **Search**          | SIMD‑backed `IndexOf`/`LastIndexOf`, `BinarySearch`, `Find/FindAll`                |
| **Set‑like**        | `Distinct`, `AddIfNotContains`, `SequenceEqual`                                    |
| **String utils**    | `ToUpper`, `ToLower`, `ChangeCaseRange` (culture‑aware)                            |
| **Factory**         | `CreateRandom` (numeric) & `CreateRandomStrings`                                   |
| **Safety**          | Full argument validation, XML‑docs, 100 % tests coverage                           |

---

## 📦 Installation

```bash
# .NET CLI
dotnet add package DynamicArray.Core
```

Or copy [`DynamicArray.cs`](src/DynamicArray.cs) into your project (no external deps).

---

## ⚡ Quick Start

```csharp
var scores = new DynamicArray<int>();

scores.AddRange(stackalloc[] { 23, 42, 7, 15 }); // span overload
scores.Insert(2, 99);                             // [23, 42, 99, 7, 15]

Console.WriteLine($"Median: {scores.Median()}"); // 23

scores.Shuffle();
foreach (var s in scores)
    Console.WriteLine(s);
```

---

## 🔍 API Overview

> **90+ public methods** are grouped below. See XML docs or IntelliSense for full signatures.

| Group            | Key Methods                                                                       |
| ---------------- | --------------------------------------------------------------------------------- |
| **Add / Insert** | `Add`, `AddRange`, `Insert`, `InsertRange`, `AddIfNotContains`                    |
| **Remove**       | `Remove`, `RemoveAt`, `RemoveLast`, `RemoveRange`, `RemoveAll`, `Clear`           |
| **Transform**    | `Sort`, `Reverse`, `Rotate`, `Flip`, `Shuffle`, `TrimExcess`, `Compact`, `Resize` |
| **Search**       | `IndexOf`, `LastIndexOf`, `BinarySearch`, `Find`, `FindAll`, `Contains`           |
| **Math / Stats** | `Sum`, `Average`, `Median`, `Min`, `Max`                                          |
| **Conversion**   | `ToArray`, `Clone`, `ConvertAll`                                                  |
| **String ext.**  | `ToUpper`, `ToLower`, `ChangeCaseRange`                                           |

---

## 📈 Complexity Analysis

> Notation: **n** = current size, **m** = added elements, **k** = removed/matched elements, **L** = avg string length.

| Operation                | Time            | Space         | Notes                               |
| ------------------------ | --------------- | ------------- | ----------------------------------- |
| `Add`                    | **O(1) amort.** | O(1)          | Fast‑path + doubling strategy       |
| `AddRange(ReadOnlySpan)` | O(m)            | O(n+m) amort. | Zero‑alloc copy when room fits      |
| `Insert`                 | O(n)            | O(1)          | Tail copied once via `Array.Copy`   |
| `InsertRange`            | O(n+m)          | O(1)          | Tail shift + single materialisation |
| `RemoveAt`               | O(n)            | O(1)          | Shift left tail                     |
| `RemoveRange`            | O(n)            | O(1)          | Single pass copy                    |
| `RemoveAll`              | O(n)            | O(1)          | Span compaction                     |
| `Clear`                  | O(1) / O(n)     | O(1)          | Value vs reference types            |
| `Sort`                   | O(n log n)      | O(log n)      | Array.Introsort                     |
| `Median`                 | O(n log n)      | O(n)          | Uses temp copy                      |
| `IndexOf`                | O(n)            | O(1)          | SIMD for default comparer           |
| `BinarySearch`           | O(log n)        | O(1)          | Requires sorted array               |
| `Sum`, `Average`         | O(n)            | O(1)          | Numeric only                        |

<details>
<summary>Full table »</summary>

*(expanded table as provided in docs/Complexity.md)*

</details>

---

## 🏎️ Benchmarks (v2.1)

| Scenario (int)           | `List<T>` | DynamicArray | Speed‑up  |
| ------------------------ | --------- | ------------ | --------- |
| 10 M × `Add`             | 0.90 ms   | 0.98 ms      | 0.9×      |
| Append w/ resize (cold)  | 1.2 µs    | 1.3 µs       | 0.9×      |
| Head `InsertRange` 100 k | 5.2 s     | 0.18 s       | **× 28**  |
| Mid `RemoveRange` 100 k  | 0.23 s    | 0.09 s       | **× 2.5** |
| `RemoveAll` 50 % match   | 0.31 s    | 0.14 s       | **× 2.2** |

*Run on Ryzen 7 5700U • .NET 8 • Tiered PGO ON. Full BenchmarkDotNet logs in `benchmarks/`.*

---

## 🧠 Memory & Growth Strategy

* **Growth factor 2×** – balances realloc cost and fragmentation (configurable).
* **Shrink‑threshold 25 %** – capacity halves when usage ≤ 25 % to avoid thrashing.
* `Compact()` – forces capacity = Count for memory‑constrained scenarios.
* All internal copies leverage `Array.Copy` or `Span<T>.CopyTo` for SIMD moves.

---

## 📅 Roadmap

* 🔄 **Immutable wrapper** (`ReadOnlyDynamicArray<T>`)
* 🧵 **Thread‑safe variant** with striped locks
* 🗂️ `GroupBy`/`Chunk` helpers (LINQ‑like without allocs)
* 📉 Optional `IReadOnlyList<T>`/`IList<T>` adapter for drop‑in replacement

Feel free to 👍 on issues or open PRs to influence priority.

---

## 🤝 Contributing

1. Fork & clone
2. `dotnet test` (all green?)
3. `dotnet benchmark` (optional) – ensure no perf regression
4. Commit using Conventional Commits (`feat:`, `fix:`, `perf:`…)
5. Open PR against `main`

We appreciate unit tests, benchmarks and real‑world usage reports!

---

## ⚖️ License

**MIT** – do whatever you want, but don’t blame us if it explodes 🧨.
See [LICENSE](LICENSE).

---

## 📝 Changelog

### 2.1 – 2025‑07‑06

* ♻️ Re‑wrote `Add` with fast/slow path – now parity with `List<T>`
* 🚀 `InsertRange`, `RemoveRange`, `RemoveAll` single‑pass implementations
* 🔍 `IndexOf`/`LastIndexOf` delegate to SIMD runtime paths
* ✨ Minor API polish – `Shuffle` now uses `Random.Shared`
* 📝 Added full Complexity Matrix & Benchmarks docs

<details>
<summary>Older entries</summary>

* **2.0 – 2025‑05‑22** – Initial public release
* **1.x** – Internal iterations

</details>

