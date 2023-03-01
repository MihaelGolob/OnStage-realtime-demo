using NAudio.Wave;

namespace AudioDemo; 

public class VolumeSampleProvider : ISampleProvider {
    private readonly float _volume;
    private readonly ISampleProvider _source;
    
    public WaveFormat WaveFormat { get; }
    
    public VolumeSampleProvider(ISampleProvider source, float volume) {
        WaveFormat = source.WaveFormat;
        _source = source;
        _volume = volume;
    }
    
    public int Read(float[] buffer, int offset, int count) {
        // read buffer from source
        var read = _source.Read(buffer, offset, count);
        
        // apply volume
        for (var i = 0; i < count; i++) {
            buffer[offset + i] *= _volume;
        }

        return read;
    }
}