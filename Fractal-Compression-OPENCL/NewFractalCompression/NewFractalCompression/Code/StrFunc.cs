using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cloo;
namespace NewFractalCompression.Code
{
    class StrFunc
    {
        static public int x, y, SumR, SumG, SumB, block_size;
        static public string vecSum = @" 
                    __kernel void floatVectorSum(__global float * v1, __global float * v2)
                    {
                        int i = get_global_id(0);
                        v1[i] = v1[i] + v2[i];
                    }";
        static public string StrCreateBlockDom = @"
                    __kernel void CreateBlockDom(int x, int y, __global int * ImageColorR, __global int * ImageColorG, __global int * ImageColorB, int range_block_size, int SumR, int SumG, int SumB)
                    {
                        int i = get_global_id(0);
                        int j = get_global_id(1);
                        SumR = SumR + ImageColorR[(x + i * 2) * range_block_size + (y + j * 2)];
                        SumG = SumG + ImageColorG[(x + i * 2) * range_block_size + (y + j * 2)];
                        SumB = SumB + ImageColorB[(x + i * 2) * range_block_size + (y + j * 2)];
                    }";
        static public string StrNewCreateBlockDom = @"
                    __kernel void NewCreateBlockDom(__global int * ImageColorR, __global int * ImageColorG, __global int * ImageColorB)
                    {
                        int i = get_global_id(0);
                        int j = get_global_id(1);
                        SumR = SumR + ImageColorR[(x + i * 2) * block_size + (y + j * 2)];
                        SumG = SumG + ImageColorG[(x + i * 2) * block_size + (y + j * 2)];
                        SumB = SumB + ImageColorB[(x + i * 2) * block_size + (y + j * 2)];
                    }";
        static public string StrCreateBlockRan = @"
                    __kernel void CreateBlockDom(int x, int y, __global int * ImageColorR, __global int * ImageColorG, __global int * ImageColorB, int range_block_size, int SumR, int SumG, int SumB)
                    {
                        int i = get_global_id(0);
                        int j = get_global_id(1);
                        Block.SumR += ImageColorRBlock.X + i, Block.Y + j];
                        Block.SumG += ImageColorG[Block.X + i, Block.Y + j];
                        Block.SumB += ImageColorB[Block.X + i, Block.Y + j];
                    }";
        //static public void CreateBlockDom(int x, int y, int[] ImageColorR, int[] ImageColorG, int[] ImageColorB,  int range_block_size, int SumR,  int SumG, int SumB)
        //{
        //    int i = get_global_id(0);
        //    int j = i;
        //    SumR += ImageColorR[(x + i * 2) * range_block_size + (y + j * 2)];
        //    SumG += ImageColorG[(x + i * 2) * range_block_size + (y + j * 2)];
        //    SumB += ImageColorB[(x + i * 2) * range_block_size + (y + j * 2)];
        //}
        //public static string StrCreateBlock = @"
        //            _kernel Block CreateBlock(_global int X, _global int Y, _global Color[,] ImageColor, _global int range_block_size, _global bool domain, double[,] Brightness)
        //            {
        //                Block Block = new Block();
        //                Block.X = X;
        //                Block.Y = Y;
        //                Block.SumR = 0;
        //                Block.SumG = 0;
        //                Block.SumB = 0;
        //                Block.Px = 0;
        //                Block.Py = 0;
        //                double BlockBrightness = 0;
        //                if (domain)
        //                {
        //                    for (int i = 0; i < range_block_size; ++i)
        //                    {
        //                        for (int j = 0; j < range_block_size; ++j)
        //                        {
        //                            Block.SumR += ImageColor[Block.X + i * 2, Block.Y + j * 2].R;
        //                            Block.SumG += ImageColor[Block.X + i * 2, Block.Y + j * 2].G;
        //                            Block.SumB += ImageColor[Block.X + i * 2, Block.Y + j * 2].B;
        //                            //Считаем общую яркость для блока
        //                            BlockBrightness += Brightness[Block.X + i * 2, Block.Y + j * 2];
        //                            //Считаем координаты центра масс блока
        //                            Block.Px += i * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
        //                            Block.Py += j * Brightness[Block.X + i * 2, Block.Y + j * 2] - ((range_block_size + 1) / 2);
        //                        }
        //                    }
        //                    Block.Px = Block.Px / BlockBrightness;
        //                    Block.Py = Block.Py / BlockBrightness;
        //                }
        //                else
        //                {
        //                    for (int i = 0; i < range_block_size; ++i)
        //                    {
        //                        for (int j = 0; j < range_block_size; ++j)
        //                        {
        //                            Block.SumR += ImageColor[Block.X + i, Block.Y + j].R;
        //                            Block.SumG += ImageColor[Block.X + i, Block.Y + j].G;
        //                            Block.SumB += ImageColor[Block.X + i, Block.Y + j].B;
        //                            BlockBrightness += Brightness[Block.X + i, Block.Y + j];
        //                            Block.Px += i * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
        //                            Block.Py += j * Brightness[Block.X + i, Block.Y + j] - ((range_block_size + 1) / 2);
        //                        }
        //                    }
        //                    Block.Px = Block.Px / BlockBrightness;
        //                    Block.Py = Block.Py / BlockBrightness;
        //                }
        //                return Block;
        //            }
        //            ";

    }
}
