using System;
using System.Collections.Generic;
using System.Globalization;

namespace ImageSorter.Helpers
{
    public class CompareHelper
    {
        public static int CompareNatural(string strA, string strB)
        {
            return CompareNatural(strA, strB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
        }

        public static int CompareNatural(string strA, string strB, CultureInfo culture, CompareOptions options)
        {
            var cmp = culture.CompareInfo;
            var iA = 0;
            var iB = 0;
            var softResult = 0;
            var softResultWeight = 0;
            while (iA < strA.Length && iB < strB.Length)
            {
                var isDigitA = char.IsDigit(strA[iA]);
                var isDigitB = char.IsDigit(strB[iB]);
                if (isDigitA != isDigitB)
                {
                    return cmp.Compare(strA, iA, strB, iB, options);
                }
                else if (!isDigitA && !isDigitB)
                {
                    var jA = iA + 1;
                    var jB = iB + 1;
                    while (jA < strA.Length && !char.IsDigit(strA[jA])) jA++;
                    while (jB < strB.Length && !char.IsDigit(strB[jB])) jB++;
                    var cmpResult = cmp.Compare(strA, iA, jA - iA, strB, iB, jB - iB, options);
                    if (cmpResult != 0)
                    {
                        var sectionA = strA.Substring(iA, jA - iA);
                        var sectionB = strB.Substring(iB, jB - iB);
                        if (cmp.Compare(sectionA + "1", sectionB + "2", options) ==
                            cmp.Compare(sectionA + "2", sectionB + "1", options))
                        {
                            return cmp.Compare(strA, iA, strB, iB, options);
                        }
                        else if (softResultWeight < 1)
                        {
                            softResult = cmpResult;
                            softResultWeight = 1;
                        }
                    }
                    iA = jA;
                    iB = jB;
                }
                else
                {
                    char zeroA = (char)(strA[iA] - (int)char.GetNumericValue(strA[iA]));
                    char zeroB = (char)(strB[iB] - (int)char.GetNumericValue(strB[iB]));
                    var jA = iA;
                    var jB = iB;
                    while (jA < strA.Length && strA[jA] == zeroA) jA++;
                    while (jB < strB.Length && strB[jB] == zeroB) jB++;
                    var resultIfSameLength = 0;
                    do
                    {
                        isDigitA = jA < strA.Length && char.IsDigit(strA[jA]);
                        isDigitB = jB < strB.Length && char.IsDigit(strB[jB]);
                        var numA = isDigitA ? (int)char.GetNumericValue(strA[jA]) : 0;
                        var numB = isDigitB ? (int)char.GetNumericValue(strB[jB]) : 0;
                        if (isDigitA && (char)(strA[jA] - numA) != zeroA) isDigitA = false;
                        if (isDigitB && (char)(strB[jB] - numB) != zeroB) isDigitB = false;
                        if (isDigitA && isDigitB)
                        {
                            if (numA != numB && resultIfSameLength == 0)
                            {
                                resultIfSameLength = numA < numB ? -1 : 1;
                            }
                            jA++;
                            jB++;
                        }
                    }
                    while (isDigitA && isDigitB);
                    if (isDigitA != isDigitB)
                    {
                        return isDigitA ? 1 : -1;
                    }
                    else if (resultIfSameLength != 0)
                    {
                        return resultIfSameLength;
                    }
                    var lA = jA - iA;
                    var lB = jB - iB;
                    if (lA != lB)
                    {
                        return lA > lB ? -1 : 1;
                    }
                    else if (zeroA != zeroB && softResultWeight < 2)
                    {
                        softResult = cmp.Compare(strA, iA, 1, strB, iB, 1, options);
                        softResultWeight = 2;
                    }
                    iA = jA;
                    iB = jB;
                }
            }
            if (iA < strA.Length || iB < strB.Length)
            {
                return iA < strA.Length ? 1 : -1;
            }
            else if (softResult != 0)
            {
                return softResult;
            }
            return 0;
        }

        public class CustomComparer<T> : IComparer<T>
        {
            private Comparison<T> _comparison;

            public CustomComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }
        }
    }
}