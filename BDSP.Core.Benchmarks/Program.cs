using System;
using System.IO;
using BenchmarkDotNet.Running;

namespace BDSP.Core.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Ensure the current directory contains the benchmark .csproj to avoid BDN discovery warnings.
            string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            Directory.SetCurrentDirectory(projectDir);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
