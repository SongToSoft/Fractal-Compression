using System;
using NewFractalCompression;
using System.Diagnostics;

namespace NewFractalCompression.Code
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            //sw.Start();
            System.Console.WriteLine("");
            CompressionClass.Compression(@"C:\Users\Dima\Documents\Fractal\NewFractalCompression\NewFractalCompression\lena color.bmp", "");
            //sw.Stop();
            //System.Console.WriteLine((sw.ElapsedMilliseconds / 1000.0).ToString());
            CompressionClass.ByteDecompression();
            //CompressionClass.Decompression();
            System.Console.WriteLine();
            System.Console.WriteLine("Please, press enter");
            Console.ReadKey();
        }
    }
}
