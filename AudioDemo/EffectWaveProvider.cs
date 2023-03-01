﻿using NAudio.Wave;

namespace AudioDemo; 

public class EffectWaveProvider : ISampleProvider {
    // private variables
    private readonly ISampleProvider? _source;
    private float[] _beatSample;
    private int _coolDown;
    private int _beatIndex = -1;
    
    // public properties
    public WaveFormat? WaveFormat => _source?.WaveFormat; 
    
    private void GetSample(string path, WaveFormat format, float volume) {
        using var clip = new WaveFileReader(path);
        // resample it to the correct format
        var resampler = new MediaFoundationResampler(clip, format).ToSampleProvider();
        _beatSample = new float[56396]; // 2211 is the length of the sample
        resampler.Read(_beatSample, 0, _beatSample.Length);
        // apply volume
        for (var i = 0; i < _beatSample.Length; i++) {
            _beatSample[i] *= volume;
        }
    }
    
    #region Public API
    
    public EffectWaveProvider(ISampleProvider source) {
        _source = source;
        _beatSample = Array.Empty<float>();
        
        GetSample("kick.wav", _source.WaveFormat, 5);
    }

    public int Read(float[] buffer, int offset, int count) {
        // TODO: refactor this mess
        var read = _source?.Read(buffer, offset, count) ?? 0;
        if (count <= 0) return 0;

        
        if (_beatIndex >= 0) {
            var index = offset;
            // insert the beat sample
            while (_beatIndex < _beatSample.Length && index < buffer.Length) {
                buffer[index] = _beatSample[_beatIndex];
                _beatIndex++;
                index++;
            }
            
            // reset the beat index
            if (_beatIndex >= _beatSample.Length) {
                _beatIndex = -1;
            }

            return read;
        }
        
        if (_coolDown > 0) {
            _coolDown -= read;
            return read;
        }
        
        #region BEAT DETECTION
        // create wave with absolute values
        var absWave = new float[read];
        for (var i = 0; i < read; i++) {
            absWave[i] = Math.Abs(buffer[i]);
        }
        
        // smooth the wave with a moving average (lowpass filter)
        const float smoothFactor = 0.5f;
        var smoothWave = new float[read];
        smoothWave[0] = buffer[0];
        for (var i = 1; i < smoothWave.Length; i++) {
            smoothWave[i] = smoothFactor * absWave[i] + (1 - smoothFactor) * smoothWave[i - 1];
        }
        
        const float minThreshold = 0.01f;
        const float dynamicThreshold = 5f;

        // insert the beat if the condition is met
        for (var i = 0; i < absWave.Length; i++) {
            var index = i + offset;
            if (index >= buffer.Length) break;
            
            if (absWave[i] > Math.Max(smoothWave[i] * dynamicThreshold, minThreshold)) {
                _coolDown = _beatSample.Length;
                _beatIndex = 0;
            }
        }
        #endregion
        
        return read;
    }
    
    #endregion
}