namespace MiniAudioEx;

 public sealed class FMGenerator : IAudioGenerator
    {
        private Oscillator carrier;
        private ConcurrentList<Oscillator> operators;

        public Oscillator Carrier => carrier;

        /// <summary>
        /// Gets the number of operators.
        /// </summary>
        /// <value></value>
        public int Count => operators.Count;

        public Oscillator this[int index]
        {
            get
            {
                if ((uint)index >= (uint)operators.Count)
                    new System.IndexOutOfRangeException();
                return operators[index];
            }
        }

        public FMGenerator(WaveType type, float frequency, float amplitude)
        {
            carrier = new Oscillator(type, frequency, amplitude);
            operators = new ConcurrentList<Oscillator>();
        }
        
        /// <summary>
        /// /// Resets the phase.
        /// </summary>
        public void Reset()
        {
            carrier.Reset();
            for(int i = 0; i < operators.Count; i++)
            {
                operators[i].Reset();
            }
        }

        public void AddOperator(WaveType type, float frequency, float depth)
        {
            operators.Add(new Oscillator(type, frequency, depth));
        }

        public void RemoveOperator(int index)
        {
            if(index >= 0 && index < operators.Count)
            {
                var target = operators[index];
                operators.Remove(target);
            }
        }

        public void OnGenerate(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;

            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = GetModulatedSample();

                for(int j = 0; j < channels; j++)
                {
                    framesOut[i+j] = sample;
                }
            }
        }

        public void OnDestroy() {}

        private float GetModulatedSample()
        {
            float modulationSum = 0.0f;

            for (int i = 0; i < operators.Count; i++)
            {
                modulationSum += operators[i].GetValue();
            }

            return carrier.GetModulatedValue(modulationSum);
        }
    }