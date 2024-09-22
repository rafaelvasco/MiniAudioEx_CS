using System;

namespace MiniAudioEx;

public interface IWaveCalculator
{
    float GetValue(float phase);
}

public sealed class Wavetable
{
    private readonly float[] data;
    private readonly int length;
    private int index;
    private float phase;
    private float phaseIncrement;
    private readonly float TAU = (float)(2 * Math.PI);

    public Wavetable(IWaveCalculator calculator, int length)
    {
        this.data = new float[length];
        this.length = length;
        this.phase = 0;
        this.phaseIncrement = 0;

        float phaseIncrement = (float)((2 * Math.PI) / length);

        for (int i = 0; i < length; i++)
        {
            data[i] = calculator.GetValue(i * phaseIncrement);
        }
    }

    public Wavetable(float[] data)
    {
        this.data = data;
        this.length = data.Length;
        this.phase = 0;
        this.phaseIncrement = 0;

        //To do:
        //If signal is non periodic (discrete then phase is between 0 and data.Length)
        //Phase increment is calculated like: data.Length * frequency / sampleRate
    }

    /// <summary>
    /// Gets a sample by the given frequency and sample rate.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public float GetValue(float frequency, float sampleRate)
    {
        float phase = this.phase > 0.0f ? (this.phase / TAU) : 0.0f;

        this.phaseIncrement = TAU * frequency / sampleRate;
        this.phase += this.phaseIncrement;
        this.phase %= TAU;

        index = (int)(phase * length);
        float t = phase * length - index;

        int i1 = index % length;
        int i2 = (index+1) % length;

        if(i1 < 0 || i2 < 0)
            return 0.0f;

        float value1 = data[i1];
        float value2 = data[i2];
        return Interpolate(value1, value2, t);
    }

    /// <summary>
    /// Gets a sample by the given phase term.
    /// </summary>
    /// <param name="phase">Phase must be between 0 and 2 * PI</param>
    /// <returns></returns>
    public float GetValueAtPhase(float phase)
    {
        phase = phase > 0.0f ? (phase / TAU) : 0.0f;

        index = (int)(phase * length);
        float t = phase * length - index;

        int i1 = index % length;
        int i2 = (index+1) % length;

        if(i1 < 0 || i2 < 0)
            return 0.0f;

        float value1 = data[i1];
        float value2 = data[i2];
        return Interpolate(value1, value2, t);
    }

    private float Interpolate(float value1, float value2, float t)
    {
        return value1 + (value2 - value1) * t;
    }
}