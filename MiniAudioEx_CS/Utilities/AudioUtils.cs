using System;

namespace MiniAudioEx;

public static class AudioUtility
{
    public static float GetMidiNoteToHertz(int keyIndex, float rootFrequency = 440.0f)
    {
        const int A4_KEY_INDEX = 69;
        return (float)(rootFrequency * Math.Pow(2, (keyIndex - A4_KEY_INDEX) / 12.0));
    }

    public static int GetSamplesPerMeasure(float bpm, float sampleRate)
    {
        const int measures = 1;
        const int beats = 4;
        const int totalBeats = beats * measures;
        float beatsPerSecond = bpm / 60.0f;
        float millisecondsPerBeat = (float)Math.Round(totalBeats / beatsPerSecond * 1000f) / 1000f;
        return (int)Math.Ceiling(millisecondsPerBeat * sampleRate);
    }

    public static int GetSamplesPerStep(int samplesPerMeasure, int stepsPerMeasure)
    {
        double d = (float)samplesPerMeasure / stepsPerMeasure;
        return (int)Math.Floor(d);
    }

    /// <summary>
    /// Helper method to calculate how many samples are needed for a single period of a signal by given frequency and sample rate.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public static int GetSampleLengthForFrequency(float frequency, float sampleRate)
    {
        return (int)Math.Ceiling(sampleRate / sampleRate);
    }

}