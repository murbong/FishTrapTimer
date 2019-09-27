using OpenCvSharp;
using System;

namespace FishTrapTimer.Core
{
    public class FindTemplate
    {
        public string Name { get; set; }
        public Mat FindMat { get; set; }
        public double Threshold { get; set; }
        public bool Find { get; set; }
        public bool Switching { get; set; }
        public Action Function { get; set; }
        public RECT BorderRatio { get; set; }
        public FindTemplate(string _name, string _find, double _threshold, RECT _borderRatio)
        {
            Name = _name;
            FindMat = Cv2.ImRead(_find).Threshold(160, 255, ThresholdTypes.Tozero);
            Threshold = _threshold;
            Find = false;
            Switching = false;
            BorderRatio = _borderRatio;
        }

        public void SetAction(Action act)
        {
            Function = act;
        }
    }
}
