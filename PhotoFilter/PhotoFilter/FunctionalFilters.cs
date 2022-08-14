using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public void invert(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)(255 - Variables.colors[0, i, j]);
                        Variables.colors[1, i, j] = (byte)(255 - Variables.colors[1, i, j]);
                        Variables.colors[2, i, j] = (byte)(255 - Variables.colors[2, i, j]);
                    }
                }
            }
        }

        public void correctBrightnessUp(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)fastMin(255, Variables.colors[0, i, j] + Variables.BC_Constant);
                        Variables.colors[1, i, j] = (byte)fastMin(255, Variables.colors[1, i, j] + Variables.BC_Constant);
                        Variables.colors[2, i, j] = (byte)fastMin(255, Variables.colors[2, i, j] + Variables.BC_Constant);
                    }
                }
            }
        }

        public void correctBrightnessDown(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)fastMax(0, Variables.colors[0, i, j] - Variables.BC_Constant);
                        Variables.colors[1, i, j] = (byte)fastMax(0, Variables.colors[1, i, j] - Variables.BC_Constant);
                        Variables.colors[2, i, j] = (byte)fastMax(0, Variables.colors[2, i, j] - Variables.BC_Constant);
                    }
                }
            }
        }

        public void enhanceContrast(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)fastMin(255, fastMax(0, (int)((Variables.colors[0, i, j] - 127) * Variables.CE_Constant + 127)));
                        Variables.colors[1, i, j] = (byte)fastMin(255, fastMax(0, (int)((Variables.colors[1, i, j] - 127) * Variables.CE_Constant + 127)));
                        Variables.colors[2, i, j] = (byte)fastMin(255, fastMax(0, (int)((Variables.colors[2, i, j] - 127) * Variables.CE_Constant + 127)));
                    }
                }
            }
        }

        public void correctGammaUp(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[0, i, j] / 255, Variables.GCUp_Constant));
                        Variables.colors[1, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[1, i, j] / 255, Variables.GCUp_Constant));
                        Variables.colors[2, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[2, i, j] / 255, Variables.GCUp_Constant));
                    }
                }
            }
        }

        public void correctGammaDown(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[0, i, j] / 255, Variables.GCDown_Constant));
                        Variables.colors[1, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[1, i, j] / 255, Variables.GCDown_Constant));
                        Variables.colors[2, i, j] = (byte)(255 * Math.Pow((double)Variables.colors[2, i, j] / 255, Variables.GCDown_Constant));
                    }
                }
            }
        }

        public void medianFilter(int height, int width, int kHeight, int kWidth, int aRow, int aColumn)
        {                                                // kHeight - kernel height, aRow - anchor's row
            lockFn();
            unsafe
            {
                int counter;
                byte[][] medianTable = new byte[3][];
                medianTable[0] = new byte[kHeight * kWidth];
                medianTable[1] = new byte[kHeight * kWidth];
                medianTable[2] = new byte[kHeight * kWidth];
                byte[,,] colorsCopy = new byte[3, height, width];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        counter = 0;
                        for (int ki = fastMax(0, aRow - 1 - i); ki < fastMin(kHeight, height - i + aRow - 1); ki++)
                        {
                            for (int kj = fastMax(0, aColumn - 1 - j); kj < fastMin(kWidth, width - j + aColumn - 1); kj++)
                            {
                                medianTable[0][counter] = Variables.colors[0, i + ki - aRow + 1, j + kj - aColumn + 1];
                                medianTable[1][counter] = Variables.colors[1, i + ki - aRow + 1, j + kj - aColumn + 1];
                                medianTable[2][counter] = Variables.colors[2, i + ki - aRow + 1, j + kj - aColumn + 1];
                                counter++;
                            }
                        }
                        Array.Sort(medianTable[0], 0, counter);
                        Array.Sort(medianTable[1], 0, counter);
                        Array.Sort(medianTable[2], 0, counter);

                        colorsCopy[0, i, j] = medianTable[0][counter / 2];
                        colorsCopy[1, i, j] = medianTable[1][counter / 2];
                        colorsCopy[2, i, j] = medianTable[2][counter / 2];
                    }
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = colorsCopy[0, i, j];
                        Variables.colors[1, i, j] = colorsCopy[1, i, j];
                        Variables.colors[2, i, j] = colorsCopy[2, i, j];
                    }
                }
            }

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }
    }
}
