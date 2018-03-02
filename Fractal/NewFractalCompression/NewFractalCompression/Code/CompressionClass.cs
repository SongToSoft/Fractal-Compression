using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace NewFractalCompression.Code
{
    class CompressionClass
    {
        public static int range_num_width, range_num_height;
        public static int domain_num_width, domain_num_height;
        public static int range_block_size, domain_block_size;
        public static int count, block_all_num;

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
            //Коэффицент для треугольных блоков
            public int Check;
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
        static Block CreateBlockRange(int X, int Y, Color[,] ImageColor, int range_block_size, double[,] Brightness, int Check)
        {
            Block Block = new Block();
            Block.X = X;
            Block.Y = Y;
            Block.SumR = 0;
            Block.SumG = 0;
            Block.SumB = 0;
            Block.Px = 0;
            Block.Py = 0;
            Block.Check = Check;
            //Счетчик для считывания треугольной матрицы
            int NewCount = 0;
            double BlockBrightness = 0;
            //Треугольная матрица слева
            if (Check == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size - NewCount; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i, Block.Y + j].R;
                        Block.SumG += ImageColor[Block.X + i, Block.Y + j].G;
                        Block.SumB += ImageColor[Block.X + i, Block.Y + j].B;
                        BlockBrightness += Brightness[Block.X + i, Block.Y + j];
                        Block.Px += i * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                    }
                    ++NewCount;
                }
            }
            //Треугольная матрица справа
            else
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = range_block_size - 1 - NewCount; j < range_block_size; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i, Block.Y + j].R;
                        Block.SumG += ImageColor[Block.X + i, Block.Y + j].G;
                        Block.SumB += ImageColor[Block.X + i, Block.Y + j].B;
                        BlockBrightness += Brightness[Block.X + i, Block.Y + j];
                        Block.Px += i * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
                    }
                    ++NewCount;
                }
            }
            Block.Px = Block.Px / BlockBrightness;
            Block.Py = Block.Py / BlockBrightness;
            return Block;
        }
        //Создание доменного блока
        static Block CreateBlockDomain(int X, int Y, Color[,] ImageColor, int range_block_size, double[,] Brightness, int Check)
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
            int NewCount = 0;
            Block.Check = Check;
            if (Check == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size - NewCount; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i * 2, Block.Y + j * 2].R;
                        Block.SumG += ImageColor[Block.X + i * 2, Block.Y + j * 2].G;
                        Block.SumB += ImageColor[Block.X + i * 2, Block.Y + j * 2].B;
                        //Считаем общую яркость для блока
                        BlockBrightness += Brightness[Block.X + i * 2, Block.Y + j * 2];
                        //Считаем координаты центра масс блока
                        Block.Px += i * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                    }
                    ++NewCount;
                }
            }
            else
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = range_block_size - 1 - NewCount; j < range_block_size; ++j)
                    {
                        Block.SumR += ImageColor[Block.X + i * 2, Block.Y + j * 2].R;
                        Block.SumG += ImageColor[Block.X + i * 2, Block.Y + j * 2].G;
                        Block.SumB += ImageColor[Block.X + i * 2, Block.Y + j * 2].B;
                        //Считаем общую яркость для блока
                        BlockBrightness += Brightness[Block.X + i * 2, Block.Y + j * 2];
                        //Считаем координаты центра масс блока
                        Block.Px += i * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                        Block.Py += j * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
                    }
                    ++NewCount;
                }
            }
        
            Block.Px = Block.Px / BlockBrightness;
            Block.Py = Block.Py / BlockBrightness;
            return Block;
        }
        //Определение угла между центрами масс двух блоков (используется для классификации)
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
            return (angle * 57.2958);
        }
        //Отразить блок по вертикали
        static Color[,] ReverseColor(Color[,] ImageColor, int block_size, Block Block)
        {

            Color[,] NewImageColor = ImageColor;
            Color[,] BlockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            //Считывание нужного блока из всего изображения
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    BlockImageColor[i, j] = NewImageColor[Block.X + i, Block.Y + j];
                }
            }
            //Отражение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    tmp_BlockImageColor[i, j] = BlockImageColor[block_size - i - 1, j];
                }
            }
            BlockImageColor = tmp_BlockImageColor;
            //Запись блока в изображение
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
        //Поворот на 90 градусов блока во всём изображении
        static Color[,] RotateColor(Color[,] ImageColor, int block_size, Block Block)
        {
            Color[,] NewImageColor = ImageColor;
            Color[,] BlockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            //Считывание нужного блока из всего изображения
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    BlockImageColor[i, j] = NewImageColor[Block.X + i, Block.Y + j];
                }
            }
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
            //Запись блока в изображение
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
        static Color[,] RotateTriangleColor(Color[,] ImageColor, int block_size, Block Block)
        {
            Color[,] NewImageColor = ImageColor;
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    NewImageColor[Block.X + i, Block.Y + j] = ImageColor[Block.Y + j, Block.X + i];
                }
            }
            return NewImageColor;
        }
        //Определение сдвига по яркости между двумя блоками
        static double Shift(Block RangeBlock, Block DomainBlock, int range_block_size, int flag)
        {
            double shift = 0;
            if (flag == 0)
                shift = ((RangeBlock.SumR) - (0.75 * DomainBlock.SumR)) / (range_block_size * range_block_size);
            if (flag == 1)
                shift = ((RangeBlock.SumG) - (0.75 * DomainBlock.SumG)) / (range_block_size * range_block_size);
            if (flag == 2)
                shift = ((RangeBlock.SumB) - (0.75 * DomainBlock.SumB)) / (range_block_size * range_block_size);
            return shift;
        }
        //Определение метрики между двумя блоками
        static double Distance(Color[,] RangeImageColor, Color[,] DomainImageColor, Block RangeBlock, Block DomainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            int NewCount = 0;
            if (flag == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size - NewCount; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].R;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].R;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                    ++NewCount;
                }
                return distance;
            }
            if (flag == 1)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size - NewCount; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].G;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].G;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                    ++NewCount;
                }
                return distance;
            }
            if (flag == 2)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size - NewCount; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].B;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].B;
                        distance += Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2);
                    }
                    ++NewCount;
                }
                return distance;
            }
            return distance;
        }
        static void ParallCompare(int istart, int iend, int jstart, int jend, Color[,] ImageColor, Block[,] RangeArray, Block[,] DomainArray, Coefficients[,,] CompressCoeff, StreamWriter sw)
        {
            for (int colorflag = 0; colorflag < 1; ++colorflag)
            {
                for (int i = istart; i < iend; ++i)
                {
                    for (int j = jstart; j < jend; ++j)
                    {
                        //Block RangeBlock = RangeArray[i, j];
                        int current_x = 0;
                        int current_y = 0;
                        double current_distance = Double.MaxValue;
                        double current_shift = 0;
                        int current_rotate = 0;
                        for (int k = 0; k < domain_num_width * 2; ++k)
                        {
                            for (int l = 0; l < domain_num_height * 2; ++l)
                            {
                                Color[,] DomainImageColor = ImageColor;
                                Color[,] RangeImageColor = ImageColor;
                                //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
                                if (Angle(RangeArray[i, j], DomainArray[k, l]) <= 30)
                                //if (true)
                                {
                                    if (DomainArray[k, l].Check == 1)
                                    {
                                        //DomainImageColor = RotateTriangleColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                                        DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                                    }
                                    if (RangeArray[i, j].Check == 1)
                                    {
                                        //RangeImageColor = RotateTriangleColor(RangeImageColor, range_block_size, RangeArray[i, j]);
                                        RangeImageColor = RotateColor(RangeImageColor, range_block_size, RangeArray[i, j]);
                                    }
                                    for (int rotate = 0; rotate < 2; ++rotate)
                                    {
                                        DomainImageColor = RotateTriangleColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                                        double shift = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, colorflag);
                                        double distance = Distance(RangeImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shift, colorflag);
                                        if (distance < current_distance)
                                        {
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
                        double proc = ((100 * count) / block_all_num);
                        System.Console.Write(proc);
                        System.Console.Write(" %");
                        System.Console.WriteLine();
                        ++count;
                        CompressCoeff[i, j, colorflag].X = current_x;
                        CompressCoeff[i, j, colorflag].Y = current_y;
                        CompressCoeff[i, j, colorflag].rotate = current_rotate;
                        CompressCoeff[i, j, colorflag].shift = current_shift;
                        printCoefficients(CompressCoeff[i, j, colorflag]);
                        sw.WriteLine(CompressCoeff[i, j, colorflag].X + " " + CompressCoeff[i, j, colorflag].Y + " " + CompressCoeff[i, j, colorflag].rotate + " " + CompressCoeff[i, j, colorflag].shift);
                    }
                }
            }
        }
        static public void Compression(string filename)
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
            range_block_size = 2;
            //Создаём ранговые блоки
            range_num_width = Image.Width / range_block_size;
            range_num_height = Image.Height / range_block_size;
            Block[,] RangeArray = new Block[2 * range_num_width, 2 * range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        RangeArray[i + (k * range_num_width), j + (k * range_num_height)] = CreateBlockRange(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage, k);
                    }
                }
            }
            //Создаем доменные блоки
            domain_num_width = range_num_width - 1;
            domain_num_height = range_num_height - 1;
            domain_block_size = range_block_size * 2;
            Block[,] DomainArray = new Block[2 * domain_num_width, 2 * domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        DomainArray[i + (k * domain_num_width), j + (k * domain_num_height)] = CreateBlockDomain(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage, k);
                    }
                }
            }
            //Алгоритм сжатия
            count = 1;
            //Общеее число преобразований
            //block_all_num = 3 * range_num_width * range_num_height;
            block_all_num = range_num_width * range_num_height * 4;
            Coefficients[,,] CompressCoeff = new Coefficients[2 * range_num_width, 2 * range_num_height, 4];
            StreamWriter sw = new StreamWriter(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression.txt");
            sw.Write(Image.Width);
            sw.Write(" ");
            sw.Write(Image.Height);
            sw.WriteLine();
            sw.WriteLine(range_block_size);
            //Непосредственное сжатие
            ParallCompare(0, range_num_width * 2, 0, range_num_height * 2, ImageColor, RangeArray, DomainArray, CompressCoeff, sw);
            sw.Close();
        }
        static public void Decompression()
        {
            //Алгоритм декомпрессии
            StreamReader sr = new StreamReader(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression.txt");
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

            //Color[,] NewImageColor = new Color[NewImage.Width, NewImage.Height];
            //for (int i = 0; i < NewImage.Width; ++i)
            //{
            //    for (int j = 0; j < NewImage.Height; ++j)
            //    {
            //        NewImageColor[i, j] = NewImage.GetPixel(i, j);
            //    }
            //}
            //double[,] NewBrightnessImage = new double[NewImage.Width, NewImage.Height];
            //for (int i = 0; i < NewImage.Width; ++i)
            //{
            //    for (int j = 0; j < NewImage.Height; ++j)
            //    {
            //        NewBrightnessImage[i, j] = NewImage.GetPixel(i, j).GetBrightness();
            //    }
            //}

            Coefficients[,] CompressCoeffR = new Coefficients[2 * range_num_width, 2 * range_num_height];
            srt = sr.ReadLine();
            for (int i = 0; i < range_num_width * 2; ++i)
            {
                for (int j = 0; j < range_num_height * 2; ++j)
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
            //Coefficients[,] CompressCoeffG = new Coefficients[range_num_width, range_num_height];
            //for (int i = 0; i < range_num_width; ++i)
            //{
            //    for (int j = 0; j < range_num_height; ++j)
            //    {
            //        //srt = sr.ReadLine();
            //        soul = srt.Split();
            //        CompressCoeffG[i, j].X = Convert.ToInt32(soul[0]);
            //        CompressCoeffG[i, j].Y = Convert.ToInt32(soul[1]);
            //        CompressCoeffG[i, j].rotate = Convert.ToInt32(soul[2]);
            //        CompressCoeffG[i, j].shift = Convert.ToDouble(soul[3]);
            //        srt = sr.ReadLine();
            //        //printCoefficients(CompressCoeff[i, j]);
            //    }
            //}

            //Coefficients[,] CompressCoeffB = new Coefficients[range_num_width, range_num_height];
            //for (int i = 0; i < range_num_width; ++i)
            //{
            //    for (int j = 0; j < range_num_height; ++j)
            //    {
            //        //srt = sr.ReadLine();
            //        soul = srt.Split();
            //        CompressCoeffB[i, j].X = Convert.ToInt32(soul[0]);
            //        CompressCoeffB[i, j].Y = Convert.ToInt32(soul[1]);
            //        CompressCoeffB[i, j].rotate = Convert.ToInt32(soul[2]);
            //        CompressCoeffB[i, j].shift = Convert.ToDouble(soul[3]);
            //        srt = sr.ReadLine();
            //        //printCoefficients(CompressCoeff[i, j]);
            //    }
            //}

            sr.Close();
            //Создаём ранговые блоки
            Block[,] RangeArray = new Block[2 * range_num_width, 2 * range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        RangeArray[i + (k * range_num_width), j + (k * range_num_height)].X = i * range_block_size;
                        RangeArray[i + (k * range_num_width), j + (k * range_num_height)].Y = j * range_block_size;
                        RangeArray[i + (k * range_num_width), j + (k * range_num_height)].Check = k;
                        //RangeArray[i + (k * range_num_width), j + (k * range_num_height)] = CreateBlockRange(i * range_block_size, j * range_block_size, NewImageColor, range_block_size, NewBrightnessImage, k);
                    }
                    //RangeArray[i, j].X = i * range_block_size;
                    //RangeArray[i, j].Y = j * range_block_size;
                }
            }
            //Создаем доменные блоки
            int domain_num_width = range_num_width - 1;
            int domain_num_height = range_num_height - 1;
            int domain_block_size = range_block_size * 2;
            Block[,] DomainArray = new Block[2 * domain_num_width, 2 * domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        DomainArray[i + (k * domain_num_width), j + (k * domain_num_height)].X = i * range_block_size;
                        DomainArray[i + (k * domain_num_width), j + (k * domain_num_height)].Y = j * range_block_size;
                        DomainArray[i + (k * domain_num_width), j + (k * domain_num_height)].Check = k;
                        //DomainArray[i + (k * domain_num_width), j + (k * domain_num_height)] = CreateBlockDomain(i * range_block_size, j * range_block_size, NewImageColor, range_block_size, NewBrightnessImage, k);
                    }
                    //DomainArray[i, j].X = i * range_block_size;
                    //DomainArray[i, j].Y = j * range_block_size;
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
                //Color[,] RotateNewImageG = NewImageColor;
                //Color[,] RotateNewImageB = NewImageColor;

                for (int i = 0; i < range_num_width * 2; ++i)
                {
                    for (int j = 0; j < range_num_height * 2; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficentR = CompressCoeffR[i, j];
                        //Coefficients Current_coefficentG = CompressCoeffG[i, j];
                        //Coefficients Current_coefficentB = CompressCoeffB[i, j];

                        Block DomainBlockR = DomainArray[Current_coefficentR.X, Current_coefficentR.Y];
                        //Block DomainBlockG = DomainArray[Current_coefficentG.X, Current_coefficentG.Y];
                        //Block DomainBlockB = DomainArray[Current_coefficentB.X, Current_coefficentB.Y];

                        ////Зеркальное отражение блоков
                        //if (Current_coefficentR.rotate > 3)
                        //    RotateNewImageR = ReverseColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //if (Current_coefficentG.rotate > 3)
                        //    RotateNewImageG = ReverseColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        //if (Current_coefficentB.rotate > 3)
                        //    RotateNewImageB = ReverseColor(RotateNewImageB, domain_block_size, DomainBlockB);

                        //if (((DomainBlockR.Check == 1) && (RangeBlock.Check == 0)) || ((DomainBlockR.Check == 0) && (RangeBlock.Check == 1)))
                        //{
                        //    RotateNewImageR = RotateColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //}
                        //for (int rotate = 0; rotate <= Current_coefficentR.rotate; ++rotate)
                        //{
                        //    RotateNewImageR = RotateTriangleColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //}
                        //for (int rotate = 0; rotate <= Current_coefficentG.rotate; ++rotate)
                        //{
                        //    RotateNewImageG = RotateColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        //}

                        //for (int rotate = 0; rotate <= Current_coefficentB.rotate; ++rotate)
                        //{
                        //    RotateNewImageB = RotateColor(RotateNewImageB, domain_block_size, DomainBlockB);
                        //}
                        int NewCount = 0;
                        if (RangeBlock.Check == 0)
                        {
                            for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                            {
                                for (int pix_y = 0; pix_y < range_block_size - NewCount; ++pix_y)
                                {
                                    Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                    int R = (int)(0.75 * colorR.R + (Current_coefficentR.shift));
                                    if (R < 0)
                                        R = 0;
                                    if (R > 255)
                                        R = 255;

                                    //Color colorG = RotateNewImageG[DomainBlockG.X + (pix_x * 2), DomainBlockG.Y + (pix_y * 2)];
                                    //int G = (int)(0.75 * colorG.G + (Current_coefficentG.shift));
                                    //if (G < 0)
                                    //    G = 0;
                                    //if (G > 255)
                                    //    G = 255;

                                    //Color colorB = RotateNewImageB[DomainBlockB.X + (pix_x * 2), DomainBlockB.Y + (pix_y * 2)];
                                    //int B = (int)(0.75 * colorB.B + (Current_coefficentB.shift));
                                    //if (B < 0)
                                    //    B = 0;
                                    //if (B > 255)
                                    //    B = 255;

                                    //Color Newcolor = Color.FromArgb(R, G, B);
                                    Color Newcolor = Color.FromArgb(R, R, R);
                                    NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                                    
                                }
                                ++NewCount;
                            }
                            RotateNewImageR = NewImageColor;
                            //RotateNewImageG = NewImageColor;
                            //RotateNewImageB = NewImageColor;
                        }
                        else
                        {
                            for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                            {
                                for (int pix_y = range_block_size - 1 - NewCount; pix_y < range_block_size; ++pix_y)
                                {
                                    Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                    int R = (int)(0.75 * colorR.R + (Current_coefficentR.shift));
                                    if (R < 0)
                                        R = 0;
                                    if (R > 255)
                                        R = 255;

                                    //Color colorG = RotateNewImageG[DomainBlockG.X + (pix_x * 2), DomainBlockG.Y + (pix_y * 2)];
                                    //int G = (int)(0.75 * colorG.G + (Current_coefficentG.shift));
                                    //if (G < 0)
                                    //    G = 0;
                                    //if (G > 255)
                                    //    G = 255;

                                    //Color colorB = RotateNewImageB[DomainBlockB.X + (pix_x * 2), DomainBlockB.Y + (pix_y * 2)];
                                    //int B = (int)(0.75 * colorB.B + (Current_coefficentB.shift));
                                    //if (B < 0)
                                    //    B = 0;
                                    //if (B > 255)
                                    //    B = 255;

                                    //Color Newcolor = Color.FromArgb(R, G, B);
                                    Color Newcolor = Color.FromArgb(R, R, R);
                                    NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                                }
                                ++NewCount;
                            }
                            RotateNewImageR = NewImageColor;
                            //RotateNewImageG = NewImageColor;
                            //RotateNewImageB = NewImageColor;
                        }
                    }
                        
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
