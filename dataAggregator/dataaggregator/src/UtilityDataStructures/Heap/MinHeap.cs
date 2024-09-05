using System;
using System.Collections.Generic;
using System.Numerics;

public class MinHeap<T> where T : IComparable<T>, INumber<T>
{
    private List<(T Value, string Key)> heap = new List<(T, string)>();
    private Dictionary<string, int> elementIndexMap = new Dictionary<string, int>();
    private int k;

    private static readonly object heapLock = new object();

    public MinHeap(int k)
    {
        this.k = k;
    }

    private int Parent(int index) => (index - 1) / 2;
    private int LeftChild(int index) => 2 * index + 1;
    private int RightChild(int index) => 2 * index + 2;

    private void Swap(int index1, int index2)
    {
        var temp = heap[index1];
        heap[index1] = heap[index2];
        heap[index2] = temp;

        elementIndexMap[heap[index1].Key] = index1;
        elementIndexMap[heap[index2].Key] = index2;
    }

    private string Insert(string key, T value)
    {
        string removedKey = "";
        if (elementIndexMap.ContainsKey(key))
        {
            throw new InvalidOperationException("Element already in the Heap, should use update.");
        }

        heap.Add((value, key));
        int current = heap.Count - 1;
        elementIndexMap[key] = current;

        while (current != 0 && heap[Parent(current)].Value.CompareTo(heap[current].Value) > 0)
        {
            Swap(current, Parent(current));
            current = Parent(current);
        }

        if (heap.Count > k)
        {
            removedKey =  RemoveMin();
        }
        return removedKey;
    }
    public bool ContainsKey(string key)
    {
        return elementIndexMap.ContainsKey(key);
    }
    public string Update(string key, T incrementValue)
    {
        lock(heapLock)
        {
            string removedKey = "";
            if (!elementIndexMap.ContainsKey(key))
            {
                removedKey = Insert(key, incrementValue);
            }

            int index = elementIndexMap[key];
            T oldValue = heap[index].Value;
            var newValue = oldValue + incrementValue;
            heap[index] = (newValue, key);

            if (newValue.CompareTo(oldValue) < 0)
            {
                while (index != 0 && heap[Parent(index)].Value.CompareTo(heap[index].Value) > 0)
                {
                    Swap(index, Parent(index));
                    index = Parent(index);
                }
            }
            else
            {
                Heapify(index);
            }
            return removedKey;
        }
    }

    private string RemoveMin()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("Heap is empty");
        }

        var min = heap[0];
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        elementIndexMap.Remove(min.Key);

        if (heap.Count > 0)
        {
            elementIndexMap[heap[0].Key] = 0;
            Heapify(0);
        }
        return min.Key;
    }

    private void Heapify(int index)
    {
        int smallest = index;
        int left = LeftChild(index);
        int right = RightChild(index);

        if (left < heap.Count && heap[left].Value.CompareTo(heap[smallest].Value) < 0)
        {
            smallest = left;
        }

        if (right < heap.Count && heap[right].Value.CompareTo(heap[smallest].Value) < 0)
        {
            smallest = right;
        }

        if (smallest != index)
        {
            Swap(index, smallest);
            Heapify(smallest);
        }
    }

    public List<string> GetSortedList()
    {
        var sortedList = new List<string>();

        List<(T Value, string Key)>? heapCopy;
        lock (heapLock)
        {
            heapCopy = heap.ToList(); // Create a copy of the heap
        }

        var tempHeap = new MinHeap<T>(k);
        foreach (var item in heapCopy)
        {
            tempHeap.Insert(item.Key, item.Value);
        }

        while (tempHeap.heap.Count > 0)
        {
            sortedList.Add(tempHeap.RemoveMin());
        }


        return sortedList;
    }

    public void PrintHeap()
    {
        foreach (var item in heap)
        {
            Console.Write($"({item.Key}, {item.Value}) ");
        }
        Console.WriteLine();
    }
}