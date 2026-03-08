using BenchmarkDotNet.Attributes;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Benchmarks.Benchmarks
{
    [Config(typeof(ScpslConfig))]
    [MemoryDiagnoser]
    public class HintCollectionBenchmark
    {
        private HintCollection _collection = null!;
        private DummyHint _dummyHintToAdd = null!;
        private DummyHint _dummyHintToRemove = null!;

        [Params(5, 20)]
        public int AssemblyCount { get; set; }

        [Params(100, 1000)]
        public int HintsPerAssembly { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _collection = new HintCollection();
            _dummyHintToAdd = new DummyHint { benchmarkId = -1 };

            for (int i = 0; i < AssemblyCount; i++)
            {
                var assemblyName = $"Assembly{i}";
                for (int j = 0; j < HintsPerAssembly; j++)
                {
                    var hint = new DummyHint { benchmarkId = j };
                    _collection.AddHint(assemblyName, hint);

                    // Get a hint to remove
                    if (i == 0 && j == 50)
                    {
                        _dummyHintToRemove = hint;
                    }
                }
            }

            // Generate cache
            _ = _collection.AllGroups;
            _ = _collection.AllHints;
        }

        [Benchmark]
        public AbstractHint[][] GetAllGroups_CacheHit()
        {
            return _collection.AllGroups;
        }

        [Benchmark]
        public AbstractHint[] GetAllHints_CacheHit()
        {
            return _collection.AllHints;
        }

        [Benchmark]
        public AbstractHint[] GetHints_ByAssembly()
        {
            return _collection.GetHints("Assembly0");
        }

        [Benchmark]
        public AbstractHint[] GetHints_ByPredicate()
        {
            return _collection.GetHints("Assembly0", h => ((DummyHint)h).benchmarkId % 2 == 0);
        }

        [Benchmark]
        public bool AddAndRemove_CausesCacheReset()
        {
            _collection.AddHint("Assembly0", _dummyHintToAdd);
            return _collection.RemoveHint("Assembly0", _dummyHintToAdd);
        }

        [Benchmark]
        public AbstractHint[] CacheRebuild_AllHints()
        {
            _collection.AddHint("Assembly0", _dummyHintToAdd);
            var result = _collection.AllHints;
            _collection.RemoveHint("Assembly0", _dummyHintToAdd); // 恢复原状
            return result;
        }

        private class DummyHint : AbstractHint
        {
            public int benchmarkId { get; set; }
        }
    }
}
