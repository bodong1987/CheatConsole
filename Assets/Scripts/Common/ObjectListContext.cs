/**
 * @brief Fix IL2CPP code size
 * @email bodong@tencent.com
*/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Common
{
    public abstract class ListViewBase
    {
        protected List<object> Context = null;

        public int Count
        {
            get
            {
                return Context.Count;
            }
        }

        public void Clear()
        {
            Context.Clear();
        }

        public void RemoveAt(int index)
        {
            Context.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            Context.RemoveRange(index, count);
        }

        public void Reverse()
        {
            Context.Reverse();
        }

        public void Sort()
        {
            Context.Sort();
        }
    }

    public class ListView<T> :
        ListViewBase,
        IEnumerable<T>
    {
        public ListView()
        {
            Context = new List<object>();

#if UNITY_EDITOR
            DebugHelper.Assert(
                typeof(T).IsClass || typeof(T).IsInterface,
                "Performance Warning! ListView Should Not Contain Value Type!!!!"
                );
#endif
        }

        public ListView(IEnumerable<T> collection)
        {
            Context = new List<object>();

            var Iter = collection.GetEnumerator();

            while (Iter.MoveNext())
            {
                Context.Add(Iter.Current);
            }
        }

        public ListView(int capacity)
        {
            Context = new List<object>(capacity);
        }


        public T this[int index]
        {
            get
            {
                return (T)Context[index];
            }
            set
            {
                Context[index] = value;
            }
        }

        public void Add(T item)
        {
            Context.Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection != null)
            {
                var Iter = collection.GetEnumerator();

                while (Iter.MoveNext())
                {
                    Context.Add(Iter.Current);
                }
            }
        }


        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return Context.BinarySearch(item, new ComparerConverter(comparer));
        }

        private struct ComparerConverter : IComparer<object>
        {
            IComparer<T> ComparerRef;

            public ComparerConverter(IComparer<T> comparer)
            {
                ComparerRef = comparer;
            }

            public int Compare(object x, object y)
            {
                return ComparerRef.Compare((T)x, (T)y);
            }
        }


        public bool Contains(T item)
        {
            return Context.Contains(item);
        }

        public int IndexOf(T item)
        {
            return Context.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Context.Insert(index, item);
        }

        public int LastIndexOf(T item)
        {
            return Context.LastIndexOf(item);
        }

        public bool Remove(T item)
        {
            return Context.Remove(item);
        }

        public void Sort(IComparer<T> comparer)
        {
            Context.Sort(new ComparerConverter(comparer));
        }

        public void Sort(Comparison<T> comparison)
        {
            Context.Sort(new ComparisonConverter(comparison));
        }

        private struct ComparisonConverter : IComparer<object>
        {
            Comparison<T> ComparerRef;

            public ComparisonConverter(Comparison<T> comparer)
            {
                ComparerRef = comparer;
            }

            public int Compare(object x, object y)
            {
                return ComparerRef((T)x, (T)y);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(Context);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public struct Enumerator : IEnumerator<T>
        {
            List<object> Reference;
            List<object>.Enumerator Iter;

            public Enumerator(List<object> InReference)
            {
                Reference = InReference;
                Iter = Reference.GetEnumerator();
            }

            public T Current
            {
                get
                {
                    return (T)Iter.Current;
                }
            }

            object IEnumerator.Current { get { throw new NotImplementedException(); } }

            public void Reset()
            {
                Iter = Reference.GetEnumerator();
            }

            public void Dispose()
            {
                Iter.Dispose();
                Reference = null;
            }

            public bool MoveNext()
            {
                return Iter.MoveNext();
            }
        }
    }

    public class ListLinqView<T> : ListView<T>
    {
        public ListLinqView() :
            base()
        {
        }

        public T[] ToArray()
        {
            T[] Results = new T[Count];

            if (Results.Length < Context.Count)
            {
                throw new ArgumentException("Input array has not enough size.");
            }

            for (int i = 0; i < Context.Count; ++i)
            {
                Results[i] = (T)Context[i];
            }

            return Results;
        }
    }

    public class DictionaryView<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected Dictionary<TKey, object> Context = null;

        public DictionaryView()
        {
            Context = new Dictionary<TKey, object>();

#if UNITY_EDITOR
            DebugHelper.Assert(
                typeof(TValue).IsClass || typeof(TValue).IsInterface,
                "Performance Warning! DictionaryView.ValueType Should Not Contain Value Type!!!!"
                );
#endif
        }

        public int Count
        {
            get { return Context.Count; }
        }

        public TValue this[TKey key]
        {
            get
            {
                var obj = Context[key];
                return obj != null ? (TValue)obj : default(TValue);
            }
            set
            {
                Context[key] = value;
            }
        }

        public Dictionary<TKey, object>.KeyCollection Keys
        {
            get
            {
                return Context.Keys;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Context.Add(key, value);
        }

        public void Clear()
        {
            Context.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return Context.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return Context.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            object ResultObject = null;

            bool bResult = Context.TryGetValue(key, out ResultObject);

            value = ResultObject != null ? (TValue)ResultObject : default(TValue);

            return bResult;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(Context);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Dictionary<TKey, object>.Enumerator GetContextEnumerator()
        {
            return Context.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            Dictionary<TKey, object> Reference;
            Dictionary<TKey, object>.Enumerator Iter;

            public Enumerator(Dictionary<TKey, object> InReference)
            {
                Reference = InReference;
                Iter = Reference.GetEnumerator();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(
                        Iter.Current.Key,
                        Iter.Current.Value != null ? (TValue)Iter.Current.Value : default(TValue)
                        );
                }
            }

            object IEnumerator.Current { get { throw new NotImplementedException(); } }

            public void Reset()
            {
                Iter = Reference.GetEnumerator();
            }

            public void Dispose()
            {
                Iter.Dispose();
                Reference = null;
            }

            public bool MoveNext()
            {
                return Iter.MoveNext();
            }
        }
    }

    /**
     * 这货比楼上的更暴力，强行要求Key也是Object
     * 适用于Key不是内置类型的Dictionary替换
    */
    public class DictionaryObjectView<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected Dictionary<object, object> Context = null;

        public DictionaryObjectView()
        {
            Context = new Dictionary<object, object>();

#if UNITY_EDITOR
            DebugHelper.Assert(
                (typeof(TValue).IsClass || typeof(TValue).IsInterface) &&
                (typeof(TKey).IsClass || typeof(TKey).IsInterface),
                "Performance Warning! DictionaryObjectView.KeyType or ValueType Should Not Contain Value Type!!!!"
                );
#endif
        }

        public int Count
        {
            get { return Context.Count; }
        }

        public TValue this[TKey key]
        {
            get
            {
                var obj = Context[key];
                return obj != null ? (TValue)obj : default(TValue);
            }
            set
            {
                Context[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Context.Add(key, value);
        }

        public void Clear()
        {
            Context.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return Context.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return Context.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            object ResultObject = null;

            bool bResult = Context.TryGetValue(key, out ResultObject);

            value = ResultObject != null ? (TValue)ResultObject : default(TValue);

            return bResult;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(Context);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            Dictionary<object, object> Reference;
            Dictionary<object, object>.Enumerator Iter;

            public Enumerator(Dictionary<object, object> InReference)
            {
                Reference = InReference;
                Iter = Reference.GetEnumerator();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(
                        Iter.Current.Key != null ? (TKey)Iter.Current.Key : default(TKey),
                        Iter.Current.Value != null ? (TValue)Iter.Current.Value : default(TValue)
                        );
                }
            }

            object IEnumerator.Current { get { throw new NotImplementedException(); } }

            public void Reset()
            {
                Iter = Reference.GetEnumerator();
            }

            public void Dispose()
            {
                Iter.Dispose();
                Reference = null;
            }

            public bool MoveNext()
            {
                return Iter.MoveNext();
            }
        }
    }

    public abstract class ListValueViewBase
    {
        protected int _size;
        protected int _version;

        protected const int DefaultCapacity = 4;

        public int Count
        {
            get { return _size; }
        }
    }

    public class ListValueView<T> :
        ListValueViewBase,
        IEnumerable<T>
    {
        T[] _items;

        static readonly T[] EmptyArray = new T[0];

        public ListValueView()
        {
            _items = EmptyArray;
        }

        public ListValueView(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            _items = new T[capacity];
        }

        void GrowIfNeeded(int newCount)
        {
            int minimumSize = _size + newCount;
            if (minimumSize > _items.Length)
                Capacity = Math.Max(Math.Max(Capacity * 2, DefaultCapacity), minimumSize);
        }

        public void Add(T item)
        {
            // If we check to see if we need to grow before trying to grow
            // we can speed things up by 25%
            if (_size == _items.Length)
            {
                GrowIfNeeded(1);
            }

            _items[_size++] = item;

            _version++;
        }

        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if ((uint)value < (uint)_size)
                    throw new ArgumentOutOfRangeException();

                Array.Resize(ref _items, value);
            }
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _items.Length);
            _size = 0;
            _version++;
        }

        void Shift(int start, int delta)
        {
            if (delta < 0)
                start -= delta;

            if (start < _size)
                Array.Copy(_items, start, _items, start + delta, _size - start);

            _size += delta;

            if (delta < 0)
                Array.Clear(_items, _size, -delta);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || (uint)index >= (uint)_size)
                throw new ArgumentOutOfRangeException("index");
            Shift(index, -1);
            Array.Clear(_items, _size, 1);
            _version++;
        }

        void CheckRange(int idx, int count)
        {
            if (idx < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if ((uint)idx + (uint)count > (uint)_size)
                throw new ArgumentException("index and count exceed length of list");
        }

        public void RemoveRange(int index, int count)
        {
            CheckRange(index, count);
            if (count > 0)
            {
                Shift(index, -count);
                Array.Clear(_items, _size, count);
                _version++;
            }
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(_items, item, 0, _size);
        }

        public int IndexOf(T item, int index)
        {
            // CheckIndex(index);
            if (index < 0 || (uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException("index");

            return Array.IndexOf<T>(_items, item, index, _size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if ((uint)index + (uint)count > (uint)_size)
                throw new ArgumentOutOfRangeException("index and count exceed length of list");

            return Array.IndexOf<T>(_items, item, index, count);
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                    throw new ArgumentOutOfRangeException("index");
                return _items[index];
            }
            set
            {
                if (index < 0 || (uint)index > (uint)_size)
                    throw new ArgumentOutOfRangeException("index");

                if ((uint)index == (uint)_size)
                    throw new ArgumentOutOfRangeException("index");
                _items[index] = value;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            ListValueView<T> l;
            int next;
            int ver;

            T current;

            internal Enumerator(ListValueView<T> l)
                : this()
            {
                this.l = l;
                ver = l._version;
            }

            public void Dispose()
            {
                l = null;
            }

            void VerifyState()
            {
                if (l == null)
                    throw new ObjectDisposedException(GetType().FullName);
                if (ver != l._version)
                    throw new InvalidOperationException(
                        "Collection was modified; enumeration operation may not execute.");
            }

            public bool MoveNext()
            {
                VerifyState();

                if (next < 0)
                    return false;

                if (next < l._size)
                {
                    current = l._items[next++];
                    return true;
                }

                next = -1;
                return false;
            }

            public T Current
            {
                get { return current; }
            }

            void IEnumerator.Reset()
            {
                VerifyState();
                next = 0;
            }

            object IEnumerator.Current
            {
                get
                {
                    VerifyState();
                    if (next <= 0)
                        throw new InvalidOperationException();
                    return current;
                }
            }
        }
    }

    public struct ReadonlyContext<T>
    {
        private List<T> Reference;

        public ReadonlyContext(List<T> InReference)
        {
            Reference = InReference;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(Reference);
        }

        public int Count
        {
            get
            {
                return Reference.Count;
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private List<T> Reference;
            private List<T>.Enumerator IterReference;

            public Enumerator(List<T> InRefernce)
            {
                Reference = InRefernce;
                IterReference = InRefernce.GetEnumerator();
            }

            public T Current
            {
                get
                {
                    return IterReference.Current;
                }
            }

            object IEnumerator.Current { get { throw new NotImplementedException(); } }

            public void Reset()
            {
                IterReference = Reference.GetEnumerator();
            }

            public void Dispose()
            {
                IterReference.Dispose();
                Reference = null;
            }

            public bool MoveNext()
            {
                return IterReference.MoveNext();
            }
        }
    }
}