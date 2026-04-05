using UnityEngine;

namespace BoomBoxOverhaul
{
    public class BoomBoxOverhaulDspGain : MonoBehaviour
    {
        public float Gain = 1f;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            float gain = Gain;

            if (gain <= 0.0001f)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0f;
                }
                return;
            }
            for (int i = 0; i < data.Length; i++)
            {
                float x = data[i] * gain;

                //softr clip
                data[i] = x / (1f + Mathf.Abs(x));
            }
        }
    }
}