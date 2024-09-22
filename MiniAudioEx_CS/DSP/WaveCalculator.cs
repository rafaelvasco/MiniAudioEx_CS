namespace MiniAudioEx;

public sealed class WaveCalculator : IWaveCalculator
{
    private delegate float WaveFunc(float phase);
    private WaveType type;
    private WaveFunc waveFunction;

    public WaveType Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
            SetWaveFunc();
        }
    }

    public WaveCalculator(WaveType type)
    {
        this.type = type;
        SetWaveFunc();
    }

    public float GetValue(float phase)
    {
        return waveFunction(phase);
    }

    private void SetWaveFunc()
    {
        switch(type)
        {
            case WaveType.Saw:
                waveFunction = Oscillator.GetSawSample;
                break;
            case WaveType.Sine:
                waveFunction = Oscillator.GetSineSample;
                break;
            case WaveType.Square:
                waveFunction = Oscillator.GetSquareSample;
                break;
            case WaveType.Triangle:
                waveFunction = Oscillator.GetTriangleSample;
                break;
        }
    }
}