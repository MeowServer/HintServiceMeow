﻿using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Utilities.Parser;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class RichTextParserPool
    {
        private static readonly ConcurrentQueue<RichTextParser> RichTextParserQueue = new();

        public static RichTextParser Rent()
        {
            if (RichTextParserQueue.TryDequeue(out RichTextParser rtp))
                return rtp;

            return new RichTextParser();
        }

        public static void Return(RichTextParser parser)
        {
            RichTextParserQueue.Enqueue(parser);
        }

        public static IReadOnlyList<LineInfo> ParseTextReturn(RichTextParser parser, string text, int size = 20, HintAlignment alignment = HintAlignment.Center)
        {
            IReadOnlyList<LineInfo> result = parser.ParseText(text, size, alignment);
            Return(parser);
            return result;
        }

        public static IReadOnlyList<LineInfo> ParseText(string text, int size = 20, HintAlignment alignment = HintAlignment.Center)
        {
            return ParseTextReturn(Rent(), text, size, alignment);
        }
    }
}
