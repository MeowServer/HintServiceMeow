using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace HintServiceMeow.Core.Utilities
{
    internal static class TextTool
    {
        private static readonly Dictionary<Tuple<string, int>, float> TextWidthCache = new Dictionary<Tuple<string, int>, float>();
        private static readonly Dictionary<Tuple<string, int>, float> TextHeightCache = new Dictionary<Tuple<string, int>, float>();
        private static readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();

        public static float GetTextWidth(string text, int size = 20)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float width;
            var tuple = Tuple.Create(text, size);

            CacheLock.EnterReadLock();
            try
            {
                if (TextWidthCache.TryGetValue(tuple, out width))
                    return width;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }

            string noTagText = Regex.Replace(text, "<.*?>", string.Empty);
            string longestText = noTagText.Split('\n').OrderByDescending(x => x.Length).First();

            int halfSizeCharacters = longestText.Count(x => char.IsLetterOrDigit(x) || char.IsSymbol(x));
            int otherCharacters = longestText.Length - halfSizeCharacters;
            width = halfSizeCharacters * size / 1f + otherCharacters * size;

            CacheLock.EnterWriteLock();
            try
            {
                TextWidthCache[tuple] = width;
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }

            return width;
        }

        public static float GetTextHeight(string text, int size = 20, float extraLineHeight = 0)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float height;
            var tuple = Tuple.Create(text, size);

            CacheLock.EnterReadLock();
            try
            {
                if (TextHeightCache.TryGetValue(tuple, out height))
                    return height;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }

            if (string.IsNullOrEmpty(text))
                return 0;

            height = text.Split('\n').Length * (size + extraLineHeight);

            CacheLock.EnterWriteLock();
            try
            {
                TextHeightCache[tuple] = height;
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }

            return height;
        }
    }

}
