using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NewFractalCompression.Code.BlockCompression;

namespace NewFractalCompression.Code
{
    class Metrics
    {
        //Варианты подсчета дистанции в классической метрике
        static public double One(double rangeValue, double domainValue, double shift)
        {
            return (Math.Pow((rangeValue * shift - AbstractCompression.u * domainValue), 2));
        }

        static public double Two(double rangeValue, double domainValue, double shift)
        {
            return (Math.Pow((rangeValue + shift - AbstractCompression.u * domainValue), 2));
        }

        //Определение метрики между двумя блоками
        //Один из вариантов метрики, предложенный Шарабайко, который по его версии должен ускорить подсчёт 
        static public double DistanceShar(Color[,] rangeImageColor, Color[,] domainImageColor, Object.Block rangeBlock, Object.Block domainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double rangeValue = 0;
            double domainValue = 0;
            double sum = 0;
            if (flag == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].R;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].R;
                        sum += rangeValue + domainValue;
                    }
                }
                distance = (0.75 * 0.75) * domainBlock.SumR2 + (range_block_size * range_block_size) * (shift * shift) + domainBlock.SumR2 - (2 * 0.75) * sum + (2 * 0.75 * shift) * domainBlock.SumR - (2 * shift * rangeBlock.SumR);
                return distance;
            }
            if (flag == 1)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].G;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].G;
                        sum += rangeValue + domainValue;
                    }
                }
                distance = (0.75 * 0.75) * domainBlock.SumG2 + (range_block_size * range_block_size) * (shift * shift) + domainBlock.SumG2 - (2 * 0.75) * sum + (2 * 0.75 * shift) * domainBlock.SumG - (2 * shift * rangeBlock.SumG);
                return distance;
            }
            if (flag == 2)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].B;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].B;
                        sum += rangeValue + domainValue;
                    }
                }
                distance = (0.75 * 0.75) * domainBlock.SumB2 + (range_block_size * range_block_size) * (shift * shift) + domainBlock.SumB2 - (2 * 0.75) * sum + (2 * 0.75 * shift) * domainBlock.SumB - (2 * shift * rangeBlock.SumB);
                return distance;
            }
            return distance;
        }

        //Классический вариант метрики
        static public double DistanceClass(Color[,] rangeImageColor, Color[,] domainImageColor, Object.Block rangeBlock, Object.Block domainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double rangeValue = 0;
            double domainValue = 0;
            if (flag == 0)
            {
                for (int i = 0; i < range_block_size; ++i)
                {
                    for (int j = 0; j < range_block_size; ++j)
                    {
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].R;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].R;
                        distance += Metrics.Two(rangeValue, domainValue, shift);
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
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].G;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].G;
                        distance += Metrics.Two(rangeValue, domainValue, shift);
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
                        rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].B;
                        domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].B;
                        distance += Metrics.Two(rangeValue, domainValue, shift);
                    }
                }
                return distance;
            }
            return distance;
        }

        static public double PSNR(Color[,] rangeImageColor, Color[,] domainImageColor, Object.Block rangeBlock, Object.Block domainBlock, int range_block_size, double shift, int flag)
        {
            double distance = 0;
            double rangeValue = 0;
            double domainValue = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    rangeValue = rangeImageColor[rangeBlock.X + i, rangeBlock.Y + j].B;
                    domainValue = domainImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].B;
                    distance += Math.Pow((rangeValue - domainValue), 2);
                }
            }
            distance = Math.Sqrt(distance) / (range_block_size * range_block_size);
            distance = -20 * Math.Log(distance);
            return distance;
        }

        static public double DistanceQuad(Color[,] classImageColor, Object.Block rangeBlock, Object.Block domainBlock, int range_block_size, double shift)
        {
            double distance = 0;
            double rangeValue = 0;
            double domainValue = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    rangeValue = classImageColor[rangeBlock.X + i, rangeBlock.Y + j].R;
                    domainValue = classImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].R;
                    distance += Metrics.Two(rangeValue, domainValue, shift);
                }
            }
            return distance;
        }
    }
}
