using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public void convolutionFn(int height, int width, int[,] kernel,
            int kHeight, int kWidth, int aRow, int aColumn, int customWeight, int offset)
        {                                                                // kHeight - kernel height, aRow - anchor's row
            lockFn();

            unsafe
            {
                int[,,] colorsCopy = new int[3, height, width];
                int weight;

                if (customWeight != 0)
                {
                    weight = customWeight;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            for (int ki = fastMax(0, aRow - 1 - i); ki < fastMin(kHeight, height - i + aRow - 1); ki++)
                            {
                                for (int kj = fastMax(0, aColumn - 1 - j); kj < fastMin(kWidth, width - j + aColumn - 1); kj++)
                                {
                                    colorsCopy[0, i, j] += Variables.colors[0, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                    colorsCopy[1, i, j] += Variables.colors[1, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                    colorsCopy[2, i, j] += Variables.colors[2, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                }
                            }
                            colorsCopy[0, i, j] /= weight;
                            colorsCopy[1, i, j] /= weight;
                            colorsCopy[2, i, j] /= weight;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            weight = 0;
                            for (int ki = fastMax(0, aRow - 1 - i); ki < fastMin(kHeight, height - i + aRow - 1); ki++)
                            {
                                for (int kj = fastMax(0, aColumn - 1 - j); kj < fastMin(kWidth, width - j + aColumn - 1); kj++)
                                {
                                    colorsCopy[0, i, j] += Variables.colors[0, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                    colorsCopy[1, i, j] += Variables.colors[1, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                    colorsCopy[2, i, j] += Variables.colors[2, i + ki - aRow + 1, j + kj - aColumn + 1] * kernel[ki, kj];
                                    weight += kernel[ki, kj];
                                }
                            }
                            if (weight == 0) weight = 1;
                            colorsCopy[0, i, j] = offset + colorsCopy[0, i, j] / weight;
                            colorsCopy[1, i, j] = offset + colorsCopy[1, i, j] / weight;
                            colorsCopy[2, i, j] = offset + colorsCopy[2, i, j] / weight;
                        }
                    }
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Variables.colors[0, i, j] = (byte)fastMin(255, fastMax(0, colorsCopy[0, i, j]));
                        Variables.colors[1, i, j] = (byte)fastMin(255, fastMax(0, colorsCopy[1, i, j]));
                        Variables.colors[2, i, j] = (byte)fastMin(255, fastMax(0, colorsCopy[2, i, j]));
                    }
                }
            }

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        public void blur()
        {
            int[,] kernel = new int[3, 3]
                {
                    {1, 1, 1},
                    {1, 1, 1},
                    {1, 1, 1}
                };
            convolutionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, 0, 0);
        }

        public void gaussianBlur()
        {
            int[,] kernel = new int[3, 3]
                {
                    {0, 1, 0},
                    {1, 4, 1},
                    {0, 1, 0}
                };
            convolutionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, 0, 0);
        }

        public void sharpen()
        {
            int[,] kernel = new int[3, 3]
                {
                    {0, -1, 0},
                    {-1, 5, -1},
                    {0, -1, 0}
                };
            convolutionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, 0, 0);
        }

        public void detectEdges()
        {
            int[,] kernel = new int[3, 3]
                {
                    {-1, 0, 0},
                    {0, 1, 0},
                    {0, 0, 0}
                };
            convolutionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, 0, 127);
        }

        public void emboss()
        {
            int[,] kernel = new int[3, 3]
                {
                    {-1, -1, 0},
                    {-1, 1, 1},
                    {0, 1, 1}
                };
            convolutionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, 0, 0);
        }

        public void applyCustomConvolution(int height, int width, int kHeight, int kWidth, int aRow, int aColumn, int customWeight, int offset)
        {
            lockFn();
            setKernel();
            updateDivisor();

            int[,] copyKernel = new int[(int)numericSizeRows.Value, (int)numericSizeColumns.Value];
            for (int i = 0; i < (int)numericSizeRows.Value; i++)
            {
                for (int j = 0; j < (int)numericSizeColumns.Value; j++)
                {
                    copyKernel[i, j] = Variables.customKernel[i, j];
                }
            }

            convolutionFn(height, width, copyKernel, kHeight, kWidth, aRow, aColumn, customWeight, offset);
        }

        public void setKernel()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    NumericUpDown c = (NumericUpDown)tableLayoutPanel.GetControlFromPosition(j, i);
                    Variables.customKernel[i, j] = (int)c.Value;
                }
            }
        }

        public void updateTable()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    NumericUpDown c = (NumericUpDown)tableLayoutPanel.GetControlFromPosition(j, i);
                    if (i < numericSizeRows.Value && j < numericSizeColumns.Value) c.Enabled = true;
                    else
                    {
                        c.Enabled = false;
                        c.Value = 0;
                    }
                }
            }
        }

        public void updateDivisor()
        {
            int divisor = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    divisor += Variables.customKernel[i, j];
                }
            }
            if (divisor <= 0) numericUpDownDivisor.Value = 1;
            else numericUpDownDivisor.Value = divisor;
        }
    }
}
