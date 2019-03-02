using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Структура используемая в квадродеревьях
namespace NewFractalCompression.Code
{
    class BlockTree
    {
        public Object.Block mainBlock;
        //Узлы блока
        public BlockTree ul;
        public BlockTree ur;
        public BlockTree dl;
        public BlockTree dr;
        //Глубина дерева
        public int depth = 0;
        public double currentDistance = double.MaxValue;
        //Конструктор дерева
        public BlockTree(Object.Block block, int blockSize)
        {
            mainBlock = block;
            mainBlock.X = block.X;
            mainBlock.Y = block.Y;
            mainBlock.SumR = block.SumR;
            mainBlock.SumG = block.SumG;
            mainBlock.SumB = block.SumB;
            mainBlock.BlockSize = blockSize;
            mainBlock.Depth = QuadTreeCompression.numLock;
            if (QuadTreeCompression.numLock == (-1))
            {
                QuadTreeCompression.numLock = 2;
            }
            else
            {
                ++QuadTreeCompression.numLock;
            }
            //Для ранговых блоковы
            if (mainBlock.BlockSize > 2)
            {
                Object.Block tmpBlock = QuadTreeCompression.CreateBlockRange(block.X, block.Y, QuadTreeCompression.classImageColor, block.BlockSize / 2, QuadTreeCompression.brightnessImage);
                ul = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = QuadTreeCompression.CreateBlockRange(block.X + block.BlockSize / 2, block.Y, QuadTreeCompression.classImageColor, block.BlockSize / 2, QuadTreeCompression.brightnessImage);
                ur = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = QuadTreeCompression.CreateBlockRange(block.X, block.Y + block.BlockSize / 2, QuadTreeCompression.classImageColor, block.BlockSize / 2, QuadTreeCompression.brightnessImage);
                dl = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = QuadTreeCompression.CreateBlockRange(block.X + block.BlockSize / 2, block.Y + block.BlockSize / 2, QuadTreeCompression.classImageColor, block.BlockSize / 2, QuadTreeCompression.brightnessImage);
                dr = new BlockTree(tmpBlock, tmpBlock.BlockSize);
            }
        }
        //Вывод структуры дерева
        public void PrintTree(BlockTree currentBlock, int cs)
        {
            if (cs == 0)
                System.Console.WriteLine("MaintBlock: Depth = " + currentBlock.mainBlock.Depth + " " + currentBlock.mainBlock.X + " " + currentBlock.mainBlock.Y);
            if (cs == 1)
                System.Console.WriteLine("UL: Depth = " + currentBlock.mainBlock.Depth + " " + currentBlock.mainBlock.X + " " + currentBlock.mainBlock.Y);
            if (cs == 2)
                System.Console.WriteLine("UR: Depth = " + currentBlock.mainBlock.Depth + " " + currentBlock.mainBlock.X + " " + currentBlock.mainBlock.Y);
            if (cs == 3)
                System.Console.WriteLine("DL: Depth = " + currentBlock.mainBlock.Depth + " " + currentBlock.mainBlock.X + " " + currentBlock.mainBlock.Y);
            if (cs == 4)
                System.Console.WriteLine("DR: Depth = " + currentBlock.mainBlock.Depth + " " + currentBlock.mainBlock.X + " " + currentBlock.mainBlock.Y);

            if (currentBlock.ul != null)
            {
                PrintTree(currentBlock.ul, 1);
            }
            if (currentBlock.ur != null)
            {
                PrintTree(currentBlock.ur, 2);
            }
            if (currentBlock.dl != null)
            {
                PrintTree(currentBlock.dl, 3);
            }
            if (currentBlock.dr != null)
            {
                PrintTree(currentBlock.dr, 4);
            }
        }
        //Обход дерева и нахождение для нужных узлов коэффициентов
        public void RoundTree(BlockTree rangeTree, Object.BlockArray[] domainArray, Color[,] classImageColor, int range_block_size)
        {
            if (!(QuadTreeCompression.CheckMonotoneBlock(rangeTree.mainBlock)) && (rangeTree.mainBlock.BlockSize > 2))
            {
                RoundTree(rangeTree.ul, domainArray, classImageColor, rangeTree.mainBlock.BlockSize);
                RoundTree(rangeTree.ur, domainArray, classImageColor, rangeTree.mainBlock.BlockSize);
                RoundTree(rangeTree.dl, domainArray, classImageColor, rangeTree.mainBlock.BlockSize);
                RoundTree(rangeTree.dr, domainArray, classImageColor, rangeTree.mainBlock.BlockSize);
            }
            else
            {
                int current_x = 0;
                int current_y = 0;
                double current_distance = Double.MaxValue;
                double current_shiftR = 0;
                double current_shiftG = 0;
                double current_shiftB = 0;
                bool oneflag = false;
                //System.Console.WriteLine("Depth: " + RangeTree.Depth + " " + RangeTree.MainBlock.BlockSize + " " + DomainArray[CurrentSize].BlockSize);
                int CurrentSize = 0;
                for (int i = 0; i < domainArray.Length; ++i)
                {
                    if ((domainArray[i].BlockSize) == (rangeTree.mainBlock.BlockSize * 2))
                    {
                        CurrentSize = i;
                    }
                }
                for (int k = 0; k < domainArray[CurrentSize].num_width; ++k)
                {
                    for (int l = 0; l < domainArray[CurrentSize].num_height; ++l)
                    {
                        double shiftR = QuadTreeCompression.Shift(rangeTree.mainBlock, domainArray[CurrentSize].Blocks[k, l], rangeTree.mainBlock.BlockSize, 0);
                        double shiftG = QuadTreeCompression.Shift(rangeTree.mainBlock, domainArray[CurrentSize].Blocks[k, l], rangeTree.mainBlock.BlockSize, 1);
                        double shiftB = QuadTreeCompression.Shift(rangeTree.mainBlock, domainArray[CurrentSize].Blocks[k, l], rangeTree.mainBlock.BlockSize, 2);
                        double distance = Metrics.DistanceQuad(classImageColor, rangeTree.mainBlock, domainArray[CurrentSize].Blocks[k, l], rangeTree.mainBlock.BlockSize, shiftR);
                        if (distance < 1000000)
                        {
                            oneflag = true;
                            current_x = k;
                            current_y = l;
                            current_shiftR = shiftR;
                            current_shiftG = shiftG;
                            current_shiftB = shiftB;
                            current_distance = distance;
                            //System.Console.WriteLine(current_x + " " + current_y + " " + current_shiftR + " " + current_shiftG + " " + current_shiftB);
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

                rangeTree.mainBlock.Active = true;
                rangeTree.mainBlock.Coeff.X = current_x;
                rangeTree.mainBlock.Coeff.Y = current_y;
                rangeTree.mainBlock.Coeff.shiftR = (int)current_shiftR;
                rangeTree.mainBlock.Coeff.shiftG = (int)current_shiftG;
                rangeTree.mainBlock.Coeff.shiftB = (int)current_shiftB;
                rangeTree.mainBlock.Coeff.Depth = rangeTree.mainBlock.Depth;
                if (QuadTreeCompression.blockChecker == 0)
                    rangeTree.mainBlock.Coeff.Depth *= (-1);
                ++QuadTreeCompression.blockChecker;
                QuadTreeCompression.listCoeff.Add(rangeTree.mainBlock.Coeff);
                //System.Console.WriteLine(RangeTree.MainBlock.Depth);
                //Запись в файл всех нужные чисел, а так же глубину нахождения узла в дереве
                //Byte[] D = BitConverter.GetBytes(RangeTree.MainBlock.Depth);
                //Byte[] X = BitConverter.GetBytes(RangeTree.MainBlock.Coeff.X);
                //Byte[] Y = BitConverter.GetBytes(RangeTree.MainBlock.Coeff.Y);
                //Byte[] SR = BitConverter.GetBytes(RangeTree.MainBlock.Coeff.shiftR);
                //Byte[] SG = BitConverter.GetBytes(RangeTree.MainBlock.Coeff.shiftG);
                //Byte[] SB = BitConverter.GetBytes(RangeTree.MainBlock.Coeff.shiftB);

                //bw.Write(MyConverter.Convert(D, 1));
                //bw.Write(MyConverter.Convert(X, 1));
                //bw.Write(MyConverter.Convert(Y, 1));
                //bw.Write(MyConverter.Convert(SR, 2));
                //bw.Write(MyConverter.Convert(SG, 2));
                //bw.Write(MyConverter.Convert(SB, 2));
            }
        }
        //Добавить коэффициент в дерево
        public void AddCoeff(BlockTree rangeTree, Object.Coefficients coeff)
        {
            if (coeff.Depth < 0)
            {
                coeff.Depth *= (-1);
            }
            if (coeff.Depth == rangeTree.mainBlock.Depth)
            {
                rangeTree.mainBlock.Active = true;
                rangeTree.mainBlock.Coeff.X = coeff.X;
                rangeTree.mainBlock.Coeff.Y = coeff.Y;
                rangeTree.mainBlock.Coeff.shiftR = coeff.shiftR;
                rangeTree.mainBlock.Coeff.shiftG = coeff.shiftG;
                rangeTree.mainBlock.Coeff.shiftB = coeff.shiftB;
            }
            else
            {
                if (rangeTree.ul != null)
                    AddCoeff(rangeTree.ul, coeff);
                if (rangeTree.ur != null)
                    AddCoeff(rangeTree.ur, coeff);
                if (rangeTree.dl != null)
                    AddCoeff(rangeTree.dl, coeff);
                if (rangeTree.dr != null)
                    AddCoeff(rangeTree.dr, coeff);
            }
        }
        //Построить по дереву изображение
        public void DrawTree(BlockTree rangeTree, Object.BlockArray[] domainArray, Color[,] newImageColor)
        {
            if (rangeTree.mainBlock.Active == false)
            {
                DrawTree(rangeTree.ul, domainArray, newImageColor);
                DrawTree(rangeTree.ur, domainArray, newImageColor);
                DrawTree(rangeTree.dl, domainArray, newImageColor);
                DrawTree(rangeTree.dr, domainArray, newImageColor);
            }
            else
            {
                ++BlockCompression.numLock;
                //System.Console.WriteLine(CompressionClass.NumLock);
                int currentSize = 0;
                for (int i = 0; i < domainArray.Length; ++i)
                {
                    if ((domainArray[i].BlockSize) == (rangeTree.mainBlock.BlockSize * 2))
                    {
                        //System.Console.WriteLine("!!!!");
                        currentSize = i;
                    }
                }              
                Object.Block domainBlock = domainArray[currentSize].Blocks[rangeTree.mainBlock.Coeff.X, rangeTree.mainBlock.Coeff.Y];
                for (int pix_x = 0; pix_x < rangeTree.mainBlock.BlockSize; ++pix_x)
                {
                    for (int pix_y = 0; pix_y < rangeTree.mainBlock.BlockSize; ++pix_y)
                    {
                        Color color = newImageColor[domainBlock.X + (pix_x * 2), domainBlock.Y + (pix_y * 2)];

                        int r = (int)(0.75 * color.R + (rangeTree.mainBlock.Coeff.shiftR));
                        if (r < 0)
                            r = 0;
                        if (r > 255)
                            r = 255;
                        int g = (int)(0.75 * color.G + (rangeTree.mainBlock.Coeff.shiftG));
                        if (g < 0)
                            g = 0;
                        if (g > 255)
                            g = 255;
                        int b = (int)(0.75 * color.B + (rangeTree.mainBlock.Coeff.shiftB));
                        if (b < 0)
                            b = 0;
                        if (b > 255)
                            b = 255;

                        Color Newcolor = Color.FromArgb(r, g, b);
                        newImageColor[rangeTree.mainBlock.X + pix_x, rangeTree.mainBlock.Y + pix_y] = Newcolor;
                    }
                }
            }
        }
    }
}
