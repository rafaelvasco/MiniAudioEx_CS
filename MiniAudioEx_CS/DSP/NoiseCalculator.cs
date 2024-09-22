using System;

namespace MiniAudioEx;

public enum NoiseType
    {
        Brown,
        Pink,
        White
    }

    public sealed class NoiseCalculator : IWaveCalculator
    {
        private delegate float NoiseFunc();
        private NoiseType type;
        private NoiseFunc noiseFunc;
        private float previousValue;
        private static Random random;

        public NoiseType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                previousValue = 0.0f;
                SetNoiseFunc();
            }
        }

        public NoiseCalculator(NoiseType type)
        {
            if(random == null)
                random = new Random();
            this.type = type;
            previousValue = 0.0f;
            SetNoiseFunc();
        }

        public float GetValue()
        {
            return noiseFunc();
        }

        public float GetValue(float phase)
        {
            return noiseFunc();
        }

        private void SetNoiseFunc()
        {
            switch(type)
            {
                case NoiseType.Brown:
                    noiseFunc = GetBrownNoise;
                    break;
                case NoiseType.Pink:
                    noiseFunc = GetPinkNoise;
                    break;
                case NoiseType.White:
                    noiseFunc = GetWhiteNoise;
                    break;
            }
        }

        private float GetBrownNoise()
        {
            float newValue = (float)random.NextDouble() * 2 - 1;
            float output = (previousValue + newValue) / 2.0f;
            previousValue = newValue;
            return output;
        }

        private float GetPinkNoise()
        {
            float value1 = (float)random.NextDouble() * 2 - 1;
            float value2 = (float)random.NextDouble() * 2 - 1;
            float output = (value1 + value2) / 2.0f;
            return output;
        }


        private float GetWhiteNoise()
        {
            float output = (float)random.NextDouble() * 2 - 1;
            return output;
        }
    }