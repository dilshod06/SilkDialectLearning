using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
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

    public class VisualTreeHelpers
    {
        /// <summary>
        /// Returns the first ancester of specified type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Returns a specific ancester of an object
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, T lookupItem)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T && current == lookupItem)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Finds an ancestor object by name and type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, string parentName)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (!string.IsNullOrEmpty(parentName))
                {
                    var frameworkElement = current as FrameworkElement;
                    if (current is T && frameworkElement != null && frameworkElement.Name == parentName)
                    {
                        return (T)current;
                    }
                }
                else if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };

            return null;

        }

        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static T FindChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                    else
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
}
