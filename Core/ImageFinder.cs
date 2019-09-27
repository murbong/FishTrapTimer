using OpenCvSharp;

namespace FishTrapTimer.Core
{
    public static class ImageFinder
    {
        public static bool ImageFind(Mat sourceMat, Mat findMat, double Threshold)
        {
            using (Mat res = sourceMat.MatchTemplate(findMat, TemplateMatchModes.CCoeffNormed))
            {
                for (int y = 0; y < res.Rows; y++)
                {
                    for (int x = 0; x < res.Cols; x++)
                    {
                        var value = res.At<float>(y, x);

                        if (value > Threshold)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
