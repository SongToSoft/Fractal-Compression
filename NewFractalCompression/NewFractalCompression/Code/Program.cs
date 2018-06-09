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
            System.Diagnostics.Stopwatch Comsw = new Stopwatch();
            System.Diagnostics.Stopwatch Decsw = new Stopwatch();

            //Comsw.Start();
            //CompressionClass.Compression(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Coldplay.bmp", "Black");
            //Comsw.Stop();
            //System.Console.WriteLine((Comsw.Elapsed));

            //Decsw.Start();
            ////CompressionClass.ColorDecompression();
            //CompressionClass.BlackDecompression();
            //Decsw.Stop();
            //System.Console.WriteLine((Decsw.Elapsed));
            //System.Console.WriteLine();
            //CompressionClass.CheckRotate(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\lena gray.bmp");

            Comsw.Start();
            CompressionClass.NewCompression(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Sea.bmp", "Color");
            Comsw.Stop();
            System.Console.WriteLine((Comsw.Elapsed));

            Decsw.Start();
            CompressionClass.NewColorDecompression();
            Decsw.Stop();
            System.Console.WriteLine((Decsw.Elapsed));

            //FileInfo Fi = new FileInfo(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression");
            //Post.Compress(Fi);
            //FileInfo Fo= new FileInfo(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\nature.bmp.cmp");
            //Post.Decompress(Fo);
            System.Console.WriteLine("Please, press enter");
            Console.ReadKey();
        }
    }
}
