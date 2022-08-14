using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoFilter
{
    public partial class PhotoFilter : Form
    {
        public PhotoFilter()
        {
            InitializeComponent();

            Variables.customKernel = new int[9, 9];
            setKernel();

            savePicture.DefaultExt = "png";
            savePicture.Filter =
                "PNG files (*.png)|*.png|All files (*.*)|*.*";
            openPicture.Filter =
                "PNG files (*.png)|*.png|JPG files (*.jpg)|*.jpg|All files (*.*)|*.*";
        }

        public delegate void lockDelegate(int height, int width);

        public void simpleLock(int height, int width, lockDelegate fn)
        {
            lockFn();
            fn(height, width);
            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        public void lockFn()
        {
            buttonSave.Enabled = false;
            buttonReset.Enabled = false;
            buttonLoad.Enabled = false;
            buttonEmboss.Enabled = false;
            buttonDetectEdges.Enabled = false;
            buttonSharpen.Enabled = false;
            buttonGaussianBlur.Enabled = false;
            buttonBlur.Enabled = false;
            buttonCorrectGammaUp.Enabled = false;
            buttonCorrectGammaDown.Enabled = false;
            buttonEnhanceContrast.Enabled = false;
            buttonCorrectBrightnessUp.Enabled = false;
            buttonCorrectBrightnessDown.Enabled = false;
            buttonInvert.Enabled = false;
            buttonMedianFilter.Enabled = false;
            buttonApplyConvolution.Enabled = false;
            buttonAtkinson.Enabled = false;
            buttonBurkes.Enabled = false;
            buttonFloydAndSteinberg.Enabled = false;
            buttonSierra.Enabled = false;
            buttonStucky.Enabled = false;
            buttonApplyQuantization.Enabled = false;
            buttonGreyscale.Enabled = false;
            numericAnchorColumn.Enabled = false;
            numericAnchorRow.Enabled = false;
            numericSizeColumns.Enabled = false;
            numericSizeRows.Enabled = false;
        }

        public void unlockFn()
        {
            buttonSave.Enabled = true;
            buttonReset.Enabled = true;
            buttonLoad.Enabled = true;
            buttonEmboss.Enabled = true;
            buttonDetectEdges.Enabled = true;
            buttonSharpen.Enabled = true;
            buttonGaussianBlur.Enabled = true;
            buttonBlur.Enabled = true;
            buttonCorrectGammaUp.Enabled = true;
            buttonCorrectGammaDown.Enabled = true;
            buttonEnhanceContrast.Enabled = true;
            buttonCorrectBrightnessUp.Enabled = true;
            buttonCorrectBrightnessDown.Enabled = true;
            buttonInvert.Enabled = true;
            buttonMedianFilter.Enabled = true;
            buttonApplyConvolution.Enabled = true;
            buttonAtkinson.Enabled = true;
            buttonBurkes.Enabled = true;
            buttonFloydAndSteinberg.Enabled = true;
            buttonSierra.Enabled = true;
            buttonStucky.Enabled = true;
            buttonApplyQuantization.Enabled = true;
            buttonGreyscale.Enabled = true;
            numericAnchorColumn.Enabled = true;
            numericAnchorRow.Enabled = true;
            numericSizeColumns.Enabled = true;
            numericSizeRows.Enabled = true;
        }

        private void buttonInvert_Click(object sender, EventArgs e)
        {
            lockDelegate fn = invert;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonCorrectBrightnessUp_Click(object sender, EventArgs e)
        {
            lockDelegate fn = correctBrightnessUp;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonCorrectBrightnessDown_Click(object sender, EventArgs e)
        {
            lockDelegate fn = correctBrightnessDown;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonEnhanceContrast_Click(object sender, EventArgs e)
        {
            lockDelegate fn = enhanceContrast;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonCorrectGammaUp_Click(object sender, EventArgs e)
        {
            lockDelegate fn = correctGammaUp;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonCorrectGammaDown_Click(object sender, EventArgs e)
        {
            lockDelegate fn = correctGammaDown;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        private void buttonMedianFilter_Click(object sender, EventArgs e)
        {
            medianFilter(Variables.P_Height, Variables.P_Width, 3, 3, 2, 2);
        }


        private void buttonBlur_Click(object sender, EventArgs e)
        {
            blur();
        }

        private void buttonGaussianBlur_Click(object sender, EventArgs e)
        {
            gaussianBlur();
        }

        private void buttonSharpen_Click(object sender, EventArgs e)
        {
            sharpen();
        }

        private void buttonDetectEdges_Click(object sender, EventArgs e)
        {
            detectEdges();
        }

        private void buttonEmboss_Click(object sender, EventArgs e)
        {
            emboss();
        }


        private void buttonLoad_Click(object sender, EventArgs e)
        {
            loadPicture();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            resetBitmap();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveBitmap();
        }


        private void numericSizeRows_ValueChanged(object sender, EventArgs e)
        {
            if (numericSizeRows.Value < numericAnchorRow.Value)
            {
                numericAnchorRow.Value = numericSizeRows.Value;
            }
            numericAnchorRow.Maximum = numericSizeRows.Value;
            updateTable();
        }

        private void numericSizeColumns_ValueChanged(object sender, EventArgs e)
        {
            if (numericSizeColumns.Value < numericAnchorColumn.Value)
            {
                numericAnchorColumn.Value = numericSizeColumns.Value;
            }
            numericAnchorColumn.Maximum = numericSizeColumns.Value;
            updateTable();
        }

        private void checkBoxAutoDivisor_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoDivisor.Checked == true)
            {
                numericUpDownDivisor.Enabled = false;
            }
            else numericUpDownDivisor.Enabled = true;
        }

        private void buttonApplyConvolution_Click(object sender, EventArgs e)
        {
            applyCustomConvolution(Variables.P_Height, Variables.P_Width, (int)numericSizeRows.Value, (int)numericSizeColumns.Value,
                (int)numericAnchorRow.Value, (int)numericAnchorColumn.Value, (int)numericUpDownDivisor.Value, (int)numericUpDownOffset.Value);
        }


        private void buttonApplyQuantization_Click(object sender, EventArgs e)
        {
            applyQuantization(Variables.P_Height, Variables.P_Width, (int)numericUpDownQuantization.Value);
        }

        private void checkBoxYcbCr_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxYcbCr.Checked == true)
            {
                labelDiffusionRed.Text = "Luma";
                labelDiffusionGreen.Text = "Blue-difference";
                labelDiffusionBlue.Text = "Red-difference";
            }
            else
            {
                labelDiffusionRed.Text = "Red";
                labelDiffusionGreen.Text = "Green";
                labelDiffusionBlue.Text = "Blue";
            }
        }


        // might be merged into one function
        private void buttonAtkinson_Click(object sender, EventArgs e)
        {
            float[,] kernel = new float[5, 5]
            {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0.125F, 0.125F},
                    {0, 0.125F, 0.125F, 0.125F, 0},
                    {0, 0, 0.125F, 0, 0}
            };

            lockFn();
            if (checkBoxYcbCr.Checked == true)
            {
                transformRGBToYCbCr(Variables.P_Height, Variables.P_Width);
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);
                transformYCbCrToRGB(Variables.P_Height, Variables.P_Width);
            }
            else
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        private void buttonBurkes_Click(object sender, EventArgs e)
        {
            float[,] kernel = new float[3, 5]
            {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0.25F, 0.125F},
                    {0.0625F, 0.125F, 0.25F, 0.125F, 0.0625F}
            };

            lockFn();
            if (checkBoxYcbCr.Checked == true)
            {
                transformRGBToYCbCr(Variables.P_Height, Variables.P_Width);
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 5, 2, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);
                transformYCbCrToRGB(Variables.P_Height, Variables.P_Width);
            }
            else
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 5, 2, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        private void buttonFloydAndSteinberg_Click(object sender, EventArgs e)
        {
            float[,] kernel = new float[3, 3]
            {
                    {0, 0, 0},
                    {0, 0, 0.4375F},
                    {0.1875F, 0.3125F, 0.0625F}
            };

            lockFn();
            if (checkBoxYcbCr.Checked == true)
            {
                transformRGBToYCbCr(Variables.P_Height, Variables.P_Width);
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);
                transformYCbCrToRGB(Variables.P_Height, Variables.P_Width);
            }
            else
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 3, 3, 2, 2, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        private void buttonSierra_Click(object sender, EventArgs e)
        {
            float[,] kernel = new float[5, 5]
            {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0.15625F, 0.09375F},
                    {0.0625F, 0.125F, 0.15625F, 0.125F, 0.0625F},
                    {0, 0.0625F, 0.09375F, 0.0625F, 0}
            };

            lockFn();
            if (checkBoxYcbCr.Checked == true)
            {
                transformRGBToYCbCr(Variables.P_Height, Variables.P_Width);
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);
                transformYCbCrToRGB(Variables.P_Height, Variables.P_Width);
            }
            else
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        private void buttonStucky_Click(object sender, EventArgs e)
        {
            float[,] kernel = new float[5, 5]
            {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 8F / 42, 4F / 42},
                    {2F / 42, 4F / 42, 8F / 42, 4F / 42, 2F / 42},
                    {1F / 42, 2F / 42, 4F / 42, 2F / 42, 1F / 42}
            };

            lockFn();
            if (checkBoxYcbCr.Checked == true)
            {
                transformRGBToYCbCr(Variables.P_Height, Variables.P_Width);
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);
                transformYCbCrToRGB(Variables.P_Height, Variables.P_Width);
            }
            else
                errorDiffusionFn(Variables.P_Height, Variables.P_Width, kernel, 5, 5, 3, 3, (int)numericUpDownDiffusionRed.Value,
                    (int)numericUpDownDiffusionGreen.Value, (int)numericUpDownDiffusionBlue.Value);

            Variables.bitmap.Dispose();
            loadModification();
            pictureModified.Image = Variables.bitmap;
            unlockFn();
        }

        private void buttonGreyscale_Click(object sender, EventArgs e)
        {
            lockDelegate fn = transformToGreyscale;
            simpleLock(Variables.P_Height, Variables.P_Width, fn);
        }

        public void bindDiffusion(NumericUpDown numeric)
        {
            numericUpDownDiffusionRed.Value = numeric.Value;
            numericUpDownDiffusionGreen.Value = numeric.Value;
            numericUpDownDiffusionBlue.Value = numeric.Value;

        }

        private void numericUpDownDiffusionRed_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxGreyscale.Checked == true)
                bindDiffusion(numericUpDownDiffusionRed);
        }

        private void numericUpDownDiffusionGreen_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxGreyscale.Checked == true)
                bindDiffusion(numericUpDownDiffusionGreen);
        }

        private void numericUpDownDiffusionBlue_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxGreyscale.Checked == true)
                bindDiffusion(numericUpDownDiffusionBlue);
        }
    }

    static class Variables
    {
        public static byte BC_Constant = 5;             // brightness correction constant (>= 1)
        public static double GCDown_Constant = 1.05;    // decoding gamma correction contrast (> 1.0)
        public static double GCUp_Constant = 0.95;      // encoding gamma correction contrast (< 1.0)
        public static double CE_Constant = 1.1;         // contrast enhancement slope (> 1.0)
        public static int P_Height;                     // picture height
        public static int P_Width;                      // picture width
        public static byte[,,] colors;
        public static byte[,,] colorsSave;
        public static Bitmap bitmap;
        public static PixelFormat pixelFormat;
        public static int[,] customKernel;
    }
}