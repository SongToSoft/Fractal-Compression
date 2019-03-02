using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression.Code
{
    class MyConverter
    {
        static public Byte[] Convert(Byte[] byteArray, int size)
        {
            Byte[] newByteArray = new Byte[size];
            for (int i = 0; i < size; ++i)
            {
                newByteArray[i] = byteArray[i];
            }
            return newByteArray;
        }

        static public Byte[] ReadByte(Byte[] byteArray, int start, int end)
        {
            Byte[] newByteArray = new Byte[end - start];
            for (int i = 0; i < end - start; ++i)
            {
                newByteArray[i] = byteArray[start + i];
            }
            return newByteArray;
        }
     }
}
