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
        public Object.Block MainBlock;
        //Узлы блока
        public BlockTree UL;
        public BlockTree UR;
        public BlockTree DL;
        public BlockTree DR;
        //Глубина дерева
        public int Depth = 0;
        public double CurrentDistance = double.MaxValue;
        //Конструктор дерева
        public BlockTree(Object.Block block, int BlockSize)
        {
            MainBlock = block;
            MainBlock.X = block.X;
            MainBlock.Y = block.Y;
            MainBlock.SumR = block.SumR;
            MainBlock.SumG = block.SumG;
            MainBlock.SumB = block.SumB;
            MainBlock.BlockSize = BlockSize;
            MainBlock.Depth = CompressionClass.NumLock;
            if (CompressionClass.NumLock == (-1))
            {
                CompressionClass.NumLock = 2;
            }
            else
            {
                ++CompressionClass.NumLock;
            }
            //Для ранговых блоковы
            if (MainBlock.BlockSize > 2)
            {
                Object.Block tmpBlock = CompressionClass.CreateBlockRange(block.X, block.Y, CompressionClass.ClassImageColor, block.BlockSize / 2, CompressionClass.BrightnessImage);
                UL = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = CompressionClass.CreateBlockRange(block.X + block.BlockSize / 2, block.Y, CompressionClass.ClassImageColor, block.BlockSize / 2, CompressionClass.BrightnessImage);
                UR = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = CompressionClass.CreateBlockRange(block.X, block.Y + block.BlockSize / 2, CompressionClass.ClassImageColor, block.BlockSize / 2, CompressionClass.BrightnessImage);
                DL = new BlockTree(tmpBlock, tmpBlock.BlockSize);

                tmpBlock = CompressionClass.CreateBlockRange(block.X + block.BlockSize / 2, block.Y + block.BlockSize / 2, CompressionClass.ClassImageColor, block.BlockSize / 2, CompressionClass.BrightnessImage);
                DR = new BlockTree(tmpBlock, tmpBlock.BlockSize);
            }
        }
        //Вывод структуры дерева
        public void PrintTree(BlockTree CurrentBlock, int CS)
        {
            if (CS == 0)
                System.Console.WriteLine("MaintBlock: Depth = " + CurrentBlock.MainBlock.Depth + " " + CurrentBlock.MainBlock.X + " " + CurrentBlock.MainBlock.Y);
            if (CS == 1)
                System.Console.WriteLine("UL: Depth = " + CurrentBlock.MainBlock.Depth + " " + CurrentBlock.MainBlock.X + " " + CurrentBlock.MainBlock.Y);
            if (CS == 2)
                System.Console.WriteLine("UR: Depth = " + CurrentBlock.MainBlock.Depth + " " + CurrentBlock.MainBlock.X + " " + CurrentBlock.MainBlock.Y);
            if (CS == 3)
                System.Console.WriteLine("DL: Depth = " + CurrentBlock.MainBlock.Depth + " " + CurrentBlock.MainBlock.X + " " + CurrentBlock.MainBlock.Y);
            if (CS == 4)
                System.Console.WriteLine("DR: Depth = " + CurrentBlock.MainBlock.Depth + " " + CurrentBlock.MainBlock.X + " " + CurrentBlock.MainBlock.Y);

            if (CurrentBlock.UL != null)
            {
                PrintTree(CurrentBlock.UL, 1);
            }
            if (CurrentBlock.UR != null)
            {
                PrintTree(CurrentBlock.UR, 2);
            }
            if (CurrentBlock.DL != null)
            {
                PrintTree(CurrentBlock.DL, 3);
            }
            if (CurrentBlock.DR != null)
            {
                PrintTree(CurrentBlock.DR, 4);
            }
        }
        //Обход дерева и нахождение для нужных узлов коэффициентов
        public void RoundTree(BlockTree RangeTree, Object.BlockArray[] DomainArray, Color[,] ClassImageColor, int range_block_size)
        {
            if (!(CompressionClass.CheckMonotoneBlock(RangeTree.MainBlock)) && (RangeTree.MainBlock.BlockSize > 2))
            {
                RoundTree(RangeTree.UL, DomainArray, ClassImageColor, RangeTree.MainBlock.BlockSize);
                RoundTree(RangeTree.UR, DomainArray, ClassImageColor, RangeTree.MainBlock.BlockSize);
                RoundTree(RangeTree.DL, DomainArray, ClassImageColor, RangeTree.MainBlock.BlockSize);
                RoundTree(RangeTree.DR, DomainArray, ClassImageColor, RangeTree.MainBlock.BlockSize);
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
                for (int i = 0; i < DomainArray.Length; ++i)
                {
                    if ((DomainArray[i].BlockSize) == (RangeTree.MainBlock.BlockSize * 2))
                    {
                        CurrentSize = i;
                    }
                }
                for (int k = 0; k < DomainArray[CurrentSize].num_width; ++k)
                {
                    for (int l = 0; l < DomainArray[CurrentSize].num_height; ++l)
                    {
                        double shiftR = CompressionClass.Shift(RangeTree.MainBlock, DomainArray[CurrentSize].Blocks[k, l], RangeTree.MainBlock.BlockSize, 0);
                        double shiftG = CompressionClass.Shift(RangeTree.MainBlock, DomainArray[CurrentSize].Blocks[k, l], RangeTree.MainBlock.BlockSize, 1);
                        double shiftB = CompressionClass.Shift(RangeTree.MainBlock, DomainArray[CurrentSize].Blocks[k, l], RangeTree.MainBlock.BlockSize, 2);
                        double distance = Metrics.DistanceQuad(ClassImageColor, RangeTree.MainBlock, DomainArray[CurrentSize].Blocks[k, l], RangeTree.MainBlock.BlockSize, shiftR);
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

                RangeTree.MainBlock.Active = true;
                RangeTree.MainBlock.Coeff.X = current_x;
                RangeTree.MainBlock.Coeff.Y = current_y;
                RangeTree.MainBlock.Coeff.shiftR = (int)current_shiftR;
                RangeTree.MainBlock.Coeff.shiftG = (int)current_shiftG;
                RangeTree.MainBlock.Coeff.shiftB = (int)current_shiftB;
                RangeTree.MainBlock.Coeff.Depth = RangeTree.MainBlock.Depth;
                if (CompressionClass.BlockChecker == 0)
                    RangeTree.MainBlock.Coeff.Depth *= (-1);
                ++CompressionClass.BlockChecker;
                CompressionClass.ListCoeff.Add(RangeTree.MainBlock.Coeff);
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
        public void AddCoeff(BlockTree RangeTree, Object.Coefficients Coeff)
        {
            if (Coeff.Depth < 0)
            {
                Coeff.Depth *= (-1);
            }
            if (Coeff.Depth == RangeTree.MainBlock.Depth)
            {
                RangeTree.MainBlock.Active = true;
                RangeTree.MainBlock.Coeff.X = Coeff.X;
                RangeTree.MainBlock.Coeff.Y = Coeff.Y;
                RangeTree.MainBlock.Coeff.shiftR = Coeff.shiftR;
                RangeTree.MainBlock.Coeff.shiftG = Coeff.shiftG;
                RangeTree.MainBlock.Coeff.shiftB = Coeff.shiftB;
            }
            else
            {
                if (RangeTree.UL != null)
                    AddCoeff(RangeTree.UL, Coeff);
                if (RangeTree.UR != null)
                    AddCoeff(RangeTree.UR, Coeff);
                if (RangeTree.DL != null)
                    AddCoeff(RangeTree.DL, Coeff);
                if (RangeTree.DR != null)
                    AddCoeff(RangeTree.DR, Coeff);
            }
        }
        //Построить по дереву изображение
        public void DrawTree(BlockTree RangeTree, Object.BlockArray[] DomainArray, Color[,] NewImageColor)
        {
            if (RangeTree.MainBlock.Active == false)
            {
                DrawTree(RangeTree.UL, DomainArray, NewImageColor);
                DrawTree(RangeTree.UR, DomainArray, NewImageColor);
                DrawTree(RangeTree.DL, DomainArray, NewImageColor);
                DrawTree(RangeTree.DR, DomainArray, NewImageColor);
            }
            else
            {
                ++CompressionClass.NumLock;
                //System.Console.WriteLine(CompressionClass.NumLock);
                int CurrentSize = 0;
                for (int i = 0; i < DomainArray.Length; ++i)
                {
                    if ((DomainArray[i].BlockSize) == (RangeTree.MainBlock.BlockSize * 2))
                    {
                        //System.Console.WriteLine("!!!!");
                        CurrentSize = i;
                    }
                }              
                Object.Block DomainBlock = DomainArray[CurrentSize].Blocks[RangeTree.MainBlock.Coeff.X, RangeTree.MainBlock.Coeff.Y];
                for (int pix_x = 0; pix_x < RangeTree.MainBlock.BlockSize; ++pix_x)
                {
                    for (int pix_y = 0; pix_y < RangeTree.MainBlock.BlockSize; ++pix_y)
                    {
                        Color color = NewImageColor[DomainBlock.X + (pix_x * 2), DomainBlock.Y + (pix_y * 2)];

                        int R = (int)(0.75 * color.R + (RangeTree.MainBlock.Coeff.shiftR));
                        if (R < 0)
                            R = 0;
                        if (R > 255)
                            R = 255;
                        int G = (int)(0.75 * color.G + (RangeTree.MainBlock.Coeff.shiftG));
                        if (G < 0)
                            G = 0;
                        if (G > 255)
                            G = 255;
                        int B = (int)(0.75 * color.B + (RangeTree.MainBlock.Coeff.shiftB));
                        if (B < 0)
                            B = 0;
                        if (B > 255)
                            B = 255;

                        Color Newcolor = Color.FromArgb(R, G, B);
                        NewImageColor[RangeTree.MainBlock.X + pix_x, RangeTree.MainBlock.Y + pix_y] = Newcolor;
                    }
                }
            }
        }
    }
}
