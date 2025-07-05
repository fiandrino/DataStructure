using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructure
{
    /// <summary>
    /// Array dinamico con funzionalità avanzate di manipolazione, ricerca e gestione della capacità.
    /// Supporta operazioni simili a List<T> con ottimizzazioni aggiuntive.
    /// </summary>
    /// <typeparam name="T">Tipo degli elementi nell'array</typeparam>
    public class DynamicArray<T> : IEnumerable<T>
    {
        #region Costanti e campi interni
        private const int GrowthFactor = 2;           // Fattore di crescita quando si espande
        private const double ShrinkThreshold = 0.25;  // Soglia per la riduzione della capacità

        private T[] _buffer;                         // Buffer interno per l'archiviazione
        #endregion

        #region Proprietà pubbliche
        /// <summary>Capacità corrente dell'array</summary>
        public int Capacity { get; private set; }

        /// <summary>Numero di elementi effettivamente presenti</summary>
        public int Count { get; private set; }
        #endregion

        #region Costruttori e inizializzazione
        /// <summary>Crea un nuovo array dinamico con capacità iniziale</summary>
        /// <param name="capacity">Capacità iniziale (default: 4)</param>
        /// <exception cref="ArgumentOutOfRangeException">Se la capacità è ≤ 0</exception>
        public DynamicArray(int capacity = 4)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new T[capacity];
            Capacity = capacity;
        }

        /// <summary>Crea un nuovo array dinamico da una collezione esistente</summary>
        /// <param name="collection">Collezione di elementi da aggiungere</param>
        public DynamicArray(IEnumerable<T> collection) : this()
        {
            AddRange(collection);
        }
        #endregion

        #region Metodi factory per creazione casuale
        /// <summary>
        /// Crea un nuovo array con elementi numerici casuali
        /// </summary>
        /// <param name="count">Numero di elementi da generare</param>
        /// <param name="minValue">Valore minimo (incluso)</param>
        /// <param name="maxValue">Valore massimo (escluso)</param>
        /// <param name="rng">Generatore di numeri casuali (opzionale)</param>
        /// <exception cref="InvalidOperationException">Se il tipo non è supportato</exception>
        public static DynamicArray<T> CreateRandom(
            int count,
            double minValue = 0,
            double maxValue = 100,
            Random rng = null)
        {
            if (!IsNumericType(typeof(T)))
                throw new InvalidOperationException("Tipo non numerico non supportato");

            rng ??= new Random();
            var array = new DynamicArray<T>(count);

            for (int i = 0; i < count; i++)
            {
                double randomValue = minValue + (rng.NextDouble() * (maxValue - minValue));
                array.Add((T)Convert.ChangeType(randomValue, typeof(T)));
            }

            return array;
        }

        /// <summary>
        /// Crea un nuovo array con stringhe casuali (solo per T = string)
        /// </summary>
        /// <param name="count">Numero di stringhe da generare</param>
        /// <param name="minLength">Lunghezza minima stringa (default: 1)</param>
        /// <param name="maxLength">Lunghezza massima stringa (default: 20)</param>
        /// <param name="rng">Generatore di numeri casuali (opzionale)</param>
        /// <exception cref="InvalidOperationException">Se T non è string</exception>
        public static DynamicArray<T> CreateRandomStrings(
            int count,
            int minLength = 1,
            int maxLength = 20,
            Random rng = null)
        {
            if (typeof(T) != typeof(string))
                throw new InvalidOperationException("Il metodo è valido solo per stringhe");

            rng ??= new Random();
            var array = new DynamicArray<T>(count);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            for (int i = 0; i < count; i++)
            {
                int length = rng.Next(minLength, maxLength + 1);
                string randomString = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[rng.Next(s.Length)]).ToArray());

                array.Add((T)(object)randomString);
            }

            return array;
        }
        #endregion

        #region Operazioni di aggiunta
        /// <summary>Aggiunge un elemento alla fine dell'array</summary>
        /// <param name="value">Elemento da aggiungere</param>
        public void Add(T value)
        {
            EnsureSpace(1);
            _buffer[Count++] = value;
        }

        /// <summary>Aggiunge una sequenza di elementi da uno span</summary>
        /// <param name="values">Elementi da aggiungere</param>
        public void AddRange(ReadOnlySpan<T> values)
        {
            if (values.IsEmpty) return;

            EnsureSpace(values.Length);
            values.CopyTo(_buffer.AsSpan(Count));
            Count += values.Length;
        }

        /// <summary>Aggiunge una sequenza di elementi da un'enumerabile</summary>
        /// <param name="values">Elementi da aggiungere</param>
        /// <exception cref="ArgumentNullException">Se values è null</exception>
        public void AddRange(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            // Ottimizzazione per collezioni con dimensioni note
            if (values is ICollection<T> collection)
            {
                EnsureSpace(collection.Count);
                collection.CopyTo(_buffer, Count);
                Count += collection.Count;
            }
            else
            {
                foreach (var item in values)
                    Add(item);
            }
        }
        #endregion

        #region Operazioni di inserimento
        /// <summary>Inserisce un elemento alla posizione specificata</summary>
        /// <param name="index">Indice di inserimento</param>
        /// <param name="value">Elemento da inserire</param>
        /// <exception cref="ArgumentOutOfRangeException">Se l'indice è fuori range</exception>
        public void Insert(int index, T value)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            EnsureSpace(1);

            // Sposta gli elementi per creare spazio
            if (index < Count)
                _buffer.AsSpan(index, Count - index).CopyTo(_buffer.AsSpan(index + 1));

            _buffer[index] = value;
            Count++;
        }

        /// <summary>Inserisce una sequenza di elementi alla posizione specificata</summary>
        /// <param name="index">Indice di partenza</param>
        /// <param name="items">Elementi da inserire</param>
        /// <exception cref="ArgumentNullException">Se items è null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se l'indice è fuori range</exception>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Ottimizzazione per collezioni con dimensioni note
            if (items is ICollection<T> collection)
            {
                int newItemsCount = collection.Count;
                EnsureSpace(newItemsCount);

                // Sposta gli elementi esistenti
                if (Count > index)
                    Array.Copy(_buffer, index, _buffer, index + newItemsCount, Count - index);

                collection.CopyTo(_buffer, index);
                Count += newItemsCount;
            }
            else
            {
                foreach (var item in items)
                    Insert(index++, item);
            }
        }
        #endregion

        #region Operazioni di rimozione
        /// <summary>Rimuove l'elemento alla posizione specificata</summary>
        /// <param name="index">Indice dell'elemento da rimuovere</param>
        /// <exception cref="ArgumentOutOfRangeException">Se l'indice è fuori range</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Sposta gli elementi successivi
            if (index < Count - 1)
                _buffer.AsSpan(index + 1, Count - index - 1).CopyTo(_buffer.AsSpan(index));

            Count--;

            // Pulisci riferimento per evitare memory leak
            if (!typeof(T).IsValueType)
                _buffer[Count] = default;

            MaybeShrink();
        }

        /// <summary>Rimuove la prima occorrenza dell'elemento specificato</summary>
        /// <param name="item">Elemento da rimuovere</param>
        /// <returns>True se trovato e rimosso, altrimenti false</returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>Rimuove e restituisce l'ultimo elemento</summary>
        /// <returns>Ultimo elemento rimosso</returns>
        /// <exception cref="InvalidOperationException">Se l'array è vuoto</exception>
        public T RemoveLast()
        {
            if (Count == 0)
                throw new InvalidOperationException("Array vuoto");

            T last = _buffer[Count - 1];
            RemoveAt(Count - 1);
            return last;
        }

        /// <summary>Rimuove un intervallo di elementi</summary>
        /// <param name="startIndex">Indice di inizio</param>
        /// <param name="endIndex">Indice di fine (incluso)</param>
        /// <exception cref="ArgumentOutOfRangeException">Se gli indici sono non validi</exception>
        public void RemoveRange(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (endIndex < 0 || endIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(endIndex));
            if (startIndex > endIndex)
                throw new ArgumentException("startIndex deve essere ≤ endIndex", nameof(startIndex));

            int removeCount = endIndex - startIndex + 1;
            int tailCount = Count - endIndex - 1;

            // Sposta gli elementi successivi
            if (tailCount > 0)
                _buffer.AsSpan(endIndex + 1, tailCount).CopyTo(_buffer.AsSpan(startIndex));

            Count -= removeCount;

            // Pulisci riferimenti
            if (!typeof(T).IsValueType)
                Array.Clear(_buffer, Count, removeCount);

            MaybeShrink();
        }

        /// <summary>Rimuove tutti gli elementi che soddisfano una condizione</summary>
        /// <param name="match">Predicato per filtrare gli elementi</param>
        /// <returns>Numero di elementi rimossi</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int freeIndex = 0;
            // Filtra gli elementi da mantenere
            for (int i = 0; i < Count; i++)
            {
                if (!match(_buffer[i]))
                {
                    _buffer[freeIndex++] = _buffer[i];
                }
            }

            int removed = Count - freeIndex;
            if (removed > 0)
            {
                // Pulisci gli elementi rimossi
                if (!typeof(T).IsValueType)
                    Array.Clear(_buffer, freeIndex, removed);

                Count = freeIndex;
                MaybeShrink();
            }
            return removed;
        }

        /// <summary>Svuota completamente l'array</summary>
        public void Clear()
        {
            if (!typeof(T).IsValueType)
                Array.Clear(_buffer, 0, Count);

            Count = 0;
        }
        #endregion

        #region Operazioni matematiche
        /// <summary>Calcola la somma degli elementi</summary>
        /// <returns>Somma convertita in double</returns>
        public double Sum()
        {
            double acc = 0;
            for (int i = 0; i < Count; i++)
                acc += Convert.ToDouble(_buffer[i]);
            return acc;
        }

        /// <summary>Calcola la media degli elementi</summary>
        /// <returns>Media degli elementi</returns>
        /// <exception cref="InvalidOperationException">Se l'array è vuoto</exception>
        public double Average()
        {
            if (Count == 0)
                throw new InvalidOperationException("Array vuoto.");

            return Sum() / Count;
        }

        /// <summary>Trova l'elemento minimo</summary>
        /// <param name="comparer">Comparatore personalizzato (opzionale)</param>
        /// <returns>Elemento minimo</returns>
        /// <exception cref="InvalidOperationException">Se l'array è vuoto</exception>
        public T Min(IComparer<T> comparer = null)
        {
            if (Count == 0)
                throw new InvalidOperationException("Array vuoto.");

            comparer ??= Comparer<T>.Default;
            T min = _buffer[0];

            for (int i = 1; i < Count; i++)
                if (comparer.Compare(_buffer[i], min) < 0)
                    min = _buffer[i];

            return min;
        }

        /// <summary>Trova l'elemento massimo</summary>
        /// <param name="comparer">Comparatore personalizzato (opzionale)</param>
        /// <returns>Elemento massimo</returns>
        /// <exception cref="InvalidOperationException">Se l'array è vuoto</exception>
        public T Max(IComparer<T> comparer = null)
        {
            if (Count == 0)
                throw new InvalidOperationException("Array vuoto.");

            comparer ??= Comparer<T>.Default;
            T max = _buffer[0];

            for (int i = 1; i < Count; i++)
                if (comparer.Compare(_buffer[i], max) > 0)
                    max = _buffer[i];

            return max;
        }

        /// <summary>Calcola la mediana degli elementi</summary>
        /// <returns>Elemento mediano</returns>
        /// <exception cref="InvalidOperationException">Se l'array è vuoto</exception>
        public T Median()
        {
            if (Count == 0)
                throw new InvalidOperationException("Array vuoto.");

            var copy = new T[Count];
            Array.Copy(_buffer, copy, Count);
            Array.Sort(copy);
            return copy[Count / 2];
        }
        #endregion

        #region Ordinamento e inversione
        /// <summary>Ordina gli elementi</summary>
        /// <param name="comparer">Comparatore personalizzato (opzionale)</param>
        public void Sort(IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            Array.Sort(_buffer, 0, Count, comparer);
        }

        /// <summary>Inverte l'ordine degli elementi</summary>
        public void Reverse() => Reverse(0, Count - 1);
        #endregion

        #region Ricerca e ricerca avanzata
        /// <summary>Trova l'indice della prima occorrenza di un elemento</summary>
        /// <param name="item">Elemento da cercare</param>
        /// <param name="start">Indice di partenza</param>
        /// <param name="comparer">Comparatore di uguaglianza (opzionale)</param>
        /// <returns>Indice dell'elemento o -1 se non trovato</returns>
        /// <exception cref="ArgumentOutOfRangeException">Se start è fuori range</exception>
        public int IndexOf(T item, int start = 0, IEqualityComparer<T> comparer = null)
        {
            if (start < 0 || start >= Count)
                throw new ArgumentOutOfRangeException(nameof(start));

            comparer ??= EqualityComparer<T>.Default;

            for (int i = start; i < Count; i++)
                if (comparer.Equals(_buffer[i], item))
                    return i;

            return -1;
        }

        /// <summary>Trova l'indice dell'ultima occorrenza di un elemento</summary>
        /// <param name="item">Elemento da cercare</param>
        /// <param name="comparer">Comparatore di uguaglianza (opzionale)</param>
        /// <returns>Indice dell'elemento o -1 se non trovato</returns>
        public int LastIndexOf(T item, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = Count - 1; i >= 0; i--)
                if (comparer.Equals(_buffer[i], item))
                    return i;

            return -1;
        }

        /// <summary>Verifica se un elemento è presente</summary>
        /// <param name="item">Elemento da cercare</param>
        /// <returns>True se trovato, altrimenti false</returns>
        public bool Contains(T item) => Contains(item, null);

        /// <summary>Verifica se un elemento è presente usando un comparatore specifico</summary>
        /// <param name="item">Elemento da cercare</param>
        /// <param name="comparer">Comparatore di uguaglianza (opzionale)</param>
        /// <returns>True se trovato, altrimenti false</returns>
        public bool Contains(T item, IEqualityComparer<T> comparer)
            => IndexOf(item, 0, comparer) != -1;

        /// <summary>Ricerca binaria di un elemento ordinato</summary>
        /// <param name="item">Elemento da cercare</param>
        /// <param name="comparer">Comparatore per l'ordinamento (opzionale)</param>
        /// <returns>Indice dell'elemento o complemento a 2 se non trovato</returns>
        public int BinarySearch(T item, IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            return Array.BinarySearch(_buffer, 0, Count, item, comparer);
        }

        /// <summary>Trova il primo elemento che soddisfa una condizione</summary>
        /// <param name="match">Predicato per filtrare gli elementi</param>
        /// <returns>Elemento trovato o default(T) se non trovato</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        public T Find(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < Count; i++)
                if (match(_buffer[i]))
                    return _buffer[i];

            return default;
        }

        /// <summary>Trova tutti gli elementi che soddisfano una condizione</summary>
        /// <param name="match">Predicato per filtrare gli elementi</param>
        /// <returns>Nuovo array dinamico con gli elementi trovati</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        public DynamicArray<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            var result = new DynamicArray<T>();
            for (int i = 0; i < Count; i++)
                if (match(_buffer[i]))
                    result.Add(_buffer[i]);

            return result;
        }

        /// <summary>Trova l'indice del primo elemento che soddisfa una condizione</summary>
        /// <param name="match">Predicato per filtrare gli elementi</param>
        /// <param name="startIndex">Indice di partenza</param>
        /// <returns>Indice dell'elemento o -1 se non trovato</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se startIndex è fuori range</exception>
        public int FindIndex(Predicate<T> match, int startIndex = 0)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            for (int i = startIndex; i < Count; i++)
                if (match(_buffer[i]))
                    return i;

            return -1;
        }

        /// <summary>Trova l'indice dell'ultimo elemento che soddisfa una condizione</summary>
        /// <param name="match">Predicato per filtrare gli elementi</param>
        /// <returns>Indice dell'elemento o -1 se non trovato</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        public int FindLastIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = Count - 1; i >= 0; i--)
                if (match(_buffer[i]))
                    return i;

            return -1;
        }
        #endregion

        #region Iterazione ed enumerazione
        /// <summary>Restituisce un enumeratore per scorrere gli elementi</summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _buffer[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Esegue un'azione per ogni elemento</summary>
        /// <param name="action">Azione da eseguire</param>
        /// <exception cref="ArgumentNullException">Se action è null</exception>
        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < Count; i++)
                action(_buffer[i]);
        }
        #endregion

        #region Utilità e operazioni varie
        /// <summary>Crea un array statico con gli elementi correnti</summary>
        public T[] ToArray()
        {
            var result = new T[Count];
            Array.Copy(_buffer, result, Count);
            return result;
        }

        /// <summary>Scambia due elementi per posizione</summary>
        /// <param name="i">Indice del primo elemento</param>
        /// <param name="j">Indice del secondo elemento</param>
        /// <exception cref="ArgumentOutOfRangeException">Se gli indici sono fuori range</exception>
        public void Swap(int i, int j)
        {
            if (i < 0 || i >= Count || j < 0 || j >= Count)
                throw new ArgumentOutOfRangeException();

            (_buffer[i], _buffer[j]) = (_buffer[j], _buffer[i]);
        }

        /// <summary>Mischia casualmente gli elementi</summary>
        /// <param name="rng">Generatore di numeri casuali (opzionale)</param>
        public void Shuffle(Random rng = null)
        {
            rng ??= new Random();
            for (int i = Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Swap(i, j);
            }
        }

        /// <summary>Riduce la capacità se eccede la soglia specificata</summary>
        /// <param name="threshold">Soglia di utilizzo (default: 90%)</param>
        public void TrimExcess(double threshold = 0.9)
        {
            if (Count < Capacity * threshold)
                ResizeBuffer(Count);
        }

        /// <summary>Crea una copia superficiale dell'array</summary>
        public DynamicArray<T> Clone()
        {
            var clone = new DynamicArray<T>(Capacity);
            clone.AddRange(_buffer.AsSpan(0, Count));
            return clone;
        }

        /// <summary>Converte tutti gli elementi in un altro tipo</summary>
        /// <typeparam name="TOutput">Tipo di output</typeparam>
        /// <param name="converter">Funzione di conversione</param>
        /// <returns>Nuovo array convertito</returns>
        /// <exception cref="ArgumentNullException">Se converter è null</exception>
        public DynamicArray<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var result = new DynamicArray<TOutput>(Count);
            for (int i = 0; i < Count; i++)
                result.Add(converter(_buffer[i]));

            return result;
        }

        /// <summary>Sposta un elemento da una posizione a un'altra</summary>
        /// <param name="fromIndex">Posizione originale</param>
        /// <param name="toIndex">Posizione di destinazione</param>
        /// <exception cref="ArgumentOutOfRangeException">Se gli indici sono fuori range</exception>
        public void Move(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            if (toIndex < 0 || toIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));

            T item = _buffer[fromIndex];
            RemoveAt(fromIndex);
            Insert(toIndex, item);
        }

        /// <summary>Sostituisce tutte le occorrenze di un valore</summary>
        /// <param name="oldValue">Valore da sostituire</param>
        /// <param name="newValue">Nuovo valore</param>
        /// <param name="comparer">Comparatore per l'identificazione (opzionale)</param>
        public void Replace(T oldValue, T newValue, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
                if (comparer.Equals(_buffer[i], oldValue))
                    _buffer[i] = newValue;
        }
        #endregion

        #region Gestione capacità
        /// <summary>Garantisce una capacità minima</summary>
        /// <param name="newCapacity">Nuova capacità minima</param>
        public void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > Capacity)
                ResizeBuffer(newCapacity);
        }

        /// <summary>Riduce la capacità al numero effettivo di elementi</summary>
        public void Compact() => ResizeBuffer(Count);

        /// <summary>Ridimensiona l'array mantenendo gli elementi</summary>
        /// <param name="newSize">Nuova dimensione dell'array</param>
        /// <exception cref="ArgumentOutOfRangeException">Se newSize è negativo</exception>
        public void Resize(int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newSize));

            if (newSize > Capacity)
                ResizeBuffer(newSize);

            // Pulisci gli elementi rimossi
            if (newSize < Count && !typeof(T).IsValueType)
                Array.Clear(_buffer, newSize, Count - newSize);

            Count = newSize;
        }
        #endregion

        #region Manipolazioni avanzate
        /// <summary>Ruota gli elementi di un numero specificato di posizioni</summary>
        /// <param name="steps">Numero di posizioni (positivo: destra, negativo: sinistra)</param>
        /// <returns>Numero effettivo di passi eseguiti</returns>
        public int Rotate(int steps)
        {
            if (Count == 0) return 0;

            // Normalizza i passi
            steps %= Count;
            if (steps == 0) return 0;
            if (steps < 0) steps += Count;

            // Algoritmo di rotazione efficiente con tre inversioni
            Reverse(0, Count - 1);
            Reverse(0, steps - 1);
            Reverse(steps, Count - 1);

            return steps;
        }

        /// <summary>Inverte l'ordine di tutti gli elementi</summary>
        public void Flip() => Flip(0, Count - 1);

        /// <summary>Inverte l'ordine degli elementi in un intervallo</summary>
        /// <param name="start">Indice di inizio</param>
        /// <param name="end">Indice di fine (incluso)</param>
        /// <exception cref="ArgumentOutOfRangeException">Se gli indici sono non validi</exception>
        public void Flip(int start, int end)
        {
            if (Count <= 1) return;
            if (start < 0 || end >= Count)
                throw new ArgumentOutOfRangeException();
            if (start > end)
                throw new ArgumentException("start > end");

            Reverse(start, end);
        }
        #endregion

        #region Operazioni su insiemi
        /// <summary>Aggiunge un elemento solo se non è già presente</summary>
        /// <param name="item">Elemento da aggiungere</param>
        /// <param name="comparer">Comparatore per l'uguaglianza (opzionale)</param>
        /// <returns>True se aggiunto, false se già presente</returns>
        public bool AddIfNotContains(T item, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            if (Contains(item, comparer))
                return false;

            Add(item);
            return true;
        }

        /// <summary>Restituisce elementi univoci nell'array</summary>
        /// <param name="comparer">Comparatore per l'uguaglianza (opzionale)</param>
        /// <returns>Nuovo array con elementi distinti</returns>
        public DynamicArray<T> Distinct(IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            var result = new DynamicArray<T>();
            var seen = new HashSet<T>(comparer);

            for (int i = 0; i < Count; i++)
                if (seen.Add(_buffer[i]))
                    result.Add(_buffer[i]);

            return result;
        }
        #endregion

        #region Verifiche e confronti
        /// <summary>Verifica se tutti gli elementi soddisfano una condizione</summary>
        /// <param name="match">Predicato da verificare</param>
        /// <returns>True se tutti soddisfano la condizione</returns>
        /// <exception cref="ArgumentNullException">Se match è null</exception>
        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < Count; i++)
                if (!match(_buffer[i]))
                    return false;

            return true;
        }

        /// <summary>Confronta due array per verificare l'uguaglianza sequenziale</summary>
        /// <param name="other">Array da confrontare</param>
        /// <param name="comparer">Comparatore per l'uguaglianza (opzionale)</param>
        /// <returns>True se gli array sono uguali</returns>
        /// <exception cref="ArgumentNullException">Se other è null</exception>
        public bool SequenceEqual(DynamicArray<T> other, IEqualityComparer<T> comparer = null)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            comparer ??= EqualityComparer<T>.Default;

            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!comparer.Equals(_buffer[i], other[i]))
                    return false;

            return true;
        }
        #endregion

        #region Helper privati
        // Garantisce spazio sufficiente per nuovi elementi
        private void EnsureSpace(int needed)
        {
            if (Count + needed <= Capacity) return;

            int newCap = Math.Max(Capacity * GrowthFactor, Count + needed);
            ResizeBuffer(newCap);
        }

        // Riduce la capacità se sotto la soglia
        private void MaybeShrink()
        {
            if (Capacity <= 4) return;
            if (Count > Capacity * ShrinkThreshold) return;

            int newCap = Math.Max(4, Math.Max(Count, Capacity / GrowthFactor));
            if (newCap < Capacity)
                ResizeBuffer(newCap);
        }

        // Ridimensiona il buffer interno
        private void ResizeBuffer(int newCapacity)
        {
            var newBuf = new T[newCapacity];
            _buffer.AsSpan(0, Count).CopyTo(newBuf);
            _buffer = newBuf;
            Capacity = newCapacity;
        }

        // Inverte gli elementi in un intervallo
        private void Reverse(int start, int end)
        {
            while (start < end)
            {
                (_buffer[start], _buffer[end]) = (_buffer[end], _buffer[start]);
                start++;
                end--;
            }
        }

        // Helper per verificare i tipi numerici
        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Indicizzatore
        /// <summary>Accesso agli elementi per indice</summary>
        /// <param name="index">Indice dell'elemento</param>
        /// <exception cref="ArgumentOutOfRangeException">Se l'indice è fuori range</exception>
        public T this[int index]
        {
            get => index < Count ?
                _buffer[index] :
                throw new ArgumentOutOfRangeException(nameof(index));

            set
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _buffer[index] = value;
            }
        }
        #endregion
    }
    public static class DinamicArrayStringExtensions
    {
        /// <summary>
        /// Converte tutte le stringhe nell'array in lowercase
        /// </summary>
        public static void ToLower(this DynamicArray<string> array)
        {
            if (array == null) return;

            for (int i = 0; i < array.Count; i++)
            {
                array[i] = array[i]?.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Converte tutte le stringhe nell'array in uppercase
        /// </summary>
        public static void ToUpper(this DynamicArray<string> array)
        {
            if (array == null) return;

            for (int i = 0; i < array.Count; i++)
            {
                array[i] = array[i]?.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Converte le stringhe in un intervallo specifico
        /// </summary>
        /// <param name="startIndex">Indice di partenza</param>
        /// <param name="count">Numero di elementi da convertire</param>
        /// <param name="culture">Cultura da usare (opzionale)</param>
        /// <param name="toUpper">True per uppercase, false per lowercase</param>
        public static void ChangeCaseRange(
            this DynamicArray<string> array,
            int startIndex,
            int count,
            bool toUpper,
            System.Globalization.CultureInfo culture = null)
        {
            if (array == null) return;
            if (startIndex < 0 || startIndex >= array.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > array.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            culture ??= System.Globalization.CultureInfo.CurrentCulture;

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (toUpper)
                    array[i] = array[i]?.ToUpper(culture);
                else
                    array[i] = array[i]?.ToLower(culture);
            }
        }
    }

}
