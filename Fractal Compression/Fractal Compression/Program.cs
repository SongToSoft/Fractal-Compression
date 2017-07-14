using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractal_Compression
{
    class Program
    {
        static public int range_size;
        static public int domain_size;
        public struct Block
        {
            public int X;
            public int Y;
        }
        public struct Coeff
        {
            public int k;
            public int l;
            public int p;
            public double q;
        }
        static double Shift(Bitmap Image, int range_x, int range_y, int domain_x, int domain_y)
        {
            double shift = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            Bitmap RangeImage = Image;
            Bitmap DomainImage = Image;
            for (int i = 0; i < range_size; ++i)
            {
                for (int j = 0;  j < range_size; ++j)
                {
                    RangeValue = RangeValue + (GetValue(RangeImage.GetPixel(range_x + j, range_y + i)));
                    DomainValue = DomainValue + (GetValue(DomainImage.GetPixel(domain_x + j * 2, domain_y + i * 2)));
                }
            }
            //shift = (RangeValue) - 0.75 * (DomainValue); 
            //Будем брать среднее арифметическое
            shift = ((RangeValue - 0.75 * (DomainValue))) / (range_size);
            return shift;
        }
        static double Distance(Bitmap Image, int range_x, int range_y, int domain_x, int domain_y, double shift, int rotate)
        {
            double Dist = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            Bitmap RangeImage = Image;
            Bitmap DomainImage = Image;
            if (rotate == 1)
                DomainImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            if (rotate == 2)
                DomainImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            if (rotate == 3)
                DomainImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            for (int i = 0; i < range_size; ++i)
            {
                for (int j = 0; j < range_size; ++j)
                {
                    RangeValue = GetValue(RangeImage.GetPixel(range_x + j, range_y + i));
                    DomainValue = GetValue(DomainImage.GetPixel(domain_x + j * 2, domain_y + i * 2));
                    Dist = Dist + Math.Pow((RangeValue - 0.75 * DomainValue + shift), 2);
                }
            }
            return Dist;
        }
        static double GetValue(Color color)
        {
            double Value = 0;
            Value = color.A + color.B + color.G;
            return Value;
        }
        static void Compresson(string filename)
        {
            Bitmap Image = new Bitmap(filename);
            //Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            //Image.Save(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\Fractal Compression\Fractal Compression\New Lenna Gray.png");
            range_size = 100;
            domain_size = range_size * 2;
            int range_width = Image.Width / range_size;
            int range_heigth = Image.Height / range_size;
            int domain_width = Image.Width / domain_size;
            int domain_heigth = Image.Height / domain_size;
            //Формируем массив ранговых блоков
            Block[,] RangeArray = new Block[range_width, range_heigth];
            for (int i = 0; i < range_heigth; ++i)
            {
                for (int j = 0; j < range_width; ++j)
                {
                    RangeArray[i, j].X = j * range_size;
                    RangeArray[i, j].Y = i * range_size;
                }
            }
            //Формируем массив доменных блоков
            Block[,] DomainArray = new Block[domain_width, domain_heigth];
            for (int i = 0; i < domain_heigth; ++i)
            {
                for (int j = 0; j < domain_width; ++j)
                {
                    DomainArray[i, j].X = j * domain_size;
                    DomainArray[i, j].Y = i * domain_size;
                }
            }
            double current_shift = 0;
            double current_distance = 0;
            int count = 0; // Счетчик
            //Алгоритм сжатия
            Coeff[,] CoeffArray = new Coeff[range_width, range_heigth];
            for (int i = 0; i < range_heigth; ++i)
            {
                for (int j = 0; j < range_width; ++j)
                {
                    Block RangeBlock = RangeArray[i, j];
                    int k0 = 1;
                    int l0 = 1;
                    int p0 = 0;
                    double q0 = 0;
                    double best_distance = Double.MaxValue;
                    for (int k = 0; k < domain_width; ++k)
                    {
                        for (int l = 0; l < domain_heigth; ++l)
                        {
                            for (int p = 0; p < 4; ++p)
                            {
                                Block DomainBlock = DomainArray[k, l];
                                current_shift = Shift(Image, RangeBlock.X, RangeBlock.Y, DomainBlock.X, DomainBlock.Y);
                                current_distance = Distance(Image, RangeBlock.X, RangeBlock.Y, DomainBlock.X, DomainBlock.Y, current_shift, p);
                                if (current_distance < best_distance)
                                {
                                    best_distance = current_distance;
                                    k0 = k;
                                    l0 = l;
                                    p0 = p;
                                    q0 = current_shift;
                                }
                                if (p == 4)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    CoeffArray[i, j].k = k0;
                    CoeffArray[i, j].l = l0;
                    CoeffArray[i, j].p = p0;
                    CoeffArray[i, j].q = q0;
                    ++count;
                    System.Console.WriteLine(count + "/" + (range_width * range_heigth));
                    System.Console.WriteLine(CoeffArray[i, j].k);
                    System.Console.WriteLine(CoeffArray[i, j].l);
                    System.Console.WriteLine(CoeffArray[i, j].p);
                    System.Console.WriteLine(CoeffArray[i, j].q);
                    System.Console.WriteLine("!!!!!!!!!");
                }
            }
            //Алгоритм декодированияы
            Bitmap NewImage = new Bitmap(Image.Width, Image.Height);
            Bitmap NewDomainImage = NewImage;
            for (int it = 0; it < 16; ++it)
            {
                for(int i = 0; i < range_heigth; ++i)
                {
                    for (int j = 0; j < range_width; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        int k0 = CoeffArray[i, j].k;
                        int l0 = CoeffArray[i, j].l;
                        int p0 = CoeffArray[i, j].p;
                        if (p0 == 1)
                            NewDomainImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        if (p0 == 2)
                            NewDomainImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        if (p0 == 3)
                            NewDomainImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                        double q0 = CoeffArray[i, j].q;
                        Color color = new Color();
                        for (int k = 0; k < range_size; ++k)
                        {
                            for (int l = 0; l < range_size; ++l)
                            {
                                int R = (int)(0.75 * NewDomainImage.GetPixel(l + j * k0, k + i * l0).A + q0);
                                int G = (int)(0.75 * NewDomainImage.GetPixel(l + j * k0, k + i * l0).B + q0);
                                int B = (int)(0.75 * NewDomainImage.GetPixel(l + j * k0, k + i * l0).G + q0);
                                if (R > 255)
                                    R = 255;
                                if (R < 0)
                                    R = 0;
                                if (G > 255)
                                    G = 255;
                                if (G < 0)
                                    G = 0;
                                if (B > 255)
                                    B = 255;
                                if (B < 0)
                                    B = 0;
                                color = Color.FromArgb(R, G, B);
                                System.Console.WriteLine(l + j * RangeBlock.X);
                                System.Console.WriteLine(k + i * RangeBlock.Y);
                                NewImage.SetPixel(l + j * RangeBlock.X, k + i * RangeBlock.Y, color);
                            }

                        }
                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\Fractal Compression\Fractal Compression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
        static void Main(string[] args)
        {
            Compresson(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\Fractal Compression\Fractal Compression\test.bmp");
        }
    }
}
