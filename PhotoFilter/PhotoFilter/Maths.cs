using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoFilter
{
    public partial class PhotoFilter
    {
        public int fastMin(int a, int b)
        {
            unsafe
            {
                if (a > b) return b;
                else return a;
            }
        }

        public int fastMax(int a, int b)
        {
            unsafe
            {
                if (a > b) return a;
                else return b;
            }
        }

        public int fastRound(float number)
        {
            int floor = (int)number;
            if (number - floor < 0.5)
                return floor;
            else
                return floor + 1;
        }

        public void reverseArray(int[] array)
        {
            unsafe
            {
                for (int i = 0; i < array.Length / 2; i++)
                {
                    int tmp = array[i];
                    array[i] = array[array.Length - i - 1];
                    array[array.Length - i - 1] = tmp;
                }
            }
        }

        public void quickSort(int[] cubeCount, int[] positions, int left, int right)
        {
            unsafe
            {
                int i = left;
                int j = right;
                int pivot = cubeCount[positions[(left + right) / 2]];
                while (i < j)
                {
                    while (cubeCount[positions[i]] < pivot) i++;
                    while (cubeCount[positions[j]] > pivot) j--;
                    if (i <= j)
                    {
                        int tmp = positions[i];
                        positions[i++] = positions[j];
                        positions[j--] = tmp;
                    }
                }
                if (left < j) quickSort(cubeCount, positions, left, j);
                if (i < right) quickSort(cubeCount, positions, i, right);
            }
        }
    }
}
