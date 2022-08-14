using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public void loadOriginal()
        {
            Bitmap bmp = new Bitmap(Variables.P_Width, Variables.P_Height, Variables.pixelFormat);
            BitmapData bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            {
                for (int i = 0; i < Variables.P_Height; i++)
                {
                    int* row = (int*)((byte*)bits.Scan0 + (i * bits.Stride));

                    if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        for (int j = 0; j < Variables.P_Width; j++)
                        {
                            row[j] = 0xFF << 24 | (Variables.colorsSave[0, i, j] << 16) | (Variables.colorsSave[1, i, j] << 8) | Variables.colorsSave[2, i, j];
                        }
                    }
                    else if (bmp.PixelFormat == PixelFormat.Format32bppRgb || bmp.PixelFormat == PixelFormat.Format24bppRgb)
                    {
                        for (int j = 0; j < Variables.P_Width; j++)
                        {
                            row[j] = (Variables.colorsSave[0, i, j] << 16) | (Variables.colorsSave[1, i, j] << 8) | Variables.colorsSave[2, i, j];
                        }
                    }
                }
            }
            bmp.UnlockBits(bits);

            unsafe
            {
                for (int i = 0; i < Variables.P_Height; i++)
                {
                    for (int j = 0; j < Variables.P_Width; j++)
                    {
                        Variables.colors[0, i, j] = Variables.colorsSave[0, i, j];
                        Variables.colors[1, i, j] = Variables.colorsSave[1, i, j];
                        Variables.colors[2, i, j] = Variables.colorsSave[2, i, j];
                    }
                }
            }
            Variables.bitmap = bmp;
        }

        public void loadModification()
        {
            Bitmap bmp = new Bitmap(Variables.P_Width, Variables.P_Height, Variables.pixelFormat);
            BitmapData bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            {
                for (int i = 0; i < Variables.P_Height; i++)
                {
                    int* row = (int*)((byte*)bits.Scan0 + (i * bits.Stride));

                    if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        for (int j = 0; j < Variables.P_Width; j++)
                        {
                            row[j] = 0xFF << 24 | (Variables.colors[0, i, j] << 16) | (Variables.colors[1, i, j] << 8) | Variables.colors[2, i, j];
                        }
                    }
                    else if (bmp.PixelFormat == PixelFormat.Format32bppRgb || bmp.PixelFormat == PixelFormat.Format24bppRgb)
                    {
                        for (int j = 0; j < Variables.P_Width; j++)
                        {
                            row[j] = (Variables.colors[0, i, j] << 16) | (Variables.colors[1, i, j] << 8) | Variables.colors[2, i, j];
                        }
                    }
                }
            }
            bmp.UnlockBits(bits);
            Variables.bitmap = bmp;
        }

        public void loadPicture()
        {
            if (openPicture.ShowDialog() == DialogResult.OK)
            {
                lockFn();
                Bitmap img = new Bitmap(Image.FromFile(openPicture.FileName));
                Variables.colors = new byte[3, img.Height, img.Width];
                Variables.colorsSave = new byte[3, img.Height, img.Width];

                BitmapData bits = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                    ImageLockMode.ReadOnly, img.PixelFormat);

                unsafe
                {
                    uint conv; // this method allows to work with both RGB and ARGB files
                               // however, the alpha layer is not modified by the program
                    for (int i = 0; i < img.Height; i++)
                    {
                        int* row = (int*)((byte*)bits.Scan0 + (i * bits.Stride));
                        for (int j = 0; j < img.Width; j++)
                        {
                            conv = (uint)row[j];
                            Variables.colorsSave[2, i, j] = (byte)(conv - ((conv >> 8) << 8)); conv >>= 8;
                            Variables.colorsSave[1, i, j] = (byte)(conv - ((conv >> 8) << 8)); conv >>= 8;
                            Variables.colorsSave[0, i, j] = (byte)(conv - ((conv >> 8) << 8)); conv >>= 8;
                        }
                    }
                }
                img.UnlockBits(bits);

                Variables.P_Height = img.Height;
                Variables.P_Width = img.Width;
                Variables.pixelFormat = img.PixelFormat;

                loadOriginal();
                img.Dispose();

                pictureOriginal.Image = Variables.bitmap;
                pictureModified.Image = Variables.bitmap;
                unlockFn();
            }
        }

        public void resetBitmap()
        {
            lockFn();
            Variables.bitmap.Dispose();
            loadOriginal();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        public void saveBitmap()
        {
            if (savePicture.ShowDialog() == DialogResult.OK)
            {
                lockFn();
                pictureModified.Image.Save(savePicture.FileName);
                unlockFn();
            }
        }


    }
}
