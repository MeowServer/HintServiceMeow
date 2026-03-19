using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace HintServiceMeow.Benchmarks
{
    public class ScpslConfig : ManualConfig
    {
        public ScpslConfig()
        {
            // Run again using the Mono runtime (requires "mono" to be configured in your Windows environment variables)
            // If the environment variable is not set, you can manually specify the path using the line below:
            string unityMonoPath = Environment.GetEnvironmentVariable("UNITY_MONO_PATH");
            if (unityMonoPath is null)
                throw new InvalidOperationException("Please set environment variable UNITY_MONO_PATH to the path of your Unity editor's mono.exe");

            var unityMonoJob = Job.Default
                .WithRuntime(new MonoRuntime("Unity_6000_Mono", unityMonoPath))
                .WithId("Unity 6000.0.43f1");

            AddJob(unityMonoJob);
        }
    }
}
