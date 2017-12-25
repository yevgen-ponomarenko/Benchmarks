using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace NetStandard.PDF
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Syncfusion>();
        }

        public class Syncfusion
        {
            public Syncfusion()
            {
            }

            [Benchmark]
            public void Split_50Mb_1000pages_by_2_pages()
            {
            }

            [Benchmark]
            public void Split_50Mb_1000pages_by_10_pages()
            {
            }
        }
    }
}
