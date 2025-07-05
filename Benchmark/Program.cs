using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataStructure;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== DynamicArray Demo (.NET 5.0) ===");

            // 1. Creazione di array dinamici
            var intArray = new DynamicArray<int>();
            var stringArray = new DynamicArray<string>();

            // 2. Aggiunta di elementi
            intArray.AddRange(Enumerable.Range(1, 5));
            stringArray.AddRange(new[] { "Apple", "Banana", "Cherry" });

            Console.WriteLine("\nArray iniziali:");
            PrintArray("Interi", intArray);
            PrintArray("Stringhe", stringArray);

            // 3. Operazioni avanzate
            Console.WriteLine("\nOperazioni avanzate:");

            // Inserimento
            intArray.Insert(2, 100);
            Console.WriteLine("Dopo inserimento (intArray[2] = 100):");
            PrintArray("Interi", intArray);

            // Rimozione
            intArray.RemoveAt(3);
            Console.WriteLine("Dopo rimozione (intArray[3]):");
            PrintArray("Interi", intArray);

            // Operazioni matematiche
            Console.WriteLine($"Somma: {intArray.Sum()}");
            Console.WriteLine($"Media: {intArray.Average():F2}");
            Console.WriteLine($"Minimo: {intArray.Min()}");
            Console.WriteLine($"Massimo: {intArray.Max()}");

            // 4. Test prestazionali
            Console.WriteLine("\nTest prestazionali:");
            TestPerformance();

            // 5. Funzionalità speciali
            Console.WriteLine("\nFunzionalità speciali:");

            // Rotazione
            intArray.Rotate(2);
            Console.WriteLine("Dopo rotazione di 2 posizioni:");
            PrintArray("Interi", intArray);

            // Mescolamento
            intArray.Shuffle();
            Console.WriteLine("Dopo mescolamento:");
            PrintArray("Interi", intArray);

            // Operazioni su stringhe
            stringArray.ToUpper();
            Console.WriteLine("Stringhe in maiuscolo:");
            PrintArray("Stringhe", stringArray);

            // 6. Test di grandi dimensioni
            Console.WriteLine("\nTest array grande (1,000,000 elementi):");
            TestLargeArray();
        }

        static void PrintArray<T>(string name, DynamicArray<T> array)
        {
            Console.WriteLine($"{name}: [{string.Join(", ", array)}]");
        }

        static void TestPerformance()
        {
            const int Size = 100_000;
            var stopwatch = new Stopwatch();
            var random = new Random();

            // Test List<T>
            var list = new List<int>();
            stopwatch.Start();
            for (int i = 0; i < Size; i++)
            {
                list.Add(i);
            }
            stopwatch.Stop();
            Console.WriteLine($"List.Add: {stopwatch.ElapsedMilliseconds} ms");

            // Test DynamicArray
            var dynArray = new DynamicArray<int>();
            stopwatch.Restart();
            for (int i = 0; i < Size; i++)
            {
                dynArray.Add(i);
            }
            stopwatch.Stop();
            Console.WriteLine($"DynamicArray.Add: {stopwatch.ElapsedMilliseconds} ms");

            // Test inserimento all'inizio
            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                list.Insert(0, i);
            }
            stopwatch.Stop();
            Console.WriteLine($"List.Insert(0): {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                dynArray.Insert(0, i);
            }
            stopwatch.Stop();
            Console.WriteLine($"DynamicArray.Insert(0): {stopwatch.ElapsedMilliseconds} ms");

            // Test ricerca
            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                _ = list.IndexOf(random.Next(Size));
            }
            stopwatch.Stop();
            Console.WriteLine($"List.IndexOf: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                _ = dynArray.IndexOf(random.Next(Size));
            }
            stopwatch.Stop();
            Console.WriteLine($"DynamicArray.IndexOf: {stopwatch.ElapsedMilliseconds} ms");
        }

        static void TestLargeArray()
        {
            const int Size = 1_000_000;
            var array = new DynamicArray<int>();

            // Popolamento
            array.AddRange(Enumerable.Range(1, Size));

            // Operazioni
            Console.WriteLine($"Elementi: {array.Count}");
            Console.WriteLine($"Capacità iniziale: {array.Capacity}");

            // Rimozione elementi
            array.RemoveAll(x => x % 2 == 0);
            Console.WriteLine($"Dopo rimozione elementi pari: {array.Count} elementi");

            // Compattazione
            array.Compact();
            Console.WriteLine($"Capacità dopo compact: {array.Capacity}");

            // Ordinamento
            var stopwatch = Stopwatch.StartNew();
            array.Sort();
            Console.WriteLine($"Ordinamento completato in {stopwatch.ElapsedMilliseconds} ms");

            // Ricerca
            stopwatch.Restart();
            int index = array.BinarySearch(Size / 2);
            Console.WriteLine($"Ricerca binaria: elemento trovato a indice {index} in {stopwatch.ElapsedTicks} ticks");
        }
    }
}