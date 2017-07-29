using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression
{
    class Program
    {
        public struct Block
        {
            public int X;
            public int Y;
        }
        public struct Coefficients
        {
            public int X;
            public int Y;
            public int rotate;
            public double shift;
        }
        static void printCoefficients(Coefficients Fac)
        {
            System.Console.Write(Fac.X);
            System.Console.Write(" ");
            System.Console.Write(Fac.Y);
            System.Console.Write(" ");
            System.Console.Write(Fac.rotate);
            System.Console.Write(" ");
            System.Console.Write(Fac.shift);
            System.Console.WriteLine();
        }
        static double[,] Rotate(double[,] ImageBrightness, int Image_width, int count)
        {
            double[,] NewImageBrightness = ImageBrightness;
            for (int it = 0; it < count; ++it)
            {
                for (int i = 0; i < Image_width; ++i)
                {
                    for (int j = 0; j < Image_width; ++j)
                    {
                        NewImageBrightness[i, j] = ImageBrightness[j, Image_width - i - 1];
                    }
                }
            }
            return NewImageBrightness;
        }
        static Color[,] RotateColor(Color[,] ImageColor, int Image_width, int count)
        {
            Color[,] NewImageColor = ImageColor;
            for (int it = 0; it < count; ++it)
            {
                for (int i = 0; i < Image_width; ++i)
                {
                    for (int j = 0; j < Image_width; ++j)
                    {
                        NewImageColor[i, j] = ImageColor[j, Image_width - i - 1];
                    }
                }
            }
            return NewImageColor;
        }
        static double Shift(double[,] ImageBrightness, Block RangeBlock, Block DomainBlock, int range_block_size)
        {
            double shift = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    RangeValue += ImageBrightness[RangeBlock.X + i, RangeBlock.Y + j];
                    DomainValue += ImageBrightness[DomainBlock.X + i * 2, DomainBlock.Y + j * 2];
                }
            }
            shift = (RangeValue / range_block_size) - 0.75 * (DomainValue / range_block_size);
            return shift;
        }
        static double Distance(double[,] RangeImageBrightness, double[,] DomainImageBrightness, Block RangeBlock, Block DomainBlock, int range_block_size, double shift)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    RangeValue = RangeImageBrightness[RangeBlock.X + i, RangeBlock.Y + j];
                    DomainValue = DomainImageBrightness[DomainBlock.X + i * 2, DomainBlock.Y + j * 2];
                    distance += Math.Pow((RangeValue - 0.75 * DomainValue + shift), 2);
                }
            }
            return distance;
        }
        static void Compression(string filename)
        {
            Bitmap Image = new Bitmap(filename);
            double[,] ImageBrightness = new double[Image.Width, Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    ImageBrightness[i, j] = Image.GetPixel(i, j).GetBrightness();
                }
            }
            //Создаём ранговые блоки
            int range_num = 10;
            int range_block_size = Image.Width / range_num;
            Block[,] RangeArray = new Block[range_num, range_num];
            for (int i = 0; i < range_num; ++i)
            {
                for (int j = 0; j < range_num; ++j)
                {
                    RangeArray[i, j].X = i * range_block_size;
                    RangeArray[i, j].Y = j * range_block_size;
                }
            }
            //Создаем доменные блоки
            int domain_num = range_num / 2;
            int domain_block_size = range_block_size * 2;
            Block[,] DomainArray = new Block[domain_num, domain_num];
            for (int i = 0; i < domain_num; ++i)
            {
                for (int j = 0; j < domain_num; ++j)
                {
                    DomainArray[i, j].X = i * domain_block_size;
                    DomainArray[i, j].Y = j * domain_block_size;
                }
            }
            //Алгоритм сжатия
            int count = 1;
            StreamWriter sw = new StreamWriter(@"C: \Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Compression.txt");
            sw.Write(Image.Width);
            sw.Write(" ");
            sw.Write(Image.Height);
            sw.WriteLine();
            sw.WriteLine(range_block_size);
            Coefficients[,] CompressCoeff = new Coefficients[range_num, range_num];
            for (int i = 0; i < range_num; ++i)
            {
                for (int j = 0; j < range_num; ++j)
                {
                    Block RangeBlock = RangeArray[i, j];
                    int current_x = 0;
                    int current_y = 0;
                    double current_distance = Double.MaxValue;
                    double current_shift = 0;
                    int current_rotate = 0;
                    for (int k = 0; k < domain_num; ++k)
                    {
                        for (int l = 0; l < domain_num; ++l)
                        {
                            Block DomainBlock = DomainArray[k, l];
                            double[,] DomainImageBrightness = ImageBrightness;
                            for (int rotate = 0; rotate < 4; ++rotate)
                            {
                                DomainImageBrightness = Rotate(ImageBrightness, Image.Width, rotate);
                                double shift = Shift(ImageBrightness, RangeBlock, DomainBlock, range_block_size);
                                double distance = Distance(ImageBrightness, DomainImageBrightness, RangeBlock, DomainBlock, range_block_size, range_block_size);
                                if (distance < current_distance)
                                {
                                    current_x = DomainBlock.X;
                                    current_y = DomainBlock.Y;
                                    current_shift = shift;
                                    current_distance = distance;
                                    current_rotate = rotate;
                                }
                            }
                        }
                    }
                    System.Console.Write(count);
                    System.Console.Write('/');
                    System.Console.Write((range_num * range_num));
                    System.Console.WriteLine();
                    ++count;
                    CompressCoeff[i, j].X = current_x;
                    CompressCoeff[i, j].Y = current_y;
                    CompressCoeff[i, j].rotate = current_rotate;
                    CompressCoeff[i, j].shift = current_shift;
                    printCoefficients(CompressCoeff[i, j]);
                    sw.WriteLine(CompressCoeff[i, j].X + " " + CompressCoeff[i, j].Y + " " + CompressCoeff[i, j].rotate + " " + CompressCoeff[i, j].shift);
                }
            }
            sw.Close();
            //Алгоритм декомпрессии

            Bitmap NewImage = new Bitmap(Image.Width, Image.Height);
            for (int it = 0; it < 16; ++it)
            {
                Bitmap RotateNewImage = NewImage;
                Color[,] RotateNewImageColor = new Color[Image.Width, Image.Height];
                for (int i = 0; i < Image.Width; ++i)
                {
                    for (int j = 0; j < Image.Height; ++j)
                    {
                        RotateNewImageColor[i, j] = RotateNewImage.GetPixel(i, j);
                    }
                }
                for (int i = 0; i < range_num; ++i)
                {
                    for (int j = 0; j < range_num; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficent = CompressCoeff[i, j];
                        RotateNewImageColor = RotateColor(RotateNewImageColor, Image.Width, Current_coefficent.rotate);
                        for (int pix = 0; pix < range_block_size; ++pix)
                        {
                            Color color = RotateNewImage.GetPixel(Current_coefficent.X + pix, Current_coefficent.Y + pix);

                            int A = (int)(0.75 * color.A + Current_coefficent.shift);
                            if (A < 0)
                                A = 0;
                            if (A > 255)
                                A = 255;

                            int B = (int)(0.75 * color.B + Current_coefficent.shift);
                            if (B < 0)
                                B = 0;
                            if (B > 255)
                                B = 255;

                            int G = (int)(0.75 * color.G + Current_coefficent.shift);
                            if (G < 0)
                                G = 0;
                            if (G > 255)
                                G = 255;

                            color = Color.FromArgb(A,B,G);
                            NewImage.SetPixel(RangeBlock.X + pix, RangeBlock.Y + pix, color);
                        }
                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

        }
        static void Main(string[] args)
        {
            Compression(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\test.bmp");
            System.Console.WriteLine();
            System.Console.WriteLine("Please, enter the key");
            Console.ReadKey();
        }
    }
}
