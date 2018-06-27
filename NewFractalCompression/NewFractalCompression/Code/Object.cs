using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression.Code
{
    class Object
    {
        public struct BlockArray
        {
            public Block[,] Blocks;
            public int num_width;
            public int num_height;
            public int BlockSize;
        }
        public struct Block
        {
            public int X;
            public int Y;
            //public int position;
            public int SumR;
            public int SumG;
            public int SumB;
            //Для разбиения на доп блоки
            public int BlockSize;
            //Эти коэффициенты использовались в одной из метрик
            public int SumR2;
            public int SumG2;
            public int SumB2;
            //Коэффиценты для классификации
            public double Px;
            public double Py;
            public Coefficients Coeff;
            public bool Active;
            public int Depth;
        }
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
            public int Depth;
        }
    }
}
