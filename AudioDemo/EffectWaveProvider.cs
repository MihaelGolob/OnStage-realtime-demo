using NAudio.Wave;

namespace AudioDemo; 

public class EffectWaveProvider : ISampleProvider {
    // private variables
    private readonly ISampleProvider? _source;
    private float[] _beatSample;
    private int _coolDown;
    
    // public properties
    public WaveFormat? WaveFormat => _source?.WaveFormat; 
    
    private void GetSample(string path, WaveFormat format) {
        using var clip = new WaveFileReader(path);
        var resampler = new MediaFoundationResampler(clip, format).ToSampleProvider();
        _beatSample = new float[2407]; // 2407 is the length of the sample
        resampler.Read(_beatSample, 0, _beatSample.Length);
    }
    
    #region Public API
    
    public EffectWaveProvider(ISampleProvider source) {
        _source = source;
        _beatSample = Array.Empty<float>();
        
        GetSample("click2.wav", _source.WaveFormat);
    }

    public int Read(float[] buffer, int offset, int count) {
        var read = _source?.Read(buffer, offset, count) ?? 0;
        if (count <= 0) return 0;

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
        const float dynamicThreshold = 3f;

        // insert the beat if the condition is met
        for (var i = 0; i < absWave.Length; i++) {
            var index = i + offset;
            if (index >= buffer.Length) break;
            
            if (absWave[i] > Math.Max(smoothWave[i] * dynamicThreshold, minThreshold)) {
                // insert the beat
                for (var j = 0; j < _beatSample.Length; j++) {
                    _coolDown = WaveFormat?.SampleRate / 10 ?? 1000;
                    var beatIndex = j + index;
                    if (beatIndex >= buffer.Length) break;
                    buffer[beatIndex] = _beatSample[j];
                }
            }
        }
        #endregion
        
        return read;
    }
    
    #endregion
}