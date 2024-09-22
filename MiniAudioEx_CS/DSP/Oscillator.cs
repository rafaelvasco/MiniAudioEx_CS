using System;

namespace MiniAudioEx;

public enum WaveType
{
    Sine,
    Square,
    Triangle,
    Saw
}

public sealed class Oscillator
{
    private delegate float WaveFunction(float phase);
    private WaveFunction waveFunc;
    private WaveType type;
    private float frequency;
    private float amplitude;
    private float phase;
    private float phaseIncrement;
    private static readonly float TAU = (float)(2 * Math.PI);

    public WaveType Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
            SetWaveFunction();
        }
    }

    public float Frequency
    {
        get
        {
            return frequency;
        }
        set
        {
            frequency = value;
            SetPhaseIncrement();
        }
    }

    public float Amplitude
    {
        get
        {
            return amplitude;
        }
        set
        {
            amplitude = value;
        }
    }

    public float Phase
    {
        get
        {
            return phase;
        }
        set
        {
            phase = value;
        }
    }

    public Oscillator(WaveType type, float frequency, float amplitude)
    {
        this.type = type;
        this.phase = 0.0f;
        this.frequency = frequency;
        this.amplitude = amplitude;
        SetPhaseIncrement();
        SetWaveFunction();
    }

    /// <summary>
    /// Resets the phase.
    /// </summary>
    public void Reset()
    {
        phase = 0;
    }

    public float GetValue()
    {
        float result = waveFunc(phase);
        phase += phaseIncrement;
        phase %= TAU;
        return result * amplitude;
    }

    /// <summary>
    /// Gets a sample by the given phase term instead of using the phase stored by this instance.
    /// </summary>
    /// <param name="phaseValue">Phase must be between 0 and 2 * PI</param>
    /// <returns></returns>
    public float GetValueAtPhase(float phaseValue)
    {
        return waveFunc(phaseValue) * amplitude;
    }

    /// <summary>
    /// Modulates the generated sample by the given phase term.
    /// </summary>
    /// <param name="phaseValue">Phase must be between 0 and 2 * PI</param>
    /// <returns></returns>
    public float GetModulatedValue(float phaseValue)
    {
        float result = waveFunc(this.phase + phaseValue);
        this.phase += phaseIncrement;
        this.phase %= TAU;
        return result * amplitude;
    }

    private void SetWaveFunction()
    {
        switch(type)
        {
            case WaveType.Saw:
                waveFunc = GetSawSample;
                break;
            case WaveType.Sine:
                waveFunc = GetSineSample;
                break;
            case WaveType.Square:
                waveFunc = GetSquareSample;
                break;
            case WaveType.Triangle:
                waveFunc = GetTriangleSample;
                break;
        }
    }

    private void SetPhaseIncrement()
    {
        phaseIncrement = TAU * frequency / AudioContext.SampleRate;
    }

    public static float GetSawSample(float phase) 
    {
        phase = phase / TAU;
        return 2.0f * phase - 1.0f;
    }

    public static float GetSineSample(float phase) 
    {
        return (float)Math.Sin(phase);
    }

    public static float GetSquareSample(float phase) 
    {
        return (float)Math.Sign(Math.Sin(phase));
    }

    public static float GetTriangleSample(float phase) 
    {
        phase = phase / TAU;
        return (float)(2 * Math.Abs(2 * (phase - 0.5)) - 1);
    }
}