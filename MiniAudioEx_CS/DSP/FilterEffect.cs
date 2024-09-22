namespace MiniAudioEx;

public sealed class FilterEffect(FilterType type, float frequency, float q, float gainDB)
    : IAudioEffect
{
    private Filter filter = new(type, frequency, q, gainDB, AudioContext.SampleRate);

    public FilterType Type => filter.Type;

    public float Frequency
    {
        get => filter.Frequency;
        set => filter.Frequency = value;
    }

    public float Q
    {
        get => filter.Q;
        set => filter.Q = value;
    }

    public float GainDB
    {
        get => filter.GainDB;
        set => filter.GainDB = value;
    }

    public void OnProcess(AudioBuffer<float> framesOut, ulong frameCount, int channels)
    {
        filter.Process(framesOut, frameCount, channels);
    }

    public void OnDestroy() {}
}