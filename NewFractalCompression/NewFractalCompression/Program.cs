using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            public int SumR;
            public int SumG;
            public int SumB;
            //Коэффиценты для классификации
            public double Px;
            public double Py;
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
        //Создание блока с предварительным вычислением коэффицентов
        static Block CreateBlock(int X, int Y, Color[,] ImageColor, int range_block_size, bool domain, double[,] Brightness)
        {
            Block Block = new Block();
            Block.X = X;
            Block.Y = Y;
            Block.SumR = 0;
            Block.SumG = 0;
            Block.SumB = 0;
            Block.Px = 0;
            Block.Py = 0;
            double BlockBrightness = 0;
            if (domain)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i * 2,Block.Y + j * 2].R;
                        Block.SumG += ImageColor[Block.X + i * 2, Block.Y + j * 2].G;
                        Block.SumB += ImageColor[Block.X + i * 2, Block.Y + j * 2].B;
                        //Считаем общую яркость для блока
                        BlockBrightness += Brightness[Block.X + i * 2, Block.Y + j * 2];
                        //Считаем координаты центра масс блока
                        Block.Px += i * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                    }
                }
                Block.Px = Block.Px / BlockBrightness;
                Block.Py = Block.Py / BlockBrightness;
            }
            else
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i, Block.Y + j].R;
                        Block.SumG += ImageColor[Block.X + i, Block.Y + j].G;
                        Block.SumB += ImageColor[Block.X + i, Block.Y + j].B;
                        BlockBrightness += Brightness[Block.X + i, Block.Y + j];
                        Block.Px += i * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                    }
                }
                Block.Px = Block.Px / BlockBrightness;
                Block.Py = Block.Py / BlockBrightness;
            }
            return Block;
        }
        //Определение угла между центрами масс двух блоков
        static double Angle(Block RangeBlock, Block DomainBlock)
        {
            double angle = 0;
            double x1 = RangeBlock.X - 0;
            double x2 = DomainBlock.X - 0;
            double y1 = RangeBlock.Y - 0;
            double y2 = DomainBlock.Y - 0;
            double d1 = Math.Sqrt(x1 * x1 + y1 * y1);
            double d2 = Math.Sqrt(x2 * x2 + y2 * y2);
            angle = Math.Acos((x1 * x2 + y1 * y2) / (d1 * d2));
            return angle * 57.2958;
        }
        //static Bitmap RotateColor(Bitmap Image, int block_size, Block Block)
        //{
        //    Bitmap NewImage = Image;
        //    Rectangle Rectangle = new Rectangle(Block.X, Block.Y, block_size, block_size);
        //    System.Drawing.Imaging.PixelFormat format = Image.PixelFormat;
        //    Bitmap NewImageRotate = Image.Clone(Rectangle, format);
        //    NewImageRotate.RotateFlip(RotateFlipType.Rotate90FlipNone);
        //    for (int i = 0; i < block_size; ++i)
        //    {
        //        for (int j = 0; j < block_size; ++j)
        //        {
        //            Color color = NewImageRotate.GetPixel(i, j);
        //            NewImage.SetPixel(Block.X + i, Block.Y + j, color);
        //        }
        //    }
        //    return NewImage;
        //}
        //Поворот на 90 градусов блока во всём изображении
        static Color[,] RotateColor(Color[,] ImageColor, int block_size, Block Block)
        {
            Color[,] NewImageColor = ImageColor;
            Color[,] BlockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    BlockImageColor[i, j] = NewImageColor[Block.X + i, Block.Y + j];
                }
            }
            //Bitmap NewImage = new Bitmap(block_size,block_size);
            //for (int i = 0; i < block_size; ++i)
            //{
            //    for (int j = 0; j < block_size; ++j)
            //    {
            //        NewImage.SetPixel(i, j, BlockImageColor[i, j]);
            //    }
            //}
            //NewImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            //for (int i = 0; i < block_size; ++i)
            //{
            //    for (int j = 0; j < block_size; ++j)
            //    {
            //        NewImageColor[Block.X + i, Block.Y + j] = NewImage.GetPixel(i, j);
            //        //NewImageColor[Block.X + i, Block.Y + j] = BlockImageColor[i, j];
            //    }
            //}
            //for (int i = 0; i < block_size; i++)
            //{
            //    for (int j = 0; j < block_size; j++)
            //    {
            //        tmp_BlockImageColor[j, block_size - i - 1] = BlockImageColor[i, j];
            //        //tmp_BlockImageColor[i, j] = BlockImageColor[j, i];
            //    }
            //}
            for (int i = 0; i < block_size; i++)
            {
                for (int j = i; j < block_size - i - 1; j++)
                {
                    Color tmp = BlockImageColor[i, j];
                    BlockImageColor[i, j] = BlockImageColor[block_size - j - 1, i];
                    BlockImageColor[block_size - j - 1, i] = BlockImageColor[block_size - i - 1, block_size - j - 1];
                    BlockImageColor[block_size - i - 1, block_size - j - 1] = BlockImageColor[block_size - j - 1, i];
                    BlockImageColor[block_size - j - 1, i] = tmp;
                }
            }
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    //NewImageColor[Block.X + i, Block.Y + j] = tmp_BlockImageColor[i, j];
                    NewImageColor[Block.X + i, Block.Y + j] = BlockImageColor[i, j];
                }
            }
            return NewImageColor;
        }
        static double Shift(Block RangeBlock, Block DomainBlock, int range_block_size, int flag)
        {
            double shift = 0;
            if (flag == 1)
                shift = ((RangeBlock.SumR) - (0.75 * DomainBlock.SumR)) / (range_block_size * range_block_size);
            if (flag == 2)
                shift = ((RangeBlock.SumG) - (0.75 * DomainBlock.SumG)) / (range_block_size * range_block_size);
            if (flag == 3)
                shift = ((RangeBlock.SumB) - (0.75 * DomainBlock.SumB)) / (range_block_size * range_block_size);
            return shift;
        }
        static double Distance(Color[,] RangeImageColor, Color[,] DomainImageColor, Block RangeBlock, Block DomainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            if (flag == 1)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].R;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].R;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            if (flag == 2)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].G;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].G;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            if (flag == 3)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].B;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].B;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            return distance;
        }
       
        static void Compression(string filename)
        {
            Bitmap Image = new Bitmap(filename);
            Color[,] ImageColor = new Color[Image.Width, Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    ImageColor[i, j] = Image.GetPixel(i, j);
                }
            }
            double[,] BrightnessImage = new double[Image.Width, Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    BrightnessImage[i, j] = Image.GetPixel(i, j).GetBrightness();
                }
            }
            //Основной параметр, отвечающий за размеры ранговых блоков
            int range_block_size = 8;
            //Создаём ранговые блоки
            int range_num_width = Image.Width / range_block_size;
            int range_num_height = Image.Height / range_block_size;
            Block[,] RangeArray = new Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    RangeArray[i, j] = CreateBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, false, BrightnessImage);
                }
            }
            //Создаем доменные блоки
            int domain_num_width = range_num_width - 1;
            int domain_num_height = range_num_height - 1;
            int domain_block_size = range_block_size * 2;
            Block[,] DomainArray = new Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    DomainArray[i, j] = CreateBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, true, BrightnessImage);
                }
            }
            //Алгоритм сжатия
            int count = 1;
            //Общеее число преобразований
            int block_all_num = range_num_width * range_num_height * 3;
            Coefficients[,] CompressCoeff = new Coefficients[range_num_width, range_num_height];
            StreamWriter sw = new StreamWriter(@"C: \Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Compression.txt");
            sw.Write(Image.Width);
            sw.Write(" ");
            sw.Write(Image.Height);
            sw.WriteLine();
            sw.WriteLine(range_block_size);
            for (int flag = 1; flag < 4; ++flag)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        int current_x = 0;
                        int current_y = 0;
                        double current_distance = Double.MaxValue;
                        double current_shift = 0;
                        int current_rotate = 0;
                        for (int k = 0; k < domain_num_width; ++k)
                        {
                            for (int l = 0; l < domain_num_height; ++l)
                            {
                                Block DomainBlock = DomainArray[k, l];
                                //Bitmap DomainImage = Image;
                                Color[,] DomainImageColor = ImageColor;
                                //System.Console.WriteLine(Angle(RangeBlock, DomainBlock));
                                //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
                                if (Angle(RangeBlock, DomainBlock) <= 10) 
                                {
                                    for (int rotate = 0; rotate < 4; ++rotate)
                                    {
                                        //DomainImage = RotateColor(DomainImage, domain_block_size, DomainBlock);
                                        //if (rotate == 1)
                                        //    DomainImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        //if (rotate == 2)
                                        //    DomainImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                        //if (rotate == 3)
                                        //    DomainImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainBlock);
                                        double shift = Shift(RangeBlock, DomainBlock, range_block_size, flag);
                                        //double shift = Shift(Image, RangeBlock, DomainBlock, range_block_size);
                                        double distance = Distance(ImageColor, DomainImageColor, RangeBlock, DomainBlock, range_block_size, shift, flag);
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
                        }
                        //System.Console.Write(count);
                        //System.Console.Write('/');
                        //System.Console.Write((range_num_width * range_num_height));
                        double proc = ((100 * count) / block_all_num);
                        System.Console.Write(proc);
                        System.Console.Write(" %");
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
                //count = 1;
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
            int range_num_width = NewImage.Width / range_block_size;
            int range_num_height = NewImage.Height / range_block_size;
           
            Coefficients[,] CompressCoeffR = new Coefficients[range_num_width, range_num_height];
            srt = sr.ReadLine();
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    //srt = sr.ReadLine();
                    soul = srt.Split();
                    CompressCoeffR[i, j].X = Convert.ToInt32(soul[0]);
                    CompressCoeffR[i, j].Y = Convert.ToInt32(soul[1]);
                    CompressCoeffR[i, j].rotate = Convert.ToInt32(soul[2]);
                    CompressCoeffR[i, j].shift = Convert.ToDouble(soul[3]);
                    srt = sr.ReadLine();
                    //printCoefficients(CompressCoeff[i, j]);
                }
            }
            Coefficients[,] CompressCoeffG = new Coefficients[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    //srt = sr.ReadLine();
                    soul = srt.Split();
                    CompressCoeffG[i, j].X = Convert.ToInt32(soul[0]);
                    CompressCoeffG[i, j].Y = Convert.ToInt32(soul[1]);
                    CompressCoeffG[i, j].rotate = Convert.ToInt32(soul[2]);
                    CompressCoeffG[i, j].shift = Convert.ToDouble(soul[3]);
                    srt = sr.ReadLine();
                    //printCoefficients(CompressCoeff[i, j]);
                }
            }
            Coefficients[,] CompressCoeffB = new Coefficients[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    //srt = sr.ReadLine();
                    soul = srt.Split();
                    CompressCoeffB[i, j].X = Convert.ToInt32(soul[0]);
                    CompressCoeffB[i, j].Y = Convert.ToInt32(soul[1]);
                    CompressCoeffB[i, j].rotate = Convert.ToInt32(soul[2]);
                    CompressCoeffB[i, j].shift = Convert.ToDouble(soul[3]);
                    srt = sr.ReadLine();
                    //printCoefficients(CompressCoeff[i, j]);
                }
            }
            sr.Close();
            //Создаём ранговые блоки
            Block[,] RangeArray = new Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    RangeArray[i, j].X = i * range_block_size;
                    RangeArray[i, j].Y = j * range_block_size;
                }
            }
            //Создаем доменные блоки
            int domain_num_width = range_num_width - 1;
            int domain_num_height = range_num_height - 1;
            int domain_block_size = range_block_size * 2;
            Block[,] DomainArray = new Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    DomainArray[i, j].X = i * range_block_size;
                    DomainArray[i, j].Y = j * range_block_size;
                }
            }
            for (int it = 0; it < 16; ++it)
            {
                Color[,] NewImageColor = new Color[NewImage.Width, NewImage.Height];
                for (int i = 0; i < NewImage.Width; ++i)
                {
                    for (int j = 0; j < NewImage.Width; ++j)
                    {
                        NewImageColor[i, j] = NewImage.GetPixel(i, j);
                    }
                }
                //Color[,] RotateNewImageColorR = new Color[NewImage.Width, NewImage.Height];
                //Color[,] RotateNewImageColorG = new Color[NewImage.Width, NewImage.Height];
                //Color[,] RotateNewImageColorB = new Color[NewImage.Width, NewImage.Height];
                Color[,] RotateNewImageR = NewImageColor;
                Color[,] RotateNewImageG = NewImageColor;
                Color[,] RotateNewImageB = NewImageColor;

                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficentR = CompressCoeffR[i, j];
                        Coefficients Current_coefficentG = CompressCoeffG[i, j];
                        Coefficients Current_coefficentB = CompressCoeffB[i, j];

                        Block DomainBlockR = DomainArray[Current_coefficentR.X, Current_coefficentR.Y];
                        Block DomainBlockG = DomainArray[Current_coefficentG.X, Current_coefficentG.Y];
                        Block DomainBlockB = DomainArray[Current_coefficentB.X, Current_coefficentB.Y];
                        
                        for (int rotate = 0; rotate < Current_coefficentR.rotate + 1; ++rotate)
                        {
                            RotateNewImageR = RotateColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        }

                        for (int rotate = 0; rotate < Current_coefficentG.rotate + 1; ++rotate)
                        {
                            RotateNewImageG = RotateColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        }

                        for (int rotate = 0; rotate < Current_coefficentB.rotate + 1; ++rotate)
                        {
                            RotateNewImageB = RotateColor(RotateNewImageB, domain_block_size, DomainBlockB);
                        }

                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                    
                                Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                int R = (int)(0.75 * colorR.R + (Current_coefficentR.shift));
                                if (R < 0)
                                    R = 0;
                                if (R > 255)
                                    R = 255;
                           
                                Color colorG = RotateNewImageG[DomainBlockG.X + (pix_x * 2), DomainBlockG.Y + (pix_y * 2)];
                                int G = (int)(0.75 * colorG.G + (Current_coefficentG.shift));
                                if (G < 0)
                                    G = 0;
                                if (G > 255)
                                    G = 255;

                                Color colorB = RotateNewImageB[DomainBlockB.X + (pix_x * 2), DomainBlockB.Y + (pix_y * 2)];
                                int B = (int)(0.75 * colorB.B + (Current_coefficentB.shift));
                                if (B < 0)
                                    B = 0;
                                if (B > 255)
                                    B = 255;

                                Color Newcolor = Color.FromArgb(R, G, B);
                                NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                            }
                        }
                        RotateNewImageR = NewImageColor;
                        RotateNewImageG = NewImageColor;
                        RotateNewImageB = NewImageColor;

                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\Expanded file tmp.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            System.Console.WriteLine("");
            Compression(@"C:\Users\Dima Bogdanov\Documents\Visual Studio 2017\Projects\NewFractalCompression\NewFractalCompression\lena.jpg");
            Decompression();
            sw.Stop();
            System.Console.WriteLine((sw.ElapsedMilliseconds / 100.0).ToString());
            System.Console.WriteLine();
            System.Console.WriteLine("Please, enter the key");
            Console.ReadKey();
        }
    }
}
