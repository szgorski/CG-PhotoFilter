using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public byte[,] countSmallCubes(int height, int width, int noCubes)
        {
            unsafe
            {
                int index;
                int[] cubeCount = new int[512];
                List<int>[] subcubeLists = new List<int>[512];
                for (int i = 0; i < 512; i++)
                    subcubeLists[i] = new List<int>();

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        index = ((Variables.colors[0, i, j] >> 5) << 6)
                            + ((Variables.colors[1, i, j] >> 5) << 3)
                            + (Variables.colors[2, i, j] >> 5);
                        subcubeLists[index].Add(((Variables.colors[0, i, j] - ((Variables.colors[0, i, j] >> 5) << 5)) << 10)
                            + ((Variables.colors[1, i, j] - ((Variables.colors[1, i, j] >> 5) << 5)) << 5)
                            + (Variables.colors[2, i, j] - ((Variables.colors[2, i, j] >> 5) << 5)));
                        cubeCount[index]++;
                    }
                }

                int[] positions = new int[512];
                for (int i = 0; i < 512; i++)
                    positions[i] = i;

                quickSort(cubeCount, positions, 0, 511);
                reverseArray(positions);

                int[,] colors = new int[3, noCubes];
                int[] subpositions = new int[32768];
                int maxOccurence;
                for (int i = 0; i < noCubes; i++)
                {
                    maxOccurence = 16192;
                    for (int k = 0; k < 32768; k++)
                        subpositions[k] = 0;

                    for (int j = 0; j < subcubeLists[positions[i]].Count; j++)
                        subpositions[subcubeLists[positions[i]][j]]++;

                    for (int k = 0; k < 32768; k++)
                    {
                        if (subpositions[k] > subpositions[maxOccurence])
                            maxOccurence = k;
                    }

                    colors[0, i] = ((positions[i] >> 6) << 5) + (maxOccurence >> 10);
                    colors[1, i] = (((positions[i] >> 3) % 8) << 5) + ((maxOccurence >> 5) % 32);
                    colors[2, i] = ((positions[i] % 8) << 5) + (maxOccurence % 32);
                }

                byte[,] byteColors = new byte[3, noCubes];
                for (int i = 0; i < noCubes; i++)
                {
                    byteColors[0, i] = (byte)colors[0, i];
                    byteColors[1, i] = (byte)colors[1, i];
                    byteColors[2, i] = (byte)colors[2, i];
                }
                return byteColors;
            }
        }

        public byte[,] countBigCubes(int height, int width, int noCubes)
        {
            unsafe
            {
                int index;
                int[] cubeCount = new int[64];
                List<int>[] subcubeLists = new List<int>[64];
                for (int i = 0; i < 64; i++)
                    subcubeLists[i] = new List<int>();

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        index = ((Variables.colors[0, i, j] >> 6) << 4)
                            + ((Variables.colors[1, i, j] >> 6) << 2)
                            + (Variables.colors[2, i, j] >> 6);
                        subcubeLists[index].Add(((Variables.colors[0, i, j] - ((Variables.colors[0, i, j] >> 6) << 6)) << 12)
                            + ((Variables.colors[1, i, j] - ((Variables.colors[1, i, j] >> 6) << 6)) << 6)
                            + (Variables.colors[2, i, j] - ((Variables.colors[2, i, j] >> 6) << 6)));
                        cubeCount[index]++;
                    }
                }

                int[] positions = new int[64];
                for (int i = 0; i < 64; i++)
                    positions[i] = i;

                quickSort(cubeCount, positions, 0, 63);
                reverseArray(positions);

                int[,] colors = new int[3, noCubes];
                int[] subpositions = new int[262144];
                int maxOccurence;
                for (int i = 0; i < noCubes; i++)
                {
                    maxOccurence = 133152;
                    for (int k = 0; k < 262144; k++)
                        subpositions[k] = 0;

                    for (int j = 0; j < subcubeLists[positions[i]].Count; j++)
                        subpositions[subcubeLists[positions[i]][j]]++;

                    for (int k = 0; k < 262144; k++)
                    {
                        if (subpositions[k] > subpositions[maxOccurence])
                            maxOccurence = k;
                    }

                    colors[0, i] = ((positions[i] >> 4) << 6) + (maxOccurence >> 12);
                    colors[1, i] = (((positions[i] >> 2) % 4) << 6) + ((maxOccurence >> 6) % 64);
                    colors[2, i] = ((positions[i] % 4) << 6) + (maxOccurence % 64);
                }

                byte[,] byteColors = new byte[3, noCubes];
                for (int i = 0; i < noCubes; i++)
                {
                    byteColors[0, i] = (byte)colors[0, i];
                    byteColors[1, i] = (byte)colors[1, i];
                    byteColors[2, i] = (byte)colors[2, i];
                }
                return byteColors;
            }
        }

        public void quantizeColors(int height, int width, int noCubes)
        {
            unsafe
            {
                byte[,] colors;
                if (noCubes <= 32)
                    colors = countBigCubes(height, width, noCubes);
                else
                    colors = countSmallCubes(height, width, noCubes);

                listViewColors.BeginUpdate();
                listViewColors.Items.Clear();
                for (int i = 0; i < colors.Length / 3; i++)
                {
                    ListViewItem item = new ListViewItem();
                    item.BackColor = Color.FromArgb(colors[0, i], colors[1, i], colors[2, i]);
                    item.Text = "⠀⠀⠀"; //"⠀⠀⠀⠀⠀";
                    listViewColors.Items.Add(item);
                }
                listViewColors.EndUpdate();

                int errorMinPosition, errorMinValue, errorValue;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        errorMinPosition = 0;
                        errorMinValue = 2147483647;
                        for (int k = 0; k < noCubes; k++)
                        {
                            errorValue = 0;
                            if (Variables.colors[0, i, j] > colors[0, k])                 // alternative - squared distances from the Manhattan norm
                                errorValue += (Variables.colors[0, i, j] - colors[0, k]); // * (Variables.colors[0, i, j] - colors[0, k]);
                            else
                                errorValue += (colors[0, k] - Variables.colors[0, i, j]); // * (colors[0, k] - Variables.colors[0, i, j]);
                            if (Variables.colors[1, i, j] > colors[1, k])
                                errorValue += (Variables.colors[1, i, j] - colors[1, k]); // * (Variables.colors[1, i, j] - colors[1, k]);
                            else
                                errorValue += (colors[1, k] - Variables.colors[1, i, j]); // * (colors[1, k] - Variables.colors[1, i, j]);
                            if (Variables.colors[2, i, j] > colors[2, k])
                                errorValue += (Variables.colors[2, i, j] - colors[2, k]); // * (Variables.colors[2, i, j] - colors[2, k]);
                            else
                                errorValue += (colors[2, k] - Variables.colors[2, i, j]); // * (colors[2, k] - Variables.colors[2, i, j]);

                            if (errorValue < errorMinValue)
                            {
                                errorMinPosition = k;
                                errorMinValue = errorValue;
                            }
                        }
                        Variables.colors[0, i, j] = colors[0, errorMinPosition];
                        Variables.colors[1, i, j] = colors[1, errorMinPosition];
                        Variables.colors[2, i, j] = colors[2, errorMinPosition];
                    }
                }
            }
        }

        public void applyQuantization(int height, int width, int noCubes)
        {
            lockFn();
            quantizeColors(height, width, noCubes);
            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }
    }
}
