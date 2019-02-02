using System;
namespace Maxwell
{
    public class AnimationInfo
    {
        public float start;
        public float fps;
        public int frames;
        public AnimationInfo(float start1, float fps1, int frames1)
        {
            start = start1;
            fps = fps1;
            frames = frames1 - 1;
        }
    }
}
