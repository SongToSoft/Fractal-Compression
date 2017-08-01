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
        
        static double[,] Rotate(double[,] ImageBrightness, int block_size, Block Block)
        {
            double[,] NewImageBrightness = ImageBrightness;
            for (int i = Block.X; i < Block.X + block_size; ++i)
            {
                for (int j = Block.Y; j < Block.Y + block_size; ++j)
                {
                    ImageBrightness[Block.Y + block_size - j - 1, i]= NewImageBrightness[i, j];

                }
            }
            return NewImageBrightness;
        }
        static Color[,] RotateColor(Color[,] ImageColor, int block_size, Block Block)
        {
            Color[,] NewImageColor = ImageColor;
            for (int i = Block.X; i < Block.X + block_size; ++i)
            {
                for (int j = Block.Y; j < Block.Y + block_size; ++j)
                {
                    NewImageColor[Block.Y + block_size - j - 1, i] = ImageColor[i, j];
                }
            }
            return NewImageColor;
        }
        //static Color[,] RotateColor(Color[,] ImageColor, int block_size, Block Block)
        //{
        //    Color[,] NewImageColor = ImageColor;
        //    for (int i = Block.X; i < Block.X + block_size; ++i)
        //    {
        //        for (int j = Block.Y; j < Block.Y + block_size; ++j)
        //        {
        //            NewImageColor[j, i] = ImageColor[i, j];
        //        }
        //    }
        //    return NewImageColor;
        //}
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
            shift =  ((RangeValue)- (0.75 * DomainValue)) / (range_block_size * range_block_size);
            return shift;
        }
        //static double Shift(Bitmap Image, Block RangeBlock, Block DomainBlock, int range_block_size)
        //{
        //    double shift = 0;
        //    double RangeValue = 0;
        //    double DomainValue = 0;
        //    for (int i = 0; i < range_block_size; ++i)
        //    {
        //        for (int j = 0; j < range_block_size; ++j)
        //        {
        //            Color color = Image.GetPixel(RangeBlock.X + i, RangeBlock.Y + j);
        //            RangeValue += ((color.R + color.G + color.B) / (3));
        //            color = Image.GetPixel(DomainBlock.X + i * 2, DomainBlock.Y + j * 2);
        //            DomainValue += ((color.R + color.G + color.B) / (3));
        //        }
        //    }
        //    shift = (RangeValue % 255) - 0.75 * (DomainValue % 255);
        //    return shift;
        //}
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
                    DomainValue = DomainImageBrightness[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)];
                    distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                }
            }
            return distance;
        }
        //static double Distance(Bitmap RangeImage, Bitmap DomainImage, Block RangeBlock, Block DomainBlock, int range_block_size, double shift)
        //{
        //    double distance = 0;
        //    double RangeValue = 0;
        //    double DomainValue = 0;
        //    for (int i = 0; i < range_block_size; ++i)
        //    {
        //        for (int j = 0; j < range_block_size; ++j)
        //        {
        //            Color color = RangeImage.GetPixel(RangeBlock.X + i, RangeBlock.Y + j);
        //            int R = color.R;
        //            int G = color.G;
        //            int B = color.B;
        //            RangeValue = R + G + B;

        //            color = DomainImage.GetPixel(DomainBlock.X + i, DomainBlock.Y + j);
        //            R = color.R;
        //            G = color.G;
        //            B = color.B;
        //            DomainValue = R + G + B;

        //            distance += Math.Pow((RangeValue - 0.75 * DomainValue + shift), 2);
        //        }
        //    }
        //    return distance;
        //}
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
            //Block[,] DomainArray = new Block[domain_num, domain_num];
            //for (int i = 0; i < domain_num; ++i)
            //{
            //    for (int j = 0; j < domain_num; ++j)
            //    {
            //        DomainArray[i, j].X = i * domain_block_size;
            //        DomainArray[i, j].Y = j * domain_block_size;
            //    }
            //}
            domain_num = range_num - 1;
            Block[,] DomainArray = new Block[domain_num, domain_num];
            for (int i = 0; i < domain_num; ++i)
            {
                for (int j = 0; j < domain_num; ++j)
                {
                    DomainArray[i, j].X = i * range_block_size;
                    DomainArray[i, j].Y = j * range_block_size;
                }
            }
            //Алгоритм сжатия
            int count = 1;
            Coefficients[,] CompressCoeff = new Coefficients[range_num, range_num];
            StreamWriter sw = new StreamWriter(@"C: \Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Compression.txt");
            sw.Write(Image.Width);
            sw.Write(" ");
            sw.Write(Image.Height);
            sw.WriteLine();
            sw.WriteLine(range_block_size);
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
                            //Bitmap DomainImage = Image;
                            double[,] DomainImageBrightness = ImageBrightness;
                            for (int rotate = 0; rotate < 4; ++rotate)
                            {
                                DomainImageBrightness = Rotate(DomainImageBrightness, domain_block_size, DomainBlock);
                                //if (rotate == 1)
                                //    DomainImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                //if (rotate == 2)
                                //    DomainImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                //if (rotate == 3)
                                //    DomainImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                double shift = Shift(ImageBrightness, RangeBlock, DomainBlock, range_block_size);
                                //double shift = Shift(Image, RangeBlock, DomainBlock, range_block_size);
                                double distance = Distance(ImageBrightness, DomainImageBrightness, RangeBlock, DomainBlock, range_block_size, shift);
                                //double distance = Distance(Image, DomainImage, RangeBlock, DomainBlock, range_block_size, shift);
                                if (distance < current_distance)
                                {
                                    //current_x = DomainBlock.X;
                                    //current_y = DomainBlock.Y;
                                    current_x = k;
                                    current_y = l;
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
        }
        static void Decompression()
        {
            //Алгоритм декомпрессии
            StreamReader sr = new StreamReader(@"C: \Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Compression.txt");
            string srt = sr.ReadLine();
            string[] soul = srt.Split();
            int Image_width = Convert.ToInt32(soul[0]);
            int Image_height = Convert.ToInt32(soul[1]);
            Bitmap NewImage = new Bitmap(Image_width, Image_height);
            srt = sr.ReadLine();
            soul = srt.Split();
            int range_block_size = Convert.ToInt32(soul[0]);
            int range_num = NewImage.Width / range_block_size;
            int domain_block_size = range_block_size * 2;
           
            Coefficients[,] CompressCoeff = new Coefficients[range_num, range_num];
            srt = sr.ReadLine();
            for (int i = 0; i < range_num; ++i)
            {
                for (int j = 0; j < range_num; ++j)
                {
                    soul = srt.Split();
                    CompressCoeff[i, j].X = Convert.ToInt32(soul[0]);
                    CompressCoeff[i, j].Y = Convert.ToInt32(soul[1]);
                    CompressCoeff[i, j].rotate = Convert.ToInt32(soul[2]);
                    CompressCoeff[i, j].shift = Convert.ToDouble(soul[3]);
                    srt = sr.ReadLine();
                    printCoefficients(CompressCoeff[i, j]);
                }
            }
            sr.Close();
            //Создаём ранговые блоки
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
            int domain_num = range_num - 1;
            Block[,] DomainArray = new Block[domain_num, domain_num];
            for (int i = 0; i < domain_num; ++i)
            {
                for (int j = 0; j < domain_num; ++j)
                {
                    DomainArray[i, j].X = i * range_block_size;
                    DomainArray[i, j].Y = j * range_block_size;
                }
            }
            for (int it = 0; it < 10; ++it)
            {
                Color[,] NewImageColor = new Color[NewImage.Width, NewImage.Height];
                for (int i = 0; i < NewImage.Width; ++i)
                {
                    for (int j = 0; j < NewImage.Width; ++j)
                    {
                        NewImageColor[i, j] = NewImage.GetPixel(i, j);
                    }
                }
                Color[,] RotateNewImageColor = new Color[NewImage.Width, NewImage.Height];

                for (int i = 0; i < range_num; ++i)
                {
                    for (int j = 0; j < range_num; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficent = CompressCoeff[i, j];

                        Block DomainBlock = DomainArray[Current_coefficent.X, Current_coefficent.Y];
                        for (int rotate = 0; rotate < 4; ++rotate)
                        {
                            RotateNewImageColor = RotateColor(RotateNewImageColor, domain_block_size, DomainBlock);
                        }
                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                                //double q = 0.75 * RotateNewImageColor[DomainBlock.X + pix_y * 2, DomainBlock.Y + pix_x * 2].GetBrightness() + Current_coefficent.shift ;
                                //Color color = RotateNewImage.GetPixel(DomainBlock.X + pix_y * 2, DomainBlock.Y + pix_x * 2);
                                Color color = RotateNewImageColor[DomainBlock.X + (pix_x * 2), DomainBlock.Y + (pix_y * 2)];
                                int A = (int)(0.75 * color.A + (Current_coefficent.shift));
                                if (A < 0)
                                    A = 0;
                                if (A > 255)
                                    A = 255;
                                int R = (int)(0.75 * (double)color.R + (Current_coefficent.shift));
                                //int R = (int)(NewImage.GetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y).R + q);
                                if (R < 0)
                                    R = 0;
                                if (R > 255)
                                    R = 255;
                                //System.Console.Write(R);
                                int G = (int)(0.75 * (double)color.G + (Current_coefficent.shift));
                                //int G = (int)(NewImage.GetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y).G + q);
                                if (G < 0)
                                    G = 0;
                                if (G > 255)
                                    G = 255;
                                //System.Console.Write(" ");
                                //System.Console.Write(G);
                                int B = (int)(0.75 * (double)color.B + (Current_coefficent.shift));
                                //int B = (int)(NewImage.GetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y).B + q);
                                if (B < 0)
                                    B = 0;
                                if (B > 255)
                                    B = 255;
                                //System.Console.Write(" ");
                                //System.Console.Write(B);
                                //System.Console.WriteLine();
                                color = Color.FromArgb(A, R, G, B);
                                //color = Color.FromArgb(R, G, B);
                                NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, color);
                            }
                        }
                    }
                }
            }
            //sr.Close();
            NewImage.Save(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
        static void Main(string[] args)
        {
            Compression(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\test.bmp");
            Decompression();
            System.Console.WriteLine();
            System.Console.WriteLine("Please, enter the key");
            Console.ReadKey();
        }
    }
}
