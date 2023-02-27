using NAudio.Wave;

namespace AudioDemo; 

public class EffectWaveProvider : ISampleProvider {
    // private variables
    private readonly ISampleProvider? _source;
    private float[] _beatSample;
    
    // public properties
    public WaveFormat? WaveFormat => _source?.WaveFormat; 
    
    private void GetSample(string path, WaveFormat format) {
        var clip = new WaveFileReader(path);
        var resampler = new MediaFoundationResampler(clip, format).ToSampleProvider();
        _beatSample = new float[format.SampleRate * clip.Length];
        var read = resampler.Read(_beatSample, 0, _beatSample.Length);
        _beatSample = _beatSample[..read];
    }
    
    #region Public API
    
    public EffectWaveProvider(ISampleProvider source) {
        _source = source;
        _beatSample = Array.Empty<float>();
        
        GetSample("click2.wav", _source.WaveFormat);
    }

    public int Read(float[] buffer, int offset, int count) {
        var read = _source.Read(buffer, offset, count);
        if (count == 0) return 0;
        
        #region BEAT DETECTION
        // create wave with absolute values
        var absWave = new float[read];
        for (var i = 0; i < read; i++) {
            absWave[i] = Math.Abs(buffer[i]);
        }
        
        // smooth the wave with a moving average (lowpass filter)
        var smoothWave = new float[read];
        var smoothFactor = 0.5f;
        smoothWave[0] = buffer[0];
        for (var i = 1; i < smoothWave.Length; i++) {
            smoothWave[i] = smoothFactor * absWave[i] + (1 - smoothFactor) * smoothWave[i - 1];
        }
        
        var minThreshold = 0.01f;
        var dynamicThreshold = 3f;

        // insert the beat
        for (var i = 0; i < _beatSample.Length; i++) {
            var index = i + offset;
            if (index >= buffer.Length) break;
            buffer[index] += _beatSample[i];
        }
        #endregion

        return read;
    }
    
    #endregion
}