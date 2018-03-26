using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression.Code
{
    class MyConverter
    {
        static public Byte[] Convert(Byte[] ByteArray, int size)
        {
            Byte[] NewByteArray = new Byte[size];
            for (int i = 0; i < size; ++i)
            {
                NewByteArray[i] = ByteArray[i];
            }
            return NewByteArray;
        }
        static public Byte[] ReadByte(Byte[] ByteArray, int start, int end)
        {
            Byte[] NewByteArray = new Byte[end - start];
            for (int i = 0; i < end - start; ++i)
            {
                NewByteArray[i] = ByteArray[start + i];
            }
            return NewByteArray;
        }
     }
}
