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
    [Obsolete("BlockCompression is deprecated, please use QuadTreeCompression.")]
    class BlockCompression : AbstractCompression
    {
        //Отразить блок по вертикали
        private static Color[,] ReverseColor(Color[,] imageColor, int block_size, Object.Block block)
        {
            Color[,] newImageColor = imageColor;
            Color[,] blockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            //Считывание нужного блока из всего изображения
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    blockImageColor[i, j] = newImageColor[block.X + i, block.Y + j];
                }
            }
            //Отражение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    tmp_BlockImageColor[i, j] = blockImageColor[block_size - i - 1, j];
                }
            }
            blockImageColor = tmp_BlockImageColor;
            //Запись блока в изображение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    //NewImageColor[Block.X + i, Block.Y + j] = tmp_BlockImageColor[i, j];
                    newImageColor[block.X + i, block.Y + j] = blockImageColor[i, j];
                }
            }
            return newImageColor;
        }
 
        //Поворот на 90 градусов блока во всём изображении
        private static Color[,] RotateColor(Color[,] imageColor, int block_size, Object.Block block)
        {
            Color[,] newImageColor = imageColor;
            Color[,] blockImageColor = new Color[block_size, block_size];
            Color[,] tmp_BlockImageColor = new Color[block_size, block_size];
            //Считывание нужного блока из всего изображения
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    blockImageColor[i, j] = newImageColor[block.X + i, block.Y + j];
                }
            }
            //Поворот блока на 90 градусов против часовой стрелки
            Color tmp;
            for (int i = 0; i < block_size / 2; i++)
            {
                for (int j = i; j < block_size - 1 - i; j++)
                {
                    tmp = blockImageColor[i, j];
                    blockImageColor[i, j] = blockImageColor[block_size - j - 1, i];
                    blockImageColor[block_size - j - 1, i] = blockImageColor[block_size - i - 1, block_size - j - 1];
                    blockImageColor[block_size - i - 1, block_size - j - 1] = blockImageColor[j, block_size - i - 1];
                    blockImageColor[j, block_size - i - 1] = tmp;
                }
            }
            //Запись блока в изображение
            for (int i = 0; i < block_size; ++i)
            {
                for (int j = 0; j < block_size; ++j)
                {
                    newImageColor[block.X + i, block.Y + j] = blockImageColor[i, j];
                }
            }
            return newImageColor;
        }

        //Сравнение коэффицентов
        static bool CoeffEquality(Object.Coefficients a, Object.Coefficients b)
        {
            if (a.X == b.X)
                if (a.Y == b.Y)
                    if (a.rotate == b.rotate)
                        if (a.shift == b.shift)
                            return true;
            return false;
        }

        private static void FindCoefficients(int istart)
        {
            for (int colorflag_tmp = 0; colorflag_tmp < 1; ++colorflag_tmp)
            {
                for (int i = istart; i < istart + 1; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
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
                                Color[,] DomainImageColor = classImageColor;
                                double shiftR = Shift(rangeArray[i, j], domainArray[k, l], range_block_size, 0);
                                double shiftG = Shift(rangeArray[i, j], domainArray[k, l], range_block_size, 1);
                                double shiftB = Shift(rangeArray[i, j], domainArray[k, l], range_block_size, 2);

                                double distance = Metrics.DistanceClass(classImageColor, DomainImageColor, rangeArray[i, j], domainArray[k, l], range_block_size, shiftR, colorflag_tmp);
                                if (distance < 10000000)
                                {
                                    oneflag = true;
                                    current_x = k;
                                    current_y = l;
                                    current_shiftR = shiftR;
                                    current_shiftG = shiftG;
                                    current_shiftB = shiftB;
                                    current_distance = distance;
                                }
                                else
                                {
                                    if (distance < current_distance)
                                    {
                                        current_x = k;
                                        current_y = l;
                                        current_shiftR = shiftR;
                                        current_shiftG = shiftG;
                                        current_shiftB = shiftB;
                                        current_distance = distance;
                                    }
                                }
                                if (oneflag == true)
                                    break;

                            }
                            if (oneflag == true)
                                break;
                        }
                        compressCoeff[i, j, colorflag_tmp].X = current_x;
                        compressCoeff[i, j, colorflag_tmp].Y = current_y;
                        compressCoeff[i, j, colorflag_tmp].rotate = current_rotate;
                        compressCoeff[i, j, colorflag_tmp].shiftR = (int)current_shiftR;
                        compressCoeff[i, j, colorflag_tmp].shiftG = (int)current_shiftG;
                        compressCoeff[i, j, colorflag_tmp].shiftB = (int)current_shiftB; 
                    }
                }
            }
        }

        public static void Compression(string filename, string quality)
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
            Bitmap image = new Bitmap(filename);
            classImageColor = new Color[image.Width, image.Height];
            for (int i = 0; i < image.Width; ++i)
            {
                for (int j = 0; j < image.Height; ++j)
                {
                    classImageColor[i, j] = image.GetPixel(i, j);
                }
            }
            double[,] BrightnessImage = new double[image.Width, image.Height];
            for (int i = 0; i < image.Width; ++i)
            {
                for (int j = 0; j < image.Height; ++j)
                {
                    BrightnessImage[i, j] = image.GetPixel(i, j).GetBrightness();
                }
            }
            //Основной параметр, отвечающий за размеры ранговых блоков
            range_block_size = 128;
            //Создаём ранговые блоки
            range_num_width = image.Width / range_block_size;
            range_num_height = image.Height / range_block_size;
            rangeArray = new Object.Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    rangeArray[i, j] = CreateBlockRange(i * range_block_size, j * range_block_size, classImageColor, range_block_size, BrightnessImage);
                }
            }
            //Создаем доменные блоки
            domain_num_width = range_num_width - 1;
            domain_num_height = range_num_height - 1;
            domain_block_size = range_block_size * 2;
            domainArray = new Object.Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    domainArray[i, j] = CreateBlockDomain(i * range_block_size, j * range_block_size, classImageColor, range_block_size, BrightnessImage);
                }
            }
            //Алгоритм сжатия
            count = 1;
            //Общеее число преобразований
            block_all_num = colorflag * range_num_width * range_num_height;
            compressCoeff = new Object.Coefficients[range_num_width, range_num_height, 1];
            BinaryWriter bw = new BinaryWriter(File.Open(@"C:\Users\Admin\Documents\GitHub\Fractal-Compression\NewFractalCompression\NewFractalCompression\Compression", FileMode.Create));
            Parallel.For(0, range_num_width, FindCoefficients);
            //Выводим коэффиценты в файл
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(image.Width), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(image.Height), 2));
            bw.Write(MyConverter.Convert(BitConverter.GetBytes(range_block_size), 1));
            for (int k = 0; k < 1; ++k)
            {
                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Byte[] X = BitConverter.GetBytes(compressCoeff[i, j, k].X);
                        Byte[] Y = BitConverter.GetBytes(compressCoeff[i, j, k].Y);

                        Byte[] SR = BitConverter.GetBytes(compressCoeff[i, j, k].shiftR);
                        Byte[] SG = BitConverter.GetBytes(compressCoeff[i, j, k].shiftG);
                        Byte[] SB = BitConverter.GetBytes(compressCoeff[i, j, k].shiftB);

                        bw.Write(MyConverter.Convert(X, 1));
                        bw.Write(MyConverter.Convert(Y, 1));
                        bw.Write(MyConverter.Convert(SR, 2));
                        bw.Write(MyConverter.Convert(SG, 2));
                        bw.Write(MyConverter.Convert(SB, 2));
                    }
                }
            }
            bw.Close();
        }

        public static void ColorDecompression()
        {
            byte[] BytesFile = File.ReadAllBytes(@"C:\Users\Admin\Documents\GitHub\Fractal-Compression\NewFractalCompression\NewFractalCompression\Compression");
            int fileCount = 0;
            //Коэффициент масштабирования
            int scale = 1;
            Byte[] tmp = MyConverter.ReadByte(BytesFile, 0, 2);
            int Image_width = BitConverter.ToInt16(tmp, 0) * scale;

            fileCount += 2;
            tmp = MyConverter.ReadByte(BytesFile, 2, 4);
            int Image_height = BitConverter.ToInt16(tmp, 0) * scale;

            fileCount += 2;
            tmp = MyConverter.ReadByte(BytesFile, 4, 5);
            int range_block_size = tmp[0] * scale;

            fileCount += 1;
            Bitmap newImage = new Bitmap(Image_width, Image_height);
            int range_num_width = newImage.Width / range_block_size;
            int range_num_height = newImage.Height / range_block_size;
            compressCoeff = new Object.Coefficients[range_num_width, range_num_height, 1];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    tmp = MyConverter.ReadByte(BytesFile, fileCount, fileCount + 1);
                    compressCoeff[i, j, 0].X = tmp[0];
                    fileCount += 1;

                    tmp = MyConverter.ReadByte(BytesFile, fileCount, fileCount + 1);
                    compressCoeff[i, j, 0].Y = tmp[0];
                    fileCount += 1;

                    tmp = MyConverter.ReadByte(BytesFile, fileCount, fileCount + 2);
                    compressCoeff[i, j, 0].shiftR = BitConverter.ToInt16(tmp, 0);
                    fileCount += 2;
                    tmp = MyConverter.ReadByte(BytesFile, fileCount, fileCount + 2);
                    compressCoeff[i, j, 0].shiftG = BitConverter.ToInt16(tmp, 0);
                    fileCount += 2;
                    tmp = MyConverter.ReadByte(BytesFile, fileCount, fileCount + 2);
                    compressCoeff[i, j, 0].shiftB = BitConverter.ToInt16(tmp, 0);
                    fileCount += 2;
                }
            }
            //Создаём ранговые блоки
            Object.Block[,] rangeArray = new Object.Block[range_num_width, range_num_height];
            for (int i = 0; i < range_num_width; ++i)
            {
                for (int j = 0; j < range_num_height; ++j)
                {
                    rangeArray[i, j].X = i * range_block_size;
                    rangeArray[i, j].Y = j * range_block_size;
                }
            }
            //Создаем доменные блоки
            int domain_num_width = range_num_width - 1;
            int domain_num_height = range_num_height - 1;
            int domain_block_size = range_block_size * 2;
            Object.Block[,] domainArray = new Object.Block[domain_num_width, domain_num_height];
            for (int i = 0; i < domain_num_width; ++i)
            {
                for (int j = 0; j < domain_num_height; ++j)
                {
                    domainArray[i, j].X = i * range_block_size;
                    domainArray[i, j].Y = j * range_block_size;
                }
            }

            for (int it = 0; it < 10; ++it)
            {
                Color[,] newImageColor = new Color[newImage.Width, newImage.Height];
                for (int i = 0; i < newImage.Width; ++i)
                {
                    for (int j = 0; j < newImage.Height; ++j)
                    {
                        newImageColor[i, j] = newImage.GetPixel(i, j);
                    }
                }
                Color[,] rotateNewImageR = newImageColor;

                for (int i = 0; i < range_num_width; ++i)
                {
                    for (int j = 0; j < range_num_height; ++j)
                    {
                        Object.Block rangeBlock = rangeArray[i, j];
                        Object.Coefficients current_coefficentR = compressCoeff[i, j, 0];

                        Object.Block domainBlockR = domainArray[current_coefficentR.X, current_coefficentR.Y];
                        for (int pix_x = 0; pix_x < range_block_size; ++pix_x)
                        {
                            for (int pix_y = 0; pix_y < range_block_size; ++pix_y)
                            {
                                Color colorR1 = rotateNewImageR[domainBlockR.X + (pix_x), domainBlockR.Y + (pix_y)];
                                Color colorR = rotateNewImageR[domainBlockR.X + (pix_x * 2), domainBlockR.Y + (pix_y * 2)];
                                //int R = (int)(U * ((colorR.R + colorR1.R) / 2) + (Current_coefficentR.shiftR));
                                int r = (int)(u * colorR.R + (current_coefficentR.shiftR));
                                if (r < 0)
                                    r = 0;
                                if (r > 255)
                                    r = 255;
                                Color colorG1 = rotateNewImageR[domainBlockR.X + (pix_x), domainBlockR.Y + (pix_y)];
                                Color colorG = rotateNewImageR[domainBlockR.X + (pix_x * 2), domainBlockR.Y + (pix_y * 2)];
                                //int G = (int)(U * ((colorG.G + colorG1.G) / 2) + (Current_coefficentR.shiftG));
                                int g = (int)(u * colorG.G + (current_coefficentR.shiftG));
                                if (g < 0)
                                    g = 0;
                                if (g > 255)
                                    g = 255;

                                Color colorB1 = rotateNewImageR[domainBlockR.X + (pix_x), domainBlockR.Y + (pix_y)];
                                Color colorB = rotateNewImageR[domainBlockR.X + (pix_x * 2), domainBlockR.Y + (pix_y * 2)];
                                //int B = (int)(U * ((colorB.B + colorB1.B) / 2) + (Current_coefficentR.shiftB));
                                int b = (int)(u * colorB.B + (current_coefficentR.shiftB));
                                if (b < 0)
                                    b = 0;
                                if (b > 255)
                                    b = 255;

                                Color Newcolor = Color.FromArgb(r, g, b);
                                newImage.SetPixel(rangeBlock.X + pix_x, rangeBlock.Y + pix_y, Newcolor);
                            }
                        }
                    }
                }
            }
            newImage.Save(@"C:\Users\Admin\Documents\GitHub\Fractal-Compression\NewFractalCompression\NewFractalCompression\Expanded file.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

    }
}
