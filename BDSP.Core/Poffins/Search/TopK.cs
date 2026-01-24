using System;

namespace BDSP.Core.Poffins.Search
{
    /// <summary>
    /// Fixed-capacity top-K collector using simple replacement.
    /// </summary>
    public sealed class TopK<T>
    {
        private readonly T[] _items;
        private readonly int[] _scores;
        private int _count;
        private int _minIndex;

        public TopK(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _items = new T[capacity];
            _scores = new int[capacity];
            _count = 0;
            _minIndex = 0;
        }

        public int Count => _count;

        public ReadOnlySpan<T> Items => _items.AsSpan(0, _count);

        public void TryAdd(in T item, int score)
        {
            if (_count < _items.Length)
            {
                _items[_count] = item;
                _scores[_count] = score;
                if (score < _scores[_minIndex])
                {
                    _minIndex = _count;
                }
                _count++;
                return;
            }

            if (score <= _scores[_minIndex])
            {
                return;
            }

            _items[_minIndex] = item;
            _scores[_minIndex] = score;
            RecomputeMinIndex();
        }

        public void MergeFrom(TopK<T> other)
        {
            for (int i = 0; i < other._count; i++)
            {
                TryAdd(other._items[i], other._scores[i]);
            }
        }

        public T[] ToSortedArray(Comparison<T> comparison)
        {
            T[] result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _items[i];
            }

            Array.Sort(result, comparison);
            return result;
        }

        private void RecomputeMinIndex()
        {
            int minIndex = 0;
            int minScore = _scores[0];
            for (int i = 1; i < _count; i++)
            {
                int score = _scores[i];
                if (score < minScore)
                {
                    minScore = score;
                    minIndex = i;
                }
            }
            _minIndex = minIndex;
        }
    }
}
