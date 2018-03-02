using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression.NewCode
{
    class NewCompressionClass
    {
        public struct RangeBlock
        {
            public int X, Y;
            public int SumR, SumG,SumB;
            //Коэффиценты для классификации
            public double Px, Py;
            public int rotate;
        }
        public struct DomainBlock
        {
            public int X, Y;
            public int SumR, SumG, SumB;
            //Коэффиценты для классификации
            public double Px, Py;
            public Color[] BlockImage;
            public int rotate;
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
        //Создание рангового блока
        public static RangeBlock CreateRangeBlock(int X, int Y, Color[] ImageColor, int block_size, double[] Brightness, int rotate)
        {
            RangeBlock Block = new RangeBlock
            {
                X = X,
                Y = Y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0,
                rotate = rotate
            };
            double BlockBrightness = 0;
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    Block.SumR += ImageColor[(((Block.X + i) * block_size) + (Block.Y + j))].R;
                    Block.SumG += ImageColor[(((Block.X + i) * block_size) + (Block.Y + j))].G;
                    Block.SumB += ImageColor[(((Block.X + i) * block_size) + (Block.Y + j))].B;
                    BlockBrightness += Brightness[(Block.X + i) * block_size + Block.Y + j];
                    Block.Px += i * Brightness[((Block.X + i) * block_size) + Block.Y + j] - ((block_size + 1) / 2);
                    Block.Py += j * Brightness[((Block.X + i) * block_size) + Block.Y + j] - ((block_size + 1) / 2);
                }
            }
            Block.Px = Block.Px / BlockBrightness;
            Block.Py = Block.Py / BlockBrightness;
            return Block;
        }
        public static DomainBlock CreateDomainBlock(int X, int Y, Color[] ImageColor, int block_size, double[] Brightness, int rotate)
        {
            DomainBlock Block = new DomainBlock
            {
                X = X,
                Y = Y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0,
                rotate = rotate
            };
            double BlockBrightness = 0;
            Block.BlockImage = new Color[(block_size * 2) * (block_size * 2)];
            for (int i = 0; i < block_size * 2; ++i)
            {
                for (int j = 0; j < block_size * 2; ++j)
                {
                    Block.BlockImage[(i * block_size * 2) + j] = ImageColor[(((Block.X + i) * block_size * 2) + (Block.Y + j))];
                }
            }
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    Block.SumR += ImageColor[(((Block.X + i * 2) * block_size) + (Block.Y + j * 2))].R;
                    Block.SumG += ImageColor[(((Block.X + i * 2) * block_size) + (Block.Y + j * 2))].G;
                    Block.SumB += ImageColor[(((Block.X + i * 2) * block_size) + (Block.Y + j * 2))].B;
                    BlockBrightness += Brightness[(Block.X + i * 2) * block_size + Block.Y + j * 2];
                    Block.Px += i * Brightness[((Block.X + i * 2) * block_size) + Block.Y + j * 2] - ((block_size + 1) / 2);
                    Block.Py += j * Brightness[((Block.X + i * 2) * block_size) + Block.Y + j * 2] - ((block_size + 1) / 2);
                }
            }
            Block.Px = Block.Px / BlockBrightness;
            Block.Py = Block.Py / BlockBrightness;
            return Block;
        }
        //Поворот на 90 градусов блока во всём изображении
        static Color[] RotateColor(int block_size, DomainBlock Block, int rotate)
        {
            Color[] NewImageColor = Block.BlockImage;
            Color[,] BlockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            //Считывание нужного блока из всего изображения
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    BlockImageColor[i, j] = NewImageColor[(i) * block_size + (j)];
                }
            }
            for (int r = 0; r < rotate; ++r)
            {
                //Поворот блока на 90 градусов
                for (int i = 0; i < block_size; ++i)
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
            }
            //Запись блока в изображение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    //NewImageColor[Block.X + i, Block.Y + j] = tmp_BlockImageColor[i, j];
                    NewImageColor[(i) * block_size + j] = BlockImageColor[i, j];
                }
            }
            return NewImageColor;
        }
        //Определение сдвига по яркости между двумя блоками
        static double Shift(RangeBlock RangeBlock, DomainBlock DomainBlock, int range_block_size, int flag)
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
        //Определение метрики между двумя блоками
        static double Distance(RangeBlock RangeBlock, DomainBlock DomainBlock, int block_size, double shift, int flag, Color[] ImageColor)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            if (flag == 1)
            {
                for (int i = 0; i < block_size; ++i)
                {
                    for (int j = 0; j < block_size; ++j)
                    {
                        RangeValue = ImageColor[(RangeBlock.X + i) * block_size + (RangeBlock.Y + j)].R;
                        DomainValue = DomainBlock.BlockImage[(i * 2) * block_size + (j * 2)].R;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            if (flag == 2)
            {
                for (int i = 0; i < block_size; ++i)
                {
                    for (int j = 0; j < block_size; ++j)
                    {
                        RangeValue = ImageColor[(RangeBlock.X + i) * block_size + (RangeBlock.Y + j)].G;
                        DomainValue = DomainBlock.BlockImage[(i * 2) * block_size + (j * 2)].G;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            if (flag == 3)
            {
                for (int i = 0; i < block_size; ++i)
                {
                    for (int j = 0; j < block_size; ++j)
                    {
                        RangeValue = ImageColor[(RangeBlock.X + i) * block_size + (RangeBlock.Y + j)].B;
                        DomainValue = DomainBlock.BlockImage[(i * 2) * block_size + (j * 2)].B;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                }
                return distance;
            }
            return distance;
        }
        //Основной алгоритм сжатия
        public static void Compression(string filename)
        {
            Bitmap Image = new Bitmap(filename);
            Color[] ImageColor = new Color[Image.Width * Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    ImageColor[(i * Image.Width) + j] = Image.GetPixel(i, j);
                }
            } 
            double[] BrightnessImage = new double[Image.Width * Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    BrightnessImage[(i * Image.Width) + j] = Image.GetPixel(i, j).GetBrightness();
                }
            }
            //Основной параметр, отвечающий за размеры ранговых блоков
            int range_block_size = 5;
            //Создаём ранговые блоки
            int range_num_width = Image.Width / range_block_size;
            int range_num_height = Image.Height / range_block_size;
            RangeBlock[] RangeArray = new RangeBlock[range_num_width * range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    RangeArray[(i * range_num_width) + j] = CreateRangeBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage, 3);    
                }
            }
            //Создаем доменные блоки
            int domain_num_width = range_num_width - 1;
            int domain_num_height = range_num_height - 1;
            int domain_block_size = range_block_size * 2;
            //DomainBlock[] DomainArray = new DomainBlock[4 * domain_num_width * domain_num_height];
            //for (int i = 0; i < domain_num_width; ++i)
            //{
            //    for (int j = 0; j < domain_num_height; ++j)
            //    {
            //        for (int k = 0; k < 4; ++k)
            //        {
            //            DomainArray[(i * domain_num_width) + (j * domain_num_height) + k] = CreateDomainBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage, k);
            //            //Делаем поворот доменного блока
            //            DomainArray[(i * domain_num_width) + (j * domain_num_height) + k].BlockImage = RotateColor(domain_block_size, DomainArray[(i * domain_num_width) + (j * domain_num_height) + k], k);
            //        }
            //    }
            //}
            DomainBlock[] DomainArray = new DomainBlock[domain_num_width * domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    DomainArray[(i * domain_num_width) + (j)] = CreateDomainBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage, 0);
                   
                }
            }

            ////Алгоритм сжатия
            int count = 1;
            //Общеее число преобразований
            int block_all_num = 3 * range_num_width * range_num_height;
            Coefficients[] CompressCoeff = new Coefficients[range_num_width * range_num_height];
            StreamWriter sw = new StreamWriter(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal-Compression-OPENCL\NewFractalCompression\NewFractalCompression\Compression.txt");
            sw.Write(Image.Width);
            sw.Write(" ");
            sw.Write(Image.Height);
            sw.WriteLine();
            sw.WriteLine(range_block_size);
            for (int colorflag = 1; colorflag < 4; ++colorflag)
            {
                for (int i = 0; i < (range_num_width * range_num_height); ++i)
                {
                    int current_x = 0;
                    int current_y = 0;
                    double current_distance = Double.MaxValue;
                    double current_shift = 0;
                    int current_rotate = 0;
                    for (int j = 0; j < (domain_num_width * domain_num_height); ++j)
                    {
                        double shift = Shift(RangeArray[i], DomainArray[j], range_block_size, colorflag);
                        double distance = Distance(RangeArray[i], DomainArray[j], range_block_size, shift, colorflag, ImageColor);
                        if (distance < current_distance)
                        {
                            current_x = DomainArray[j].X / range_block_size;
                            current_y = DomainArray[j].Y / range_block_size;
                            current_shift = shift;
                            current_distance = distance;
                            current_rotate = DomainArray[j].rotate;
                        }
                    }
                    double proc = ((100 * count) / block_all_num);
                    System.Console.Write(proc);
                    System.Console.Write(" %");
                    System.Console.WriteLine();
                    ++count;
                    CompressCoeff[i].X = current_x;
                    CompressCoeff[i].Y = current_y;
                    CompressCoeff[i].rotate = current_rotate;
                    CompressCoeff[i].shift = current_shift;
                    printCoefficients(CompressCoeff[i]);
                    sw.WriteLine(CompressCoeff[i].X + " " + CompressCoeff[i].Y + " " + CompressCoeff[i].rotate + " " + CompressCoeff[i].shift);
                }
            }
            //for (int colorflag = 1; colorflag < 4; ++colorflag)
            //{
            //    for (int i = 0; i < range_num_width; ++i)
            //    {
            //        for (int j = 0; j < range_num_height; ++j)
            //        {
            //            //Block RangeBlock = RangeArray[i, j];
            //            int current_x = 0;
            //            int current_y = 0;
            //            double current_distance = Double.MaxValue;
            //            double current_shift = 0;
            //            int current_rotate = 0;
            //            for (int k = 0; k < domain_num_width; ++k)
            //            {
            //                for (int l = 0; l < domain_num_height; ++l)
            //                {
            //                    //Block DomainBlock = DomainArray[k, l];
            //                    //Bitmap DomainImage = Image;
            //                    Color[,] DomainImageColor = ImageColor;
            //                    //System.Console.WriteLine(Angle(RangeBlock, DomainBlock));
            //                    //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
            //                    if (Angle(RangeArray[i, j], DomainArray[k, l]) <= 1)
            //                    //if (true)
            //                    {
            //                        for (int rotate = 0; rotate < 4; ++rotate)
            //                        {
            //                            //if (rotate > 3)
            //                            //{
            //                            //    DomainImageColor = ReverseColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
            //                            //}
            //                            DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
            //                            double shift = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, colorflag);
            //                            //double shift = Shift(Image, RangeBlock, DomainBlock, range_block_size);
            //                            double distance = Distance(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shift, colorflag);
            //                            //double distance = Distance(Image, DomainImage, RangeBlock, DomainBlock, range_block_size, shift);
            //                            if (distance < current_distance)
            //                            {
            //                                //current_x = DomainBlock.X;
            //                                //current_y = DomainBlock.Y;
            //                                current_x = k;
            //                                current_y = l;
            //                                current_shift = shift;
            //                                current_distance = distance;
            //                                current_rotate = rotate;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            //System.Console.Write(count);
            //            //System.Console.Write('/');
            //            //System.Console.Write((range_num_width * range_num_height));
            //            double proc = ((100 * count) / block_all_num);
            //            System.Console.Write(proc);
            //            System.Console.Write(" %");
            //            System.Console.WriteLine();
            //            ++count;
            //            CompressCoeff[i, j].X = current_x;
            //            CompressCoeff[i, j].Y = current_y;
            //            CompressCoeff[i, j].rotate = current_rotate;
            //            CompressCoeff[i, j].shift = current_shift;
            //            printCoefficients(CompressCoeff[i, j]);
            //            sw.WriteLine(CompressCoeff[i, j].X + " " + CompressCoeff[i, j].Y + " " + CompressCoeff[i, j].rotate + " " + CompressCoeff[i, j].shift);
            //        }
            //    }
            //    //count = 1;
            //}
        }
    }
}
