using System;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, int offsetSamples = 0)
    {
        // WAV format parsing (basic)
        int sampleCount = BitConverter.ToInt32(wavFile, 40);
        int frequency = BitConverter.ToInt32(wavFile, 24);
        int channels = wavFile[22];
        int sampleSize = BitConverter.ToInt16(wavFile, 34) / 8;

        float[] samples = new float[sampleCount / sampleSize];
        int dataStart = 44;
        for (int i = 0; i < samples.Length; i++)
        {
            short sampleValue = BitConverter.ToInt16(wavFile, dataStart + i * sampleSize);
            samples[i] = sampleValue / 32768.0f;
        }

        AudioClip audioClip = AudioClip.Create("WavClip", samples.Length, channels, frequency, false);
        audioClip.SetData(samples, offsetSamples);
        return audioClip;
    }
}