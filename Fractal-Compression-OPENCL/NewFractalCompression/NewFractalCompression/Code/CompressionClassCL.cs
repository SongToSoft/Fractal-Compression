using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using Cloo;
using System.Runtime.InteropServices;


namespace NewFractalCompression.Code
{
    class CompressionClassCL
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
                        Block.SumR += ImageColor[Block.X + i * 2, Block.Y + j * 2].R;
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
        //Определение сдвига по яркости между двумя блоками
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
        //Определение метрики между двумя блоками
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
        static public void Compression(string filename)
        {
            //Меняю все ComputePlatform.Platforms[1] на ComputePlatform.Platforms[0]
            ComputeContextPropertyList Properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
            ComputeContext Context = new ComputeContext(ComputeDeviceTypes.All, Properties, null, IntPtr.Zero);

            //Список устройств, для которых мы будем компилировать написанную программу
            List<ComputeDevice> Devs = new List<ComputeDevice>();
            Devs.Add(ComputePlatform.Platforms[0].Devices[0]);
            Devs.Add(ComputePlatform.Platforms[0].Devices[1]);
            Devs.Add(ComputePlatform.Platforms[0].Devices[2]);

            //Компиляция программы из CreateBlock
            ComputeProgram prog = null;
            try
            {
                prog = new ComputeProgram(Context, StrFunc.StrCreateBlockDom);

                //prog = new ComputeProgram(Context, StrFunc.StrNewCreateBlockDom);

                prog.Build(Devs, "", null, IntPtr.Zero);
            }
            catch
            {

            }
            ComputeKernel kernelCreateBlockDom = prog.CreateKernel("CreateBlockDom");

            //ComputeKernel kernelCreateBlockDom = prog.CreateKernel("NewCreateBlockDom");

            Bitmap Image = new Bitmap(filename);
            Color[] ImageColor = new Color[Image.Width * Image.Height];
            //Делаем ещё массивы т.к. Cloo не может работать ни с чем кроме long и типами явно приводимых к нему
            int[] ImageColorR = new int[Image.Width * Image.Height];
            int[] ImageColorG = new int[Image.Width * Image.Height];
            int[] ImageColorB = new int[Image.Width * Image.Height];
            //Считываем по пикселям изображение
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    ImageColor[(i) * Image.Width + j] = Image.GetPixel(i, j);
                }
            }
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    ImageColorR[(i) * Image.Width + j] = Image.GetPixel(i, j).R;
                    ImageColorG[(i) * Image.Width + j] = Image.GetPixel(i, j).G;
                    ImageColorB[(i) * Image.Width + j] = Image.GetPixel(i, j).B;
                }
            }
            double[] BrightnessImage = new double[Image.Width * Image.Height];
            for (int i = 0; i < Image.Width; ++i)
            {
                for (int j = 0; j < Image.Height; ++j)
                {
                    BrightnessImage[(i) * Image.Width + j] = Image.GetPixel(i, j).GetBrightness();
                }
            }
            //Основной параметр, отвечающий за размеры ранговых блоков
            int range_block_size = 5;
            //Создаём ранговые блоки
            int range_num_width = Image.Width / range_block_size;
            int range_num_height = Image.Height / range_block_size;
            Block[] RangeArray = new Block[range_num_width * range_num_height];

            ComputeBuffer<int> BufImageColorR = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, ImageColorR);
            ComputeBuffer<int> BufImageColorG = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, ImageColorG);
            ComputeBuffer<int> BufImageColorB = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, ImageColorB);
            ComputeBuffer<double> BufBrightness = new ComputeBuffer<double>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, BrightnessImage);
            //ComputeBuffer<int> BufWIDTH = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, Image.Width);
            for (int i = 0; i < RangeArray.Length; ++i)
            {
                for (int j = 0; j < RangeArray.Length; ++j)
                {
                    //ComputeBuffer<Block> BufBlock = new ComputeBuffer<Block>(Contex)
                    ComputeBuffer<int> BufX = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, i * range_block_size);
                    ComputeBuffer<int> BufY = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, j * range_block_size);
                    ComputeBuffer<int> Buf_range_block_size = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, range_block_size);
                    ComputeBuffer<int> BufSumR = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, RangeArray[(i) * range_num_width + j].SumR);
                    ComputeBuffer<int> BufSumG = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, RangeArray[(i) * range_num_width + j].SumG);
                    ComputeBuffer<int> BufSumB = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, RangeArray[(i) * range_num_width + j].SumB);
                    //ComputeBuffer<Color> BufImageColor = new ComputeBuffer<Color>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, ImageColor);
                    //Объявляем какие данные будут использоваться в программе StrCreateBlockDom
                    kernelCreateBlockDom.SetMemoryArgument(0, BufX);
                    kernelCreateBlockDom.SetMemoryArgument(1, BufY);
                    kernelCreateBlockDom.SetMemoryArgument(2, BufImageColorR);
                    kernelCreateBlockDom.SetMemoryArgument(3, BufImageColorG);
                    kernelCreateBlockDom.SetMemoryArgument(4, BufImageColorB);
                    kernelCreateBlockDom.SetMemoryArgument(5, Buf_range_block_size);
                    kernelCreateBlockDom.SetMemoryArgument(6, BufSumR);
                    kernelCreateBlockDom.SetMemoryArgument(7, BufSumG);
                    kernelCreateBlockDom.SetMemoryArgument(8, BufSumB);
                    //kernelCreateBlockDom.SetMemoryArgument(9, BufWIDTH);
                    StrFunc.x = i * range_block_size;
                    StrFunc.y = j * range_block_size;
                    StrFunc.SumR = 0;
                    StrFunc.SumG = 0;
                    StrFunc.SumB = 0;
                    //Создание програмной очереди. Не забудте указать устройство, на котором будет исполняться программа!
                    ComputeCommandQueue Queue = new ComputeCommandQueue(Context, Cloo.ComputePlatform.Platforms[0].Devices[0], Cloo.ComputeCommandQueueFlags.None);
                    //Старт. Execute запускает программу-ядро vecSum указанное колличество раз {}
                    Queue.Execute(kernelCreateBlockDom, null, new long[] { range_block_size * range_block_size }, null, null);
                    //Считывание данных из памяти устройства.
                    int SumR = 0, SumG = 0, SumB = 0;
                    GCHandle SumRCHandle = GCHandle.Alloc(SumR, GCHandleType.Pinned);
                    GCHandle SumGCHandle = GCHandle.Alloc(SumR, GCHandleType.Pinned);
                    GCHandle SumBCHandle = GCHandle.Alloc(SumB, GCHandleType.Pinned);
                    Queue.Read<int>(BufSumR, true, 0, 1, SumRCHandle.AddrOfPinnedObject(), null);
                    Queue.Read<int>(BufSumG, true, 0, 1, SumRCHandle.AddrOfPinnedObject(), null);
                    Queue.Read<int>(BufSumB, true, 0, 1, SumRCHandle.AddrOfPinnedObject(), null);
                    RangeArray[(i) * range_num_width + j].SumR = SumR;
                    RangeArray[(i) * range_num_width + j].SumG = SumG;
                    RangeArray[(i) * range_num_width + j].SumB = SumB;
                    RangeArray[(i) * range_num_width + j].X = i * range_block_size;
                    RangeArray[(i) * range_num_width + j].Y = j * range_block_size;

                    System.Console.Write(RangeArray[(i - 1) * range_num_width + j].X);
                    System.Console.Write(" ");
                    System.Console.Write(RangeArray[(i - 1) * range_num_width + j].Y);
                    System.Console.Write(" ");
                    System.Console.Write(RangeArray[(i - 1) * range_num_width + j].SumR);
                    System.Console.Write(" ");
                    System.Console.Write(RangeArray[(i - 1) * range_num_width + j].SumG);
                    System.Console.Write(" ");
                    System.Console.Write(RangeArray[(i - 1) * range_num_width + j].SumB);
                    System.Console.Write(" ");
                    System.Console.WriteLine();
                }
            }
            // Загрузка данных в указатели для дальнейшего использования.
            //ComputeBuffer<Block>[,] BufBlock = new ComputeBuffer<Block>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, range_num_height, RangeArray);
            //ComputeBuffer<float> bufV1 = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, v1);
            //ComputeBuffer<float> bufV2 = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, v2);
            //kernelVecSum.SetMemoryArgument(0, bufV1);
            //kernelVecSum.SetMemoryArgument(1, bufV2);
            //for (int i = 0; i < range_num_width; ++i)
            //{
            //    for (int j = 0; j < range_num_height; ++j)
            //    {
            //        RangeArray[(i - 1) * Image.Height + j] = CreateBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, false, BrightnessImage);
            //    }
            //}
            ////Создаем доменные блоки
            //int domain_num_width = range_num_width - 1;
            //int domain_num_height = range_num_height - 1;
            //int domain_block_size = range_block_size * 2;
            //Block[,] DomainArray = new Block[domain_num_width, domain_num_height];
            //for (int i = 0; i < domain_num_width; ++i)
            //{
            //    for (int j = 0; j < domain_num_height; ++j)
            //    {
            //        DomainArray[i, j] = CreateBlock(i * range_block_size, j * range_block_size, ImageColor, range_block_size, true, BrightnessImage);
            //    }
            //}
        }
        static public void Decompression()
        {
            //Алгоритм декомпрессии
            StreamReader sr = new StreamReader(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal-Compression-OPENCL\NewFractalCompression\NewFractalCompression\Compression.txt");
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

            Coefficients[,] CompressCoeffB = new Coefficients[range_num_width, range_num_height];

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

                        //Зеркальное отражение блоков
                        if (Current_coefficentR.rotate > 3)
                            RotateNewImageR = ReverseColor(RotateNewImageR, domain_block_size, DomainBlockR);
                        if (Current_coefficentG.rotate > 3)
                            RotateNewImageG = ReverseColor(RotateNewImageG, domain_block_size, DomainBlockG);
                        if (Current_coefficentB.rotate > 3)
                            RotateNewImageB = ReverseColor(RotateNewImageB, domain_block_size, DomainBlockB);

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

                                Color Newcolor = Color.FromArgb(R, R, R);
                                NewImage.SetPixel(RangeBlock.X + pix_x, RangeBlock.Y + pix_y, Newcolor);
                            }
                        }
                        RotateNewImageR = NewImageColor;
                        RotateNewImageG = NewImageColor;
                        RotateNewImageB = NewImageColor;

                    }
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal-Compression-OPENCL\NewFractalCompression\NewFractalCompression\Expanded file tmp 05.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
