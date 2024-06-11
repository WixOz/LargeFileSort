using System;
using System.Collections.Generic;

namespace LargeFileSorter.Comparer
{
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
}