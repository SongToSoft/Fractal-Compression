using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFractalCompression.Code
{
    static class Post
    {
        static public void Compress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and already compressed files.
                if ((File.GetAttributes(fi.FullName) & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".cmp")
                {
                    // Create the compressed file.
                    using (FileStream outFile =
                            File.Create(fi.FullName + ".cmp"))
                    {
                        using (DeflateStream Compress =
                            new DeflateStream(outFile,
                            CompressionMode.Compress))
                        {
                            // Copy the source file into 
                            // the compression stream.
                            inFile.CopyTo(Compress);

                            Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            fi.Name, fi.Length.ToString(), outFile.Length.ToString());
                        }
                    }
                }
            }
        }
        static public void Decompress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, 
                // for example "doc" from report.doc.cmp.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length
                        - fi.Extension.Length);

                //Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (DeflateStream Decompress = new DeflateStream(inFile,
                        CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        Decompress.CopyTo(outFile);

                        Console.WriteLine("Decompressed: {0}", fi.Name);
                    }
                }
            }
        }
        static public int GetPow(int Num)
        {
            for (int i = 0; i < Num; ++i)
            {
                int Checker = (int)Math.Pow((2), i);
                if (Checker == Num)
                    return i;
            }
            return 1;
        }
    }
}
