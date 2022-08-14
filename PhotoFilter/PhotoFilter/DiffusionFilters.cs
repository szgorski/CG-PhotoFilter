using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public void errorDiffusionFn(int height, int width, float[,] kernel, int kHeight, int kWidth,
            int aRow, int aColumn, int noRed, int noGreen, int noBlue)        // kHeight - kernel height
        {    // aRow - anchor's row                                                                
            unsafe
            {
                float[,,] colorsCopy = new float[3, height, width];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        colorsCopy[0, i, j] = Variables.colors[0, i, j];
                        colorsCopy[1, i, j] = Variables.colors[1, i, j];
                        colorsCopy[2, i, j] = Variables.colors[2, i, j];
                    }
                }

                float[] approximation = new float[3];
                float[] error = new float[3];
                float[] errorBase = new float[3] { 256F / (noRed - 1), 256F / (noGreen - 1), 256F / (noBlue - 1) };
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        // quantization
                        approximation[0] = errorBase[0] * fastRound(colorsCopy[0, i, j] / errorBase[0]);
                        approximation[1] = errorBase[1] * fastRound(colorsCopy[1, i, j] / errorBase[1]);
                        approximation[2] = errorBase[2] * fastRound(colorsCopy[2, i, j] / errorBase[2]);

                        error[0] = colorsCopy[0, i, j] - approximation[0];
                        error[1] = colorsCopy[1, i, j] - approximation[1];
                        error[2] = colorsCopy[2, i, j] - approximation[2];

                        colorsCopy[0, i, j] = approximation[0];
                        colorsCopy[1, i, j] = approximation[1];
                        colorsCopy[2, i, j] = approximation[2];

                        // adding error
                        for (int ki = 0; ki < fastMin(height - i, kHeight - aRow + 1); ki++)
                        {     // assuming non-zero values olny after the anchor
                            for (int kj = fastMax(1 - aColumn, -j); kj < fastMin(kWidth - aColumn + 1, width - j); kj++)
                            {
                                if (kernel[aRow + ki - 1, aColumn + kj - 1] != 0) // assuming no rounding error 
                                {                                                 // (in future modifications)
                                    colorsCopy[0, i + ki, j + kj] += error[0] * kernel[aRow + ki - 1, aColumn + kj - 1];
                                    colorsCopy[1, i + ki, j + kj] += error[1] * kernel[aRow + ki - 1, aColumn + kj - 1];
                                    colorsCopy[2, i + ki, j + kj] += error[2] * kernel[aRow + ki - 1, aColumn + kj - 1];
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)fastMin(255, fastMax(0, fastRound(colorsCopy[0, i, j])));
                        Variables.colors[1, i, j] = (byte)fastMin(255, fastMax(0, fastRound(colorsCopy[1, i, j])));
                        Variables.colors[2, i, j] = (byte)fastMin(255, fastMax(0, fastRound(colorsCopy[2, i, j])));
                    }
                }
            }
        }

        public void transformRGBToYCbCr(int height, int width)
        {
            unsafe
            {
                byte Y, Cb, Cr;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Y = (byte)fastMin(255, fastMax(0, fastRound(0.299F * Variables.colors[0, i, j]
                            + 0.587F * Variables.colors[1, i, j] + 0.114F * Variables.colors[2, i, j])));
                        Cb = (byte)fastMin(255, fastMax(0, fastRound(128F - 0.168736F * Variables.colors[0, i, j]
                            - 0.331264F * Variables.colors[1, i, j] + 0.5F * Variables.colors[2, i, j])));
                        Cr = (byte)fastMin(255, fastMax(0, fastRound(128F + 0.5F * Variables.colors[0, i, j]
                            - 0.418688F * Variables.colors[1, i, j] - 0.081312F * Variables.colors[2, i, j])));

                        Variables.colors[0, i, j] = Y;
                        Variables.colors[1, i, j] = Cb;
                        Variables.colors[2, i, j] = Cr;
                    }
                }
            }
        }

        public void transformYCbCrToRGB(int height, int width)
        {
            unsafe
            {
                byte R, G, B;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        R = (byte)fastMin(255, fastMax(0, fastRound(1.402F * (Variables.colors[2, i, j] - 128)
                            + Variables.colors[0, i, j])));
                        G = (byte)fastMin(255, fastMax(0, fastRound(-0.34414F * (Variables.colors[1, i, j] - 128)
                            - 0.71414F * (Variables.colors[2, i, j] - 128) + Variables.colors[0, i, j])));
                        B = (byte)fastMin(255, fastMax(0, fastRound(1.772F * (Variables.colors[1, i, j] - 128)
                            + Variables.colors[0, i, j])));

                        Variables.colors[0, i, j] = R;
                        Variables.colors[1, i, j] = G;
                        Variables.colors[2, i, j] = B;
                    }
                }

            }
        }

        public void transformToGreyscale(int height, int width)
        {
            unsafe
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        // CIE 1931: Y = 0.2126 * R + 0.7152 * G + 0.0722 * B
                        byte shade = (byte)(fastMin(255, fastRound(0.2126F * Variables.colors[0, i, j]
                            + 0.7152F * Variables.colors[1, i, j] + 0.0722F * Variables.colors[2, i, j])));
                        Variables.colors[0, i, j] = shade;
                        Variables.colors[1, i, j] = shade;
                        Variables.colors[2, i, j] = shade;
                    }
                }
            }
        }
    }
}
