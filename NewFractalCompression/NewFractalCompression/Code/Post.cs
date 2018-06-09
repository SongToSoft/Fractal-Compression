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
        static public void GrayGrad(string filename)
        {
            if (!(File.Exists(filename)))
            {
                System.Console.WriteLine("Файла не существует");
                return;
            }
            Bitmap TMPImage = new Bitmap(filename);
            Color[,] TMPImageColor = new Color[TMPImage.Width, TMPImage.Height];
            for (int i = 0; i < TMPImage.Width; ++i)
            {
                for (int j = 0; j < TMPImage.Height; ++j)
                {
                    TMPImageColor[i, j] = TMPImage.GetPixel(i, j);
                }
            }
            Bitmap NewImage = new Bitmap(TMPImage.Width, TMPImage.Height);
            for (int i = 0; i < NewImage.Width; ++i)
            {
                for (int j = 0; j < NewImage.Height; ++j)
                {
                    int RRR = (TMPImageColor[i, j].R + TMPImageColor[i, j].G + TMPImageColor[i, j].B) / 3;
                    if (RRR > 255)
                        RRR = 255;
                    if (RRR < 0)
                        RRR = 0;
                    // System.Console.WriteLine(ImageColor[i, j].R);
                    //System.Console.WriteLine(RRR);
                    Color NewColor = Color.FromArgb(RRR, RRR, RRR);
                    NewImage.SetPixel(i, j, NewColor);
                }
            }
            NewImage.Save(@"C:\Users\Dima\Documents\Фрактальное сжатие\Fractal\NewFractalCompression\NewFractalCompression\Gray Image.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
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
        static public bool EqualImage(Color[,] Image1, Color[,] Image2)
        {
            bool flag = true;
            for (int i = 0; i < Image1.Length; ++i)
            {
                for (int j = 0; j < Image1.Length; ++j)
                {
                    if (Image1[i, j] != Image2[i, j])
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag == false)
                    break;
            }
            return flag;
        }

    }
}
