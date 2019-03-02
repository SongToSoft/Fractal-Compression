using System;
using NewFractalCompression;
using System.Diagnostics;
using System.IO;

namespace NewFractalCompression.Code
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch comSw = new Stopwatch();
            System.Diagnostics.Stopwatch decSw = new Stopwatch();

            System.Console.WriteLine("Classic compression: ");
            comSw.Start();
            BlockCompression.Compression(@"C:\Users\dbogdano\Documents\GitHub\Fractal-Compression\NewFractalCompression\NewFractalCompression\Sea New.bmp", "Color");
            comSw.Stop();
            System.Console.WriteLine((comSw.Elapsed));

            decSw.Start();
            BlockCompression.ColorDecompression();
            decSw.Stop();
            System.Console.WriteLine((decSw.Elapsed));


            System.Console.WriteLine("Quad Tree compression: ");
            comSw.Start();
            QuadTreeCompression.Compression(@"C:\Users\dbogdano\Documents\GitHub\Fractal-Compression\NewFractalCompression\NewFractalCompression\Sea New.bmp", "Color");
            comSw.Stop();
            System.Console.WriteLine((comSw.Elapsed));

            decSw.Start();
            QuadTreeCompression.FakeDecompression();
            decSw.Stop();
            System.Console.WriteLine((decSw.Elapsed));

            System.Console.WriteLine("Please, press enter");
            Console.ReadKey();
        }
    }
}
