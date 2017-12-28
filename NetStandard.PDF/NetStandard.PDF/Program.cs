using System;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Syncfusion.Pdf.Parsing;
using PdfDocument = Syncfusion.Pdf.PdfDocument;

namespace NetStandard.PDF
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SplitPDF>();
        }

        [MemoryDiagnoser]
        [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 0, targetCount: 1, invocationCount: 10)]
        public class SplitPDF
        {
            private readonly string _rootFolder;
            private readonly string _resultsSyncfusionFolder;
            private readonly string _resultsiTextFolder;

            public SplitPDF()
            {
                var rootFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
                _rootFolder = rootFolder.FullName;
                _resultsSyncfusionFolder = Path.Combine(rootFolder.FullName, "Results-SyncFusion");
                _resultsiTextFolder = Path.Combine(rootFolder.FullName, "Results-iText");
                if (!Directory.Exists(_resultsSyncfusionFolder))
                {
                    Directory.CreateDirectory(_resultsSyncfusionFolder);
                }
                if (!Directory.Exists(_resultsiTextFolder))
                {
                    Directory.CreateDirectory(_resultsiTextFolder);
                }
            }

            //[Benchmark]
            //public void Syncfusion_Split_24Mb_1000pages_by_2_pages()
            //{
            //    RunSynfusionBenchmark("k1_1000pages_24mb.pdf", 2);
            //}
            
            //[Benchmark]
            //public void Syncfusion_Split_1Mb_1000pages_by_2_pages()
            //{
            //    RunSynfusionBenchmark("k1_1000pages_1mb.pdf", 2);
            //}

            //[Benchmark]
            //public void iText_Split_24Mb_1000pages_by_2_pages()
            //{
            //    RuniTextBenchmark("k1_1000pages_24mb.pdf", 2);
            //}

            //[Benchmark]
            //public void iText_Split_1Mb_1000pages_by_2_pages()
            //{
            //    RuniTextBenchmark("k1_1000pages_1mb.pdf", 2);
            //}

            [Benchmark]
            public void Syncfusion_Split_125Mb_gt_7500pages_by_10_pages()
            {
                RunSynfusionBenchmark("sample_125Mb_gt_7500pages.pdf", 10);
            }

            [Benchmark]
            public void iText_Split_125Mb_gt_7500pages_10_pages()
            {
                RuniTextBenchmark("sample_125Mb_gt_7500pages.pdf", 10);
            }



            public void RuniTextBenchmark(string fileToSplit, int splitByPagesNumber, int? pagesCountToProcess = null)
            {
                var srcFile = Path.Combine(_rootFolder, fileToSplit);
                var file = new FileInfo(srcFile);
                var name = file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.Ordinal));

                using (var reader = new PdfReader(srcFile))
                {

                    var pagesCount = pagesCountToProcess ?? reader.NumberOfPages;
                    var iteration = 1;
                    var currentPageIndex = 0;

                    //Split the pages into separate documents.
                    while (currentPageIndex < pagesCount)
                    {
                        var endPage = currentPageIndex + splitByPagesNumber < pagesCount
                            ? currentPageIndex + splitByPagesNumber
                            : pagesCount;

                        var document = new Document();
                        var copy = new PdfCopy(document, File.Create(Path.Combine(_resultsiTextFolder, $"splitted_{name}_{iteration}.pdf")));

                        document.Open();

                        while (currentPageIndex++ < endPage)
                        {
                            copy.AddPage(copy.GetImportedPage(reader, currentPageIndex));
                        }

                        document.Close();
                        iteration++;
                    }
                }
            }

            public void RunSynfusionBenchmark(string fileToSplit, int splitByPagesNumber, int? pagesCountToProcess = null)
            {
                var srcFile = Path.Combine(_rootFolder, fileToSplit);
                var file = new FileInfo(srcFile);
                var name = file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.Ordinal));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var srcStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read);
                var docToSplit = new PdfLoadedDocument(srcStream) { EnableMemoryOptimization = true };

                var pagesCount = pagesCountToProcess ?? docToSplit.PageCount;

                var iteration = 1;
                var currentPageIndex = 0;

                //Split the pages into separate documents.
                while (currentPageIndex < pagesCount)
                {
                    var newDocument = new PdfDocument { EnableMemoryOptimization = true };

                    int endPage = currentPageIndex + splitByPagesNumber < pagesCount
                        ? currentPageIndex + splitByPagesNumber - 1
                        : pagesCount - 1;

                    newDocument.ImportPageRange(docToSplit, currentPageIndex, endPage);

                    using (var fileStream = File.Create(Path.Combine(_resultsSyncfusionFolder, $"splitted_{name}_{iteration}.pdf")))
                    {
                        newDocument.Save(fileStream);
                        newDocument.Close();
                    }

                    currentPageIndex += splitByPagesNumber;
                    iteration++;
                }

                srcStream.Dispose();
                docToSplit.Close();
            }
        }
    }
}
