using System;
using System.Diagnostics;

namespace NewFractalCompression.Code
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            System.Console.WriteLine("");

            CompressionClassTask.Compression(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\lena.bmp", "Black");
            sw.Stop();
            System.Console.WriteLine((sw.ElapsedMilliseconds / 1000.0).ToString());
            System.Console.WriteLine();
            CompressionClassTask.Decompression();
            System.Console.WriteLine();
            System.Console.WriteLine("Please, enter the key");
            Console.ReadKey();
        }
    }
}
