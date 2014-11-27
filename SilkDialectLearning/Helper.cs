using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SilkDialectLearning
{
    public class Assistant
    {
        public static Byte[] SoundToByte(string soundPath)
        {
            FileStream fs = new FileStream(soundPath, FileMode.Open);
            System.IO.Stream stream;
            stream = fs;
            Byte[] blob;
            blob = StreamHelper.ReadToEnd(stream);
            return blob;
        }

        public static BitmapImage GetBitmapImageFrom(string fileName)
        {
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(fileName);
            bmpImage.EndInit();
            return bmpImage;
        }

        public static byte[] BitmapToByte(string fileName)
        {
            return BitmapImageToByte(GetBitmapImageFrom(fileName));
        }

        public static byte[] BitmapImageToByte(BitmapImage bitmapImg)
        {
            // Encode the image in JPEG format, then save it to a stream
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImg));
            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            // Put the image in the Meaning table
            byte[] blob = stream.ToArray();
            return blob;
        }

        public static BitmapSource ByteToBitmapSource(byte[] blob)
        {
            byte[] picture = blob;
            MemoryStream stream = new MemoryStream();
            stream.Write(picture, 0, picture.Length);
            PngBitmapDecoder bmpDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            return (BitmapSource)bmpDecoder.Frames[0];
        }

    }

    public class StreamHelper
    {
        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

    }
    
    public class Converter
    {
        public static Byte[] BitmapToByte(Bitmap bitmap)
        {
            System.Drawing.Image bmp = bitmap;
            //Modify bmp
            Byte[] blob;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            blob = StreamHelper.ReadToEnd(stream);
            //blob = new byte[stream.Length];
            stream.Read(blob, 0, (int)stream.Length);
            stream.Close();
            return blob;
        }

        public static Byte[] SoundToByte(string soundPath)
        {
            FileStream fs = new FileStream(soundPath, FileMode.Open);
            System.IO.Stream stream;
            stream = fs;
            Byte[] blob;
            blob = StreamHelper.ReadToEnd(stream);
            return blob;
        }

        public static Bitmap byteArrayToBitmap(Byte[] byteArrayIn)
        {

            MemoryStream ms = new MemoryStream(byteArrayIn, true);
            ms.Write(byteArrayIn, 0, byteArrayIn.Length);
            Bitmap bp = (Bitmap)Bitmap.FromStream(ms);
            //Image returnImage = Image.FromStream(ms);
            return bp;
        }

        public static Stream byteArrayToStream(Byte[] bytes)
        {
            Stream str = new MemoryStream(bytes);
            return str;
        }

        public static System.Windows.Controls.Image ByteToWPFImage(byte[] blob)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(blob, 0, blob.Length);
            stream.Position = 0;

            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            System.Windows.Media.Imaging.BitmapImage bi = new System.Windows.Media.Imaging.BitmapImage();
            bi.BeginInit();

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            System.Windows.Controls.Image image2 = new System.Windows.Controls.Image() { Source = bi };
            return image2;
        }
    }
}
