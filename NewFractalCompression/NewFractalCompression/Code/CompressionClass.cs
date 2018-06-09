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
        private static double U = 0.75;
        public static int range_num_width, range_num_height;
        public static int domain_num_width, domain_num_height;
        public static int range_block_size, domain_block_size, colorflag;
        public static int count, block_all_num;
        public static Block[,] RangeArray;
        public static Block[,] DomainArray;
        public static Color[,] ImageColor;
        public static Coefficients[,,] CompressCoeff;
        public struct Coefficients
        {
            public int X;
            public int Y;
            //public int position;
            public int rotate;
            //public double shift;
            public int shift;
            //Один из вариантов
            public int shiftR;
            public int shiftG;
            public int shiftB;
        }
        public struct Block
        {
            public int X;
            public int Y;
            //public int position;
            public int SumR;
            public int SumG;
            public int SumB;
            //Эти коэффициенты использовались в одной из метрик
            public int SumR2;
            public int SumG2;
            public int SumB2;
            //Коэффиценты для классификации
            public double Px;
            public double Py;
        }
        static void PrintCoefficients(Coefficients Fac)
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
        static Block CreateBlockRange(int X, int Y, Color[,] ImageColor, int range_block_size, double[,] Brightness)
        {
            Block Block = new Block
            {
                X = X,
                Y = Y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0
            };
            double BlockBrightness = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    Block.SumR += ImageColor[Block.X + i, Block.Y + j].R;
                    Block.SumG += ImageColor[Block.X + i, Block.Y + j].G;
                    Block.SumB += ImageColor[Block.X + i, Block.Y + j].B;

                    Block.SumR2 += (Block.SumR * Block.SumR);
                    Block.SumG2 += (Block.SumG * Block.SumG);
                    Block.SumB2 += (Block.SumB * Block.SumB);

                    BlockBrightness += Brightness[Block.X + i, Block.Y + j];
                    Block.Px += i * Brightness[Block.X + i, Block.Y + j];
                    Block.Py += j * Brightness[Block.X + i, Block.Y + j];
                }
            }
            Block.Px = (Block.Px / BlockBrightness) - ((range_block_size + 1) / 2);
            Block.Py = (Block.Py / BlockBrightness) - ((range_block_size + 1) / 2);
            return Block;
        }
        //Создание доменного блока
        static Block CreateBlockDomain(int X, int Y, Color[,] ImageColor, int range_block_size, double[,] Brightness)
        {
            Block Block = new Block
            {
                X = X,
                Y = Y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0
            };
            double BlockBrightness = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    Block.SumR += ImageColor[Block.X + i * 2, Block.Y + j * 2].R;
                    Block.SumG += ImageColor[Block.X + i * 2, Block.Y + j * 2].G;
                    Block.SumB += ImageColor[Block.X + i * 2, Block.Y + j * 2].B;

                    Block.SumR2 += (Block.SumR * Block.SumR);
                    Block.SumG2 += (Block.SumG * Block.SumG);
                    Block.SumB2 += (Block.SumB * Block.SumB);

                    //Считаем общую яркость для блока
                    BlockBrightness += Brightness[Block.X + i * 2, Block.Y + j * 2];
                    //Считаем координаты центра масс блока
                    Block.Px += i * Brightness[Block.X + i * 2, Block.Y + j * 2];
                    Block.Py += j * Brightness[Block.X + i * 2, Block.Y + j * 2];
                }
            }
            Block.Px = (Block.Px / BlockBrightness) - ((range_block_size + 1) / 2);
            Block.Py = (Block.Py / BlockBrightness) - ((range_block_size + 1) / 2);
            return Block;
        }
        //Определение угла между центрами масс двух блоков (используется для классификации)
        static double Angle(Block RangeBlock, Block DomainBlock)
        {
            double angle = 0;
            double Vec1X = RangeBlock.Px;
            double Vec1Y = RangeBlock.Py;
            double Vec2X = DomainBlock.Px;
            double Vec2Y = DomainBlock.Py;

            double Sum1 = Math.Sqrt(Vec1X * Vec1X + Vec1Y * Vec1Y);
            double Sum2 = Math.Sqrt(Vec2X * Vec2X + Vec2Y * Vec2Y);
            double Scalar = Vec1X * Vec2X + Vec1Y * Vec2Y;
            angle = Math.Acos(Scalar / (Sum1 * Sum2));
            //Переводим из радин в градусы
            //return (angle * 57.2958);
            return (angle);
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
            //Поворот блока на 90 градусов против часовой стрелки
            Color tmp;
            for (int i = 0; i < block_size / 2; i++)
            {
                for (int j = i; j < block_size - 1 - i; j++)
                {
                    tmp = BlockImageColor[i, j];
                    BlockImageColor[i, j] = BlockImageColor[block_size - j - 1, i];
                    BlockImageColor[block_size - j - 1, i] = BlockImageColor[block_size - i - 1, block_size - j - 1];
                    BlockImageColor[block_size - i - 1, block_size - j - 1] = BlockImageColor[j, block_size - i - 1];
                    BlockImageColor[j, block_size - i - 1] = tmp;
                }
            }
            //Запись блока в изображение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    NewImageColor[Block.X + i, Block.Y + j] = BlockImageColor[i, j];
                }
            }
            return NewImageColor;
        }
        //Определение сдвига по яркости между двумя блоками
        static double Shift(Block RangeBlock, Block DomainBlock, int range_block_size, int flag)
        {
            double shift = 0;
            if (flag == 0)
                shift = ((RangeBlock.SumR) - (U * DomainBlock.SumR)) / (range_block_size * range_block_size);
            if (flag == 1)
                shift = ((RangeBlock.SumG) - (U * DomainBlock.SumG)) / (range_block_size * range_block_size);
            if (flag == 2)
                shift = ((RangeBlock.SumB) - (U * DomainBlock.SumB)) / (range_block_size * range_block_size);
            return shift;
        }
        //Сравнение коэффицентов
        static bool CoeffEquality(Coefficients A, Coefficients B)
        {
            if (A.X == B.X)
                if (A.Y == B.Y)
                    if (A.rotate == B.rotate)
                        if (A.shift == B.shift)
                            return true;
            return false;
        }
        static void ParallCompress(int istart)
        {
            for (int colorflag_tmp = 0; colorflag_tmp < colorflag; ++colorflag_tmp)
            {
                for (int i = istart; i < istart + 1; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        //Block RangeBlock = RangeArray[i, j];
                        //int current_position = 0;
                        int current_x = 0;
                        int current_y = 0;
                        double current_distance = Double.MaxValue;
                        double current_shift = 0;
                        int current_rotate = 0;
                        bool oneflag = false;
                        for (int k = 0; k < domain_num_width; ++k)
                        {
                            for (int l = 0; l < domain_num_height; ++l)
                            {
                                Color[,] DomainImageColor = ImageColor;
                                //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
                                //if (Angle(RangeArray[i, j], DomainArray[k, l]) <= (Math.PI / 2))
                                //if (true)
                                //{
                                    //for (int rotate = 0; rotate < 4; ++rotate)
                                    //{
                                        DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                                        double shift = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, colorflag_tmp);
                                        double distance = Metrics.DistanceClass(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shift, colorflag_tmp);
                                        if (distance < 1000000)
                                        {
                                            oneflag = true;
                                            current_x = k;
                                            current_y = l;
                                            current_shift = shift;
                                            current_distance = distance;
                                            //current_rotate = rotate;
                                            //current_rotate = 0;
                                        }
                                        else
                                        {
                                            if (distance < current_distance)
                                            {
                                                current_x = k;
                                                current_y = l;
                                                current_shift = shift;
                                                current_distance = distance;
                                                //current_rotate = rotate;
                                                current_rotate = 0;
                                            }
                                        }
                                    //    if (oneflag == true)
                                    //        break;
                                    //}
                                //}
                                if (oneflag == true)
                                    break;
                            }
                            if (oneflag == true)
                                break;
                        }
                        //double proc = ((100 * count) / block_all_num);
                        //System.Console.Write(proc);
                        //System.Console.Write(" %");
                        //System.Console.WriteLine();
                        //++count;
                        CompressCoeff[i, j, colorflag_tmp].X = current_x;
                        CompressCoeff[i, j, colorflag_tmp].Y = current_y;
                        CompressCoeff[i, j, colorflag_tmp].rotate = current_rotate;
                        CompressCoeff[i, j, colorflag_tmp].shift = (int)current_shift;
                        //PrintCoefficients(CompressCoeff[i, j, colorflag_tmp]);
                        //sw.WriteLine(CompressCoeff[i, j, colorflag].X + " " + CompressCoeff[i, j, colorflag].Y + " " + CompressCoeff[i, j, colorflag].rotate + " " + CompressCoeff[i, j, colorflag].shift);
                    }
                }
                //count = 1;
            }
        }
        static public void Compression(string filename, string quality)
        {
            if (!(File.Exists(filename)))
            {
                System.Console.WriteLine("Файла не существует");
                return;
            }
            //Черно-белый файл или нет
            if (quality == "Black")
            {
                colorflag = 1;
            }
            else
            {
                if (quality == "Color")
                {
                    colorflag = 3;
                }
                else
                {
                    System.Console.WriteLine("Тип выбран не правильно");
                    return;
                }
            }
            Bitmap Image = new Bitmap(filename);
            ImageColor = new Color[Image.Width, Image.Height];
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
            range_block_size = 4;
            //Создаём ранговые блоки
            range_num_width = Image.Width / range_block_size;
            range_num_height = Image.Height / range_block_size;
            RangeArray = new Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    RangeArray[i, j] = CreateBlockRange(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage);
                    //RangeArray[i, j].position = j + (i * range_num_width);
                    //System.Console.WriteLine(RangeArray[i, j].position);
                }
            }
            //Создаем доменные блоки
            domain_num_width = range_num_width - 1;
            domain_num_height = range_num_height - 1;
            domain_block_size = range_block_size * 2;
            DomainArray = new Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    DomainArray[i, j] = CreateBlockDomain(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage);
                    //DomainArray[i, j].position = j + (i * range_num_width);
                    //System.Console.WriteLine(DomainArray[i, j].position);
                    //System.Console.WriteLine(i + " " + j);
                }
            }
            //Алгоритм сжатия
            count = 1;
            //Общеее число преобразований
            block_all_num = colorflag * range_num_width * range_num_height;
            CompressCoeff = new Coefficients[range_num_width, range_num_height, colorflag];
            //StreamWriter sw = new StreamWriter(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression.txt");
            BinaryWriter bw = new BinaryWriter(File.Open(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression", FileMode.Create));

            Parallel.For(0, range_num_width, ParallCompress);
            //Выводим коэффиценты в файл
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(Image.Width), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(Image.Height), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(range_block_size), 1));
            for (int k = 0; k < colorflag; ++k)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Byte[] X = BitConverter.GetBytes(CompressCoeff[i, j, k].X);
                        Byte[] Y = BitConverter.GetBytes(CompressCoeff[i, j, k].Y);
                        //Byte[] R = BitConverter.GetBytes(CompressCoeff[i, j, k].rotate);
                        Byte[] S = BitConverter.GetBytes(CompressCoeff[i, j, k].shift);


                        //sw.WriteLine(CompressCoeff[i, j, k].X + " " + CompressCoeff[i, j, k].Y + " " + CompressCoeff[i, j, k].rotate + " " + CompressCoeff[i, j, k].shift);
               
                        bw.Write(MyConverter.Convert(X, 1));
                        bw.Write(MyConverter.Convert(Y, 1));
                        //bw.Write(MyConverter.Convert(R, 1));
                        bw.Write(MyConverter.Convert(S, 2));
                    }
                }
            }
            //sw.Close();
            bw.Close();
        }
        static public void ColorDecompression()
        {
            byte[] BytesFile = File.ReadAllBytes(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression");
            int FileCount = 0;
            //Коэффициент масштабирования
            double Scale = 1;
            Byte[] Tmp = MyConverter.ReadByte(BytesFile, 0, 2);
            int Image_width = (int)(BitConverter.ToInt16(Tmp, 0) * Scale);
            //+
            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 2, 4);
            int Image_height = (int)(BitConverter.ToInt16(Tmp, 0) * Scale);
            //+

            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 4, 5);
            int range_block_size = (int)(Tmp[0] * Scale);
            //+

            FileCount += 1;
            Bitmap NewImage = new Bitmap(Image_width, Image_height);
            int range_num_width = NewImage.Width / range_block_size;
            int range_num_height = NewImage.Height / range_block_size;
            CompressCoeff = new Coefficients[range_num_width, range_num_height, 3];
            for (int k = 0; k < 3; ++k)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].X = BitConverter.ToInt16(Tmp, 0);
                        CompressCoeff[i, j, k].X = Tmp[0];
                        FileCount += 1;

                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].Y = BitConverter.ToInt16(Tmp, 0);
                        CompressCoeff[i, j, k].Y = Tmp[0];
                        FileCount += 1;

                        //Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].rotate = Tmp[0];
                        //++FileCount;

                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 2);
                        CompressCoeff[i, j, k].shift = BitConverter.ToInt16(Tmp, 0);
                        FileCount += 2;
                    }
                }
            }
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
                Color[,] RotateNewImageR = NewImageColor;
                Color[,] RotateNewImageG = NewImageColor;
                Color[,] RotateNewImageB = NewImageColor;

                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficentR = CompressCoeff[i, j, 0];
                        Coefficients Current_coefficentG = CompressCoeff[i, j, 1];
                        Coefficients Current_coefficentB = CompressCoeff[i, j, 2];

                        Block DomainBlockR = DomainArray[Current_coefficentR.X, Current_coefficentR.Y];
                        Block DomainBlockG = DomainArray[Current_coefficentG.X, Current_coefficentG.Y];
                        Block DomainBlockB = DomainArray[Current_coefficentB.X, Current_coefficentB.Y];

                        ////Зеркальное отражение блоков
                        //if (Current_coefficentR.rotate > 3)
                        //    RotateNewImageR = ReverseColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //if (Current_coefficentG.rotate > 3)
                        //    RotateNewImageG = ReverseColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        //if (Current_coefficentB.rotate > 3)
                        //    RotateNewImageB = ReverseColor(RotateNewImageB, domain_block_size, DomainBlockB);

                        //for (int rotate = 0; rotate <= Current_coefficentR.rotate; ++rotate)
                        //{
                        //    RotateNewImageR = RotateColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //}

                        //for (int rotate = 0; rotate <= Current_coefficentG.rotate; ++rotate)
                        //{
                        //    RotateNewImageG = RotateColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        //}

                        //for (int rotate = 0; rotate <= Current_coefficentB.rotate; ++rotate)
                        //{
                        //    RotateNewImageB = RotateColor(RotateNewImageB, domain_block_size, DomainBlockB);
                        //}

                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                                Color colorR1 = RotateNewImageR[DomainBlockR.X + (pix_x), DomainBlockR.Y + (pix_y)];
                                Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                int R = (int)(U * ((colorR.R + colorR1.R) / 2) + (Current_coefficentR.shift));
                                //int R = (int)(U * colorR.R + (Current_coefficentR.shift));
                                if (R < 0)
                                    R = 0;
                                if (R > 255)
                                    R = 255;
                                Color colorG1 = RotateNewImageG[DomainBlockG.X + (pix_x), DomainBlockG.Y + (pix_y)];
                                Color colorG = RotateNewImageG[DomainBlockG.X + (pix_x * 2), DomainBlockG.Y + (pix_y * 2)];
                                int G = (int)(U * ((colorG.G + colorG1.G) / 2) + (Current_coefficentG.shift));
                                //int G = (int)(U * colorG.G + (Current_coefficentG.shift));
                                if (G < 0)
                                    G = 0;
                                if (G > 255)
                                    G = 255;

                                Color colorB1 = RotateNewImageB[DomainBlockB.X + (pix_x), DomainBlockB.Y + (pix_y)];
                                Color colorB = RotateNewImageB[DomainBlockB.X + (pix_x * 2), DomainBlockB.Y + (pix_y * 2)];
                                int B = (int)(U * ((colorB.B + colorB1.B) / 2) + (Current_coefficentB.shift));
                                //int B = (int)(U * colorB.B + (Current_coefficentB.shift));
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
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
        static public void BlackDecompression()
        {
            byte[] BytesFile = File.ReadAllBytes(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression");
            int FileCount = 0;
            //Коэффициент масштабирования
            double Scale = 1;
            Byte[] Tmp = MyConverter.ReadByte(BytesFile, 0, 2);
            int Image_width = (int)(BitConverter.ToInt16(Tmp, 0) * Scale);
            //+
            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 2, 4);
            int Image_height = (int)(BitConverter.ToInt16(Tmp, 0) * Scale);
            //+

            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 4, 5);
            int range_block_size = (int)(Tmp[0] * Scale);
            //+

            FileCount += 1;
            Bitmap NewImage = new Bitmap(Image_width, Image_height);
            int range_num_width = NewImage.Width / range_block_size;
            int range_num_height = NewImage.Height / range_block_size;
            CompressCoeff = new Coefficients[range_num_width, range_num_height, 3];
            for (int k = 0; k < 1; ++k)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].X = BitConverter.ToInt16(Tmp, 0);
                        CompressCoeff[i, j, k].X = Tmp[0];
                        FileCount += 1;

                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].Y = BitConverter.ToInt16(Tmp, 0);
                        CompressCoeff[i, j, k].Y = Tmp[0];
                        FileCount += 1;

                        //Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                        //CompressCoeff[i, j, k].rotate = Tmp[0];
                        //++FileCount;

                        Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 2);
                        CompressCoeff[i, j, k].shift = BitConverter.ToInt16(Tmp, 0);
                        FileCount += 2;
                    }
                }
            }
            //int RW = 0;
            //for (int k = 0; k < 1; ++k)
            //{
            //    for (int i = 1; i < range_num_width; ++i)
            //    {
            //        for (int j = 1; j < range_num_height; ++j)
            //        {
            //           if (CoeffEquality(CompressCoeff[i - 1, j - 1, k], CompressCoeff[i, j, k]))
            //           {
            //                ++RW;
            //           }
            //        }
            //    }
            //}
            //System.Console.WriteLine(RW);
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
            for (int it = 0; it < 20; ++it)
            {
                Color[,] NewImageColor = new Color[NewImage.Width, NewImage.Height];
                for (int i = 0; i < NewImage.Width; ++i)
                {
                    for (int j = 0; j < NewImage.Width; ++j)
                    {
                        NewImageColor[i, j] = NewImage.GetPixel(i, j);
                    }
                }
                Color[,] RotateNewImageR = NewImageColor;

                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficentR = CompressCoeff[i, j, 0];

                        Block DomainBlockR = DomainArray[Current_coefficentR.X, Current_coefficentR.Y];

                        //System.Console.WriteLine(Current_coefficentR.rotate);
                        //for (int rotate = 0; rotate <= Current_coefficentR.rotate; ++rotate)
                        //{
                        //    RotateNewImageR = RotateColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        //}

                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                                Color colorR1 = RotateNewImageR[DomainBlockR.X + (pix_x), DomainBlockR.Y + (pix_y)];
                                Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                int R = (int)(U * ((colorR.R + colorR1.R) / 2) + (Current_coefficentR.shift));
                                //int R = (int)(U * colorR.R + (Current_coefficentR.shift));
                                if (R < 0)
                                    R = 0;
                                if (R > 255)
                                    R = 255;

                                Color Newcolor = Color.FromArgb(R, R, R);
                                NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                            }
                        }
                        RotateNewImageR = NewImageColor;

                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
        static public void CheckRotate(string filename)
        {
            Bitmap Image = new Bitmap(filename);
            ImageColor = new Color[Image.Width, Image.Height];
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
            for (int i = 0; i < 3; ++i)
                ImageColor = RotateColor(ImageColor, 256, CreateBlockDomain(0, 0, ImageColor, 256, BrightnessImage));
            Bitmap NewImage = new Bitmap(Image.Width, Image.Height);
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    NewImage.SetPixel(i, j, ImageColor[i, j]);
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Test Rotate.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

        }
        //Один из вариантов
        static public void NewParallCompress(int istart)
        {
            for (int colorflag_tmp = 0; colorflag_tmp < 1; ++colorflag_tmp)
            {
                for (int i = istart; i < istart + 1; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        //Block RangeBlock = RangeArray[i, j];
                        //int current_position = 0;
                        int current_x = 0;
                        int current_y = 0;
                        double current_distance = Double.MaxValue;
                        double current_shiftR = 0;
                        double current_shiftG = 0;
                        double current_shiftB = 0;
                        int current_rotate = 0;
                        bool oneflag = false;
                        for (int k = 0; k < domain_num_width; ++k)
                        {
                            for (int l = 0; l < domain_num_height; ++l)
                            {
                                Color[,] DomainImageColor = ImageColor;
                                //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
                                //if (Angle(RangeArray[i, j], DomainArray[k, l]) <= (Math.PI / 2))
                                //DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                                double shiftR = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 0);
                                double shiftG = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 1);
                                double shiftB = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 2);
                                //if (shiftR < current_shiftR)
                                //{
                                //    current_x = k;
                                //    current_y = l;
                                //    current_shiftR = shiftR;
                                //    current_shiftG = shiftG;
                                //    current_shiftB = shiftB;
                                //    //current_distance = distance;
                                //    //current_rotate = rotate;
                                //    //current_rotate = 0;
                                //}
                                //System.Console.WriteLine(Angle(RangeArray[i, j], DomainArray[k, l]));
                                //if (Angle(RangeArray[i, j], DomainArray[k, l]) <= 2)
                                {
                                    double distance = Metrics.DistanceClass(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shiftR, colorflag_tmp);
                                    //double distance = Metrics.DistanceShar(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shiftR, colorflag_tmp);
                                    //double distance = Metrics.PSNR(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shiftR, colorflag_tmp);
                                    if (distance < 10000000)
                                    {
                                        oneflag = true;
                                        current_x = k;
                                        current_y = l;
                                        current_shiftR = shiftR;
                                        current_shiftG = shiftG;
                                        current_shiftB = shiftB;
                                        current_distance = distance;
                                        //current_rotate = rotate;
                                        //current_rotate = 0;
                                    }
                                    else
                                    {
                                        //if (distance < current_distance)
                                        if (shiftR < current_shiftR)
                                        {
                                            current_x = k;
                                            current_y = l;
                                            current_shiftR = shiftR;
                                            current_shiftG = shiftG;
                                            current_shiftB = shiftB;
                                            current_distance = distance;
                                            //current_rotate = rotate;
                                            //current_rotate = 0;
                                        }
                                    }
                                    if (oneflag == true)
                                        break;
                                }

                            }
                            if (oneflag == true)
                                break;
                        }
                            //double proc = ((100 * count) / block_all_num);
                            //System.Console.Write(proc);
                            //System.Console.Write(" %");
                            //System.Console.WriteLine();
                            ++count;
                            CompressCoeff[i, j, colorflag_tmp].X = current_x;
                            CompressCoeff[i, j, colorflag_tmp].Y = current_y;
                            CompressCoeff[i, j, colorflag_tmp].rotate = current_rotate;
                            CompressCoeff[i, j, colorflag_tmp].shiftR = (int)current_shiftR;
                            CompressCoeff[i, j, colorflag_tmp].shiftG = (int)current_shiftG;
                            CompressCoeff[i, j, colorflag_tmp].shiftB = (int)current_shiftB;
                            //printCoefficients(CompressCoeff[i, j, colorflag_tmp]);
                            //sw.WriteLine(CompressCoeff[i, j, colorflag].X + " " + CompressCoeff[i, j, colorflag].Y + " " + CompressCoeff[i, j, colorflag].rotate + " " + CompressCoeff[i, j, colorflag].shift);
                        }
                    }
                }
        }
        static public void NewParallCompressRandom(int istart)
        {
            for (int colorflag_tmp = 0; colorflag_tmp < 1; ++colorflag_tmp)
            {
                for (int i = istart; i < istart + 1; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        //Block RangeBlock = RangeArray[i, j];
                        //int current_position = 0;
                        int current_x = 0;
                        int current_y = 0;
                        double current_distance = Double.MaxValue;
                        double current_shiftR = 0;
                        double current_shiftG = 0;
                        double current_shiftB = 0;
                        int current_rotate = 0;
                       // bool oneflag = false;
                        for (int rand = 0; rand < 10; ++rand)
                        {
                            Color[,] DomainImageColor = ImageColor;
                            //Выполняем выбор доменного блока, если угол его центр масс с ранговым блоком меньше определенного угла
                            //if (Angle(RangeArray[i, j], DomainArray[k, l]) <= (Math.PI / 2))
                            Random random = new Random();
                            int k = random.Next(0, domain_num_width);
                            int l = random.Next(0, domain_num_height);
                            if ((k > 255) || (l > 255))
                            {
                                System.Console.WriteLine("Yep");
                            }
                            DomainImageColor = RotateColor(DomainImageColor, domain_block_size, DomainArray[k, l]);
                            double shiftR = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 0);
                            double shiftG = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 1);
                            double shiftB = Shift(RangeArray[i, j], DomainArray[k, l], range_block_size, 2);
                            double distance = Metrics.DistanceClass(ImageColor, DomainImageColor, RangeArray[i, j], DomainArray[k, l], range_block_size, shiftR, colorflag_tmp);
                            //if (distance < 0)
                            if (distance < current_distance)
                            {
                                current_x = k;
                                current_y = l;
                                current_shiftR = shiftR;
                                current_shiftG = shiftG;
                                current_shiftB = shiftB;
                                current_distance = distance;
                                //current_rotate = rotate;
                                //current_rotate = 0;
                            }
                            //if (distance < 5000000)
                            //{
                            //    oneflag = true;
                            //    current_x = k;
                            //    current_y = l;
                            //    current_shiftR = shiftR;
                            //    current_shiftG = shiftG;
                            //    current_shiftB = shiftB;
                            //    current_distance = distance;
                            //    //current_rotate = rotate;
                            //    //current_rotate = 0;
                            //}
                            //else
                            //{
                            //    if (distance < current_distance)
                            //    {
                            //        current_x = k;
                            //        current_y = l;
                            //        current_shiftR = shiftR;
                            //        current_shiftG = shiftG;
                            //        current_shiftB = shiftB;
                            //        current_distance = distance;
                            //        //current_rotate = rotate;
                            //        //current_rotate = 0;
                            //    }
                            //}
                            //if (oneflag == true)
                                //break;
                        }
                        //double proc = ((100 * count) / block_all_num);
                        //System.Console.Write(proc);
                        //System.Console.Write(" %");
                        //System.Console.WriteLine();
                        //++count;
                        CompressCoeff[i, j, colorflag_tmp].X = current_x;
                        CompressCoeff[i, j, colorflag_tmp].Y = current_y;
                        CompressCoeff[i, j, colorflag_tmp].rotate = current_rotate;
                        CompressCoeff[i, j, colorflag_tmp].shiftR = (int)current_shiftR;
                        CompressCoeff[i, j, colorflag_tmp].shiftG = (int)current_shiftG;
                        CompressCoeff[i, j, colorflag_tmp].shiftB = (int)current_shiftB;
                        //printCoefficients(CompressCoeff[i, j, colorflag_tmp]);
                        //sw.WriteLine(CompressCoeff[i, j, colorflag].X + " " + CompressCoeff[i, j, colorflag].Y + " " + CompressCoeff[i, j, colorflag].rotate + " " + CompressCoeff[i, j, colorflag].shift);
                    }
                }
            }
        }
        static public void NewCompression(string filename, string quality)
        {
            if (!(File.Exists(filename)))
            {
                System.Console.WriteLine("Файла не существует");
                return;
            }
            //Черно-белый файл или нет
            if (quality == "Black")
            {
                colorflag = 1;
            }
            else
            {
                if (quality == "Color")
                {
                    colorflag = 3;
                }
                else
                {
                    System.Console.WriteLine("Тип выбран не правильно");
                    return;
                }
            }
            Bitmap Image = new Bitmap(filename);
            ImageColor = new Color[Image.Width, Image.Height];
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
            range_block_size = 4;
            //Создаём ранговые блоки
            range_num_width = Image.Width / range_block_size;
            range_num_height = Image.Height / range_block_size;
            RangeArray = new Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    RangeArray[i, j] = CreateBlockRange(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage);
                }
            }
            //Создаем доменные блоки
            domain_num_width = range_num_width - 1;
            domain_num_height = range_num_height - 1;
            domain_block_size = range_block_size * 2;
            DomainArray = new Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    DomainArray[i, j] = CreateBlockDomain(i * range_block_size, j * range_block_size, ImageColor, range_block_size, BrightnessImage);
                }
            }
            //domain_block_size = range_block_size * 2;
            //domain_num_width = range_num_width - domain_block_size;
            //domain_num_height = range_num_height - domain_block_size;
            //DomainArray = new Block[domain_num_width, domain_num_height];
            //for (int i = 0; i < domain_num_width; ++i)
            //{
            //    for (int j = 0; j < domain_num_height; ++j)
            //    {
            //        DomainArray[i, j] = CreateBlockDomain(i, j, ImageColor, range_block_size, BrightnessImage);
            //    }
            //}
            //Алгоритм сжатия
            count = 1;
            //Общеее число преобразований
            block_all_num = colorflag * range_num_width * range_num_height;
            CompressCoeff = new Coefficients[range_num_width, range_num_height, 1];
            //StreamWriter sw = new StreamWriter(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression.txt");
            BinaryWriter bw = new BinaryWriter(File.Open(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression", FileMode.Create));
            for (int i = 0; i < range_num_width; ++i)
            {
                NewParallCompress(i);
            }
            //Parallel.For(0, range_num_width, NewParallCompress);
            //Parallel.For(0, range_num_width, NewParallCompressRandom);
            //Выводим коэффиценты в файл
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(Image.Width), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(Image.Height), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(range_block_size), 1));
            for (int k = 0; k < 1; ++k)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Byte[] X = BitConverter.GetBytes(CompressCoeff[i, j, k].X);
                        Byte[] Y = BitConverter.GetBytes(CompressCoeff[i, j, k].Y);
                        //Byte[] R = BitConverter.GetBytes(CompressCoeff[i, j, k].rotate);
                        Byte[] SR = BitConverter.GetBytes(CompressCoeff[i, j, k].shiftR);
                        Byte[] SG = BitConverter.GetBytes(CompressCoeff[i, j, k].shiftG);
                        Byte[] SB = BitConverter.GetBytes(CompressCoeff[i, j, k].shiftB);


                        //sw.WriteLine(CompressCoeff[i, j, k].X + " " + CompressCoeff[i, j, k].Y + " " + CompressCoeff[i, j, k].rotate + " " + CompressCoeff[i, j, k].shift);

                        bw.Write(MyConverter.Convert(X, 1));
                        bw.Write(MyConverter.Convert(Y, 1));
                        //bw.Write(MyConverter.Convert(R, 1));
                        bw.Write(MyConverter.Convert(SR, 2));
                        bw.Write(MyConverter.Convert(SG, 2));
                        bw.Write(MyConverter.Convert(SB, 2));
                    }
                }
            }
            //sw.Close();
            bw.Close();
        }
        static public void NewColorDecompression()
        {
            byte[] BytesFile = File.ReadAllBytes(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Compression");
            int FileCount = 0;
            //Коэффициент масштабирования
            int Scale = 1;
            Byte[] Tmp = MyConverter.ReadByte(BytesFile, 0, 2);
            int Image_width = BitConverter.ToInt16(Tmp, 0) * Scale;
            //+
            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 2, 4);
            int Image_height = BitConverter.ToInt16(Tmp, 0) * Scale;
            //+

            FileCount += 2;
            Tmp = MyConverter.ReadByte(BytesFile, 4, 5);
            int range_block_size = Tmp[0] * Scale;
            //+

            FileCount += 1;
            //Bitmap NewImage = new Bitmap(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Sea.bmp");
            Bitmap NewImage = new Bitmap(Image_width, Image_height);
            int range_num_width = NewImage.Width / range_block_size;
            int range_num_height = NewImage.Height / range_block_size;
            CompressCoeff = new Coefficients[range_num_width, range_num_height, 1];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                    CompressCoeff[i, j, 0].X = Tmp[0];
                    FileCount += 1;

                    Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 1);
                    CompressCoeff[i, j, 0].Y = Tmp[0];
                    FileCount += 1;

                    Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 2);
                    CompressCoeff[i, j, 0].shiftR = BitConverter.ToInt16(Tmp, 0);
                    FileCount += 2;
                    Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 2);
                    CompressCoeff[i, j, 0].shiftG = BitConverter.ToInt16(Tmp, 0);
                    FileCount += 2;
                    Tmp = MyConverter.ReadByte(BytesFile, FileCount, FileCount + 2);
                    CompressCoeff[i, j, 0].shiftB = BitConverter.ToInt16(Tmp, 0);
                    FileCount += 2;
                    //System.Console.WriteLine(CompressCoeff[i, j, 0].X * 32 + " " + CompressCoeff[i, j, 0].Y * 32 + " " + CompressCoeff[i, j, 0].shiftR + " " + CompressCoeff[i, j, 0].shiftG + " " + CompressCoeff[i, j, 0].shiftB);
                }
            }
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
            //domain_block_size = range_block_size * 2;
            //domain_num_width = range_num_width - domain_block_size;
            //domain_num_height = range_num_height - domain_block_size;
            //DomainArray = new Block[domain_num_width, domain_num_height];
            //for (int i = 0; i < domain_num_width; ++i)
            //{
            //    for (int j = 0; j < domain_num_height; ++j)
            //    {
            //        DomainArray[i, j].X = i;
            //        DomainArray[i, j].X = j;
            //    }
            //}
            //for (int i = 0; i < range_num_width; ++i)
            //{
            //    for (int j = 0; j < range_num_height; ++j)
            //    {
            //        System.Console.WriteLine("(" + RangeArray[i, j].X + " - " + RangeArray[i, j].Y + ") " + CompressCoeff[i, j, 0].X + " " + CompressCoeff[i, j, 0].Y + " " + CompressCoeff[i, j, 0].shiftR + " " + CompressCoeff[i, j, 0].shiftG + " " + CompressCoeff[i, j, 0].shiftB);
            //    }
            //}
            for (int it = 0; it < 10; ++it)
            {
                Color[,] NewImageColor = new Color[NewImage.Width, NewImage.Height];
                for (int i = 0; i < NewImage.Width; ++i)
                {
                    for (int j = 0; j < NewImage.Height; ++j)
                    {
                        NewImageColor[i, j] = NewImage.GetPixel(i, j);
                    }
                }
                Color[,] RotateNewImageR = NewImageColor;

                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Block RangeBlock = RangeArray[i, j];
                        Coefficients Current_coefficentR = CompressCoeff[i, j, 0];

                        Block DomainBlockR = DomainArray[Current_coefficentR.X, Current_coefficentR.Y];
                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                                Color colorR1 = RotateNewImageR[DomainBlockR.X + (pix_x), DomainBlockR.Y + (pix_y)];
                                Color colorR = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                //int R = (int)(U * ((colorR.R + colorR1.R) / 2) + (Current_coefficentR.shiftR));
                                int R = (int)(U * colorR.R + (Current_coefficentR.shiftR));
                                if (R < 0)
                                    R = 0;
                                if (R > 255)
                                    R = 255;
                                Color colorG1 = RotateNewImageR[DomainBlockR.X + (pix_x), DomainBlockR.Y + (pix_y)];
                                Color colorG = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                //int G = (int)(U * ((colorG.G + colorG1.G) / 2) + (Current_coefficentR.shiftG));
                                int G = (int)(U * colorG.G + (Current_coefficentR.shiftG));
                                if (G < 0)
                                    G = 0;
                                if (G > 255)
                                    G = 255;

                                Color colorB1 = RotateNewImageR[DomainBlockR.X + (pix_x), DomainBlockR.Y + (pix_y)];
                                Color colorB = RotateNewImageR[DomainBlockR.X + (pix_x * 2), DomainBlockR.Y + (pix_y * 2)];
                                //int B = (int)(U * ((colorB.B + colorB1.B) / 2) + (Current_coefficentR.shiftB));
                                int B = (int)(U * colorB.B + (Current_coefficentR.shiftB));
                                if (B < 0)
                                    B = 0;
                                if (B > 255)
                                    B = 255;

                                Color Newcolor = Color.FromArgb(R, G, B);
                                NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                            }
                        }
                        //RotateNewImageR = NewImageColor;
                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

    }
}
