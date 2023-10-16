namespace PathFindingAlgorithms.PriorityQueue
{
    public class PriorityQueue<T>
    {
        private List<T> heap;
        public Dictionary<T, int> itemIndices;
        private IComparer<T> comparer;

        public int Count => heap.Count;
        public int IndicesCount => itemIndices.Count;

        public PriorityQueue(IComparer<T> comparer)
        {
            heap = new List<T>();
            itemIndices = new Dictionary<T, int>();
            this.comparer = comparer;
        }

        public void Enqueue(T item)
        {
            heap.Add(item);
            itemIndices[item] = heap.Count - 1;
            HeapifyUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Priority queue is empty.");

            T firstItem = heap[0];
            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            itemIndices[heap[0]] = 0;
            heap.RemoveAt(lastIndex);
            itemIndices.Remove(firstItem);

            if (heap.Count > 1)
                HeapifyDown(0);

            return firstItem;
        }

        public bool Contains(T item)
        {
            return itemIndices.ContainsKey(item);
        }

        public T Peek()
        {
            return heap[0];
        }

        public void Remove(T item)
        {
            if (!itemIndices.ContainsKey(item))
                throw new ArgumentException("Item does not exist in the priority queue.");

            int index = itemIndices[item];
            itemIndices.Remove(item);
            if (index == heap.Count - 1)
            {
                heap.RemoveAt(index);
            }
            else
            {
                T lastItem = heap[heap.Count - 1];
                heap[index] = lastItem;
                heap.RemoveAt(heap.Count - 1);
                itemIndices[lastItem] = index;
                UpdatePriority(lastItem);
            }
        }

        private void RemoveAt(int index)
        {
            T lastItem = heap[heap.Count - 1];
            heap[index] = lastItem;
            itemIndices[lastItem] = index;
            heap.RemoveAt(heap.Count - 1);
            itemIndices.Remove(lastItem);

            if (index < heap.Count)
            {
                HeapifyUp(index);
                HeapifyDown(index);
            }
        }


        private void HeapifyUp(int index)
        {
            int parentIndex = (index - 1) / 2;

            while (index > 0 && comparer.Compare(heap[index], heap[parentIndex]) < 0)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }

            itemIndices[heap[index]] = index;
        }

        private void HeapifyDown(int index)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex < heap.Count && comparer.Compare(heap[leftChildIndex], heap[smallestIndex]) < 0)
                smallestIndex = leftChildIndex;

            if (rightChildIndex < heap.Count && comparer.Compare(heap[rightChildIndex], heap[smallestIndex]) < 0)
                smallestIndex = rightChildIndex;

            if (smallestIndex != index)
            {
                Swap(index, smallestIndex);
                HeapifyDown(smallestIndex);
            }
        }


        private void Swap(int index1, int index2)
        {
            T temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;

            itemIndices[heap[index1]] = index1;
            itemIndices[heap[index2]] = index2;
        }

        public void UpdatePriority(T item)
        {
            if (!itemIndices.ContainsKey(item))
                throw new ArgumentException("Item does not exist in the priority queue.");

            int index = itemIndices[item];
            HeapifyUp(index);
            HeapifyDown(index);
        }
    }
}
