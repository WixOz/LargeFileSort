using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LargeFileSorter
{
    class ExternalSort
    {
        public static void Go()
        {
            string inputFile = "LargeFile_10Gb_100624_065011.txt";
            string tempDirectory = "temp_chunks";
            Directory.CreateDirectory(tempDirectory);

            int chunkSize = 500000; // Количество строк в каждом чанке
            int chunkCount = 0;

            var baseComparer = StringComparer.Ordinal;
            var customComparer = new CustomComparer(baseComparer);

            // Шаг 1: Разделение на части и их сортировка с использованием параллелизма
            List<Task> sortTasks = new List<Task>();

            using (StreamReader sr = new StreamReader(inputFile))
            {
                List<string> lines = new List<string>();
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);

                    if (lines.Count >= chunkSize)
                    {
                        int currentChunkIndex = chunkCount++;
                        List<string> linesToSort = new List<string>(lines);
                        sortTasks.Add(Task.Run(() =>
                        {
                            linesToSort.Sort(customComparer);
                            File.WriteAllLines(Path.Combine(tempDirectory, $"chunk_{currentChunkIndex}.txt"), linesToSort);
                        }));
                        lines.Clear();
                    }
                }

                if (lines.Count > 0)
                {
                    int currentChunkIndex = chunkCount++;
                    List<string> linesToSort = new List<string>(lines);
                    sortTasks.Add(Task.Run(() =>
                    {
                        linesToSort.Sort(customComparer);
                        File.WriteAllLines(Path.Combine(tempDirectory, $"chunk_{currentChunkIndex}.txt"), linesToSort);
                    }));
                }
            }

            Task.WaitAll(sortTasks.ToArray());

            // Шаг 2: Слияние отсортированных частей
            MergeChunks(tempDirectory, chunkCount, "sorted_file.txt", customComparer);

            // Удаление временных файлов
            Directory.Delete(tempDirectory, true);
        }

        static void MergeChunks(string tempDirectory, int chunkCount, string outputFile, IComparer<string> comparer)
        {
            List<StreamReader> readers = new List<StreamReader>();
            for (int i = 0; i < chunkCount; i++)
            {
                readers.Add(new StreamReader(Path.Combine(tempDirectory, $"chunk_{i}.txt")));
            }

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                // Priority queue for merging chunks
                var priorityQueue = new PriorityQueue<(string line, int readerIndex), string>(comparer);
                int uniqueId = 0;

                // Инициализация очереди
                Parallel.For(0, readers.Count, i =>
                {
                    if (!readers[i].EndOfStream)
                    {
                        var line = readers[i].ReadLine();
                        priorityQueue.Enqueue((line, i), line);
                    }
                });

                while (priorityQueue.Count > 0)
                {
                    var smallest = priorityQueue.Dequeue();
                    sw.WriteLine(smallest.line);

                    int readerIndex = smallest.readerIndex;
                    if (!readers[readerIndex].EndOfStream)
                    {
                        var line = readers[readerIndex].ReadLine();
                        priorityQueue.Enqueue((line, readerIndex), line);
                    }
                }
            }

            // Закрытие всех StreamReader
            Parallel.ForEach(readers, reader => reader.Close());
        }
    }

    internal class CustomComparer : IComparer<string>
    {
        private const string Separator = ". ";
        private readonly IComparer<string> _baseComparer;

        public CustomComparer(IComparer<string> baseComparer)
        {
            _baseComparer = baseComparer;
        }

        public int Compare(string? x, string? y)
        {
            if (x == null || y == null)
            {
                return _baseComparer.Compare(x, y);
            }

            int baseComparison = _baseComparer.Compare(x, y);
            if (baseComparison == 0)
            {
                return 0;
            }

            int xSepIndex = x.IndexOf(Separator, StringComparison.Ordinal);
            int ySepIndex = y.IndexOf(Separator, StringComparison.Ordinal);

            if (xSepIndex == -1 || ySepIndex == -1)
            {
                return _baseComparer.Compare(x, y);
            }

            string xStr = x[(xSepIndex + Separator.Length)..];
            string yStr = y[(ySepIndex + Separator.Length)..];

            int strComparison = _baseComparer.Compare(xStr, yStr);
            if (strComparison != 0)
            {
                return strComparison;
            }

            int xNum = int.Parse(x[..xSepIndex]);
            int yNum = int.Parse(y[..ySepIndex]);

            return xNum.CompareTo(yNum);
        }
    }

    public class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _heap;
        private readonly IComparer<TPriority> _comparer;

        public PriorityQueue(IComparer<TPriority> comparer)
        {
            _heap = new List<(TElement, TPriority)>();
            _comparer = comparer;
        }

        public int Count => _heap.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            lock (_heap)
            {
                _heap.Add((element, priority));
                HeapifyUp(_heap.Count - 1);
            }
        }

        public TElement Dequeue()
        {
            lock (_heap)
            {
                if (_heap.Count == 0)
                {
                    throw new InvalidOperationException("The priority queue is empty");
                }

                TElement result = _heap[0].Element;
                _heap[0] = _heap[_heap.Count - 1];
                _heap.RemoveAt(_heap.Count - 1);
                HeapifyDown(0);
                return result;
            }
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_comparer.Compare(_heap[index].Priority, _heap[parentIndex].Priority) >= 0)
                {
                    break;
                }

                var temp = _heap[index];
                _heap[index] = _heap[parentIndex];
                _heap[parentIndex] = temp;
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (index < lastIndex)
            {
                int leftChildIndex = index * 2 + 1;
                int rightChildIndex = index * 2 + 2;

                if (leftChildIndex > lastIndex)
                {
                    break;
                }

                int smallestChildIndex = (rightChildIndex > lastIndex || _comparer.Compare(_heap[leftChildIndex].Priority, _heap[rightChildIndex].Priority) < 0) ? leftChildIndex : rightChildIndex;

                if (_comparer.Compare(_heap[index].Priority, _heap[smallestChildIndex].Priority) <= 0)
                {
                    break;
                }

                var temp = _heap[index];
                _heap[index] = _heap[smallestChildIndex];
                _heap[smallestChildIndex] = temp;
                index = smallestChildIndex;
            }
        }
    }
}
