using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NewFractalCompression.Code.CompressionClass;

namespace NewFractalCompression.Code
{
    class Metrics
    {
        //Варианты подсчета дистанции в классической метрике
        static public double One(double RangeValue, double DomainValue, double shift)
        {
            return (Math.Pow((RangeValue * shift - 0.75 * DomainValue), 2));
        }
        static public double Two(double RangeValue, double DomainValue, double shift)
        {
            return (Math.Pow((RangeValue + shift - 0.75 * DomainValue), 2));
        }
        //Определение метрики между двумя блоками
        //Один из вариантов метрики, предложенный Шарабайко, который по его версии должен ускорить подсчёт 
        static public double DistanceShar(Color[,] RangeImageColor, Color[,] DomainImageColor, Block RangeBlock, Block DomainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            double Sum = 0;
            if (flag == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].R;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].R;
                        Sum += RangeValue + DomainValue;
                    }
                }
                distance = (0.75 * 0.75) * DomainBlock.SumR2 + (range_block_size * range_block_size) * (shift * shift) + DomainBlock.SumR2 - (2 * 0.75) * Sum + (2 * 0.75 * shift) * DomainBlock.SumR - (2 * shift * RangeBlock.SumR);
                return distance;
            }
            if (flag == 1)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].G;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].G;
                        Sum += RangeValue + DomainValue;
                    }
                }
                distance = (0.75 * 0.75) * DomainBlock.SumG2 + (range_block_size * range_block_size) * (shift * shift) + DomainBlock.SumG2 - (2 * 0.75) * Sum + (2 * 0.75 * shift) * DomainBlock.SumG - (2 * shift * RangeBlock.SumG);
                return distance;
            }
            if (flag == 2)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        RangeValue = RangeImageColor[RangeBlock.X + i, RangeBlock.Y + j].B;
                        DomainValue = DomainImageColor[DomainBlock.X + (i * 2), DomainBlock.Y + (j * 2)].B;
                        Sum += RangeValue + DomainValue;
                    }
                }
                distance = (0.75 * 0.75) * DomainBlock.SumB2 + (range_block_size * range_block_size) * (shift * shift) + DomainBlock.SumB2 - (2 * 0.75) * Sum + (2 * 0.75 * shift) * DomainBlock.SumB - (2 * shift * RangeBlock.SumB);
                return distance;
            }
            return distance;
        }
        //Классический вариант метрики
        static public double Distance(Color[,] RangeImageColor, Color[,] DomainImageColor, Block RangeBlock, Block DomainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double RangeValue = 0;
            double DomainValue = 0;
            if (flag == 0)
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
            if (flag == 1)
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
            if (flag == 2)
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
    }
}
