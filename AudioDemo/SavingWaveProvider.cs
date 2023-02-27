using NAudio.Wave;

namespace AudioDemo;

public class SavingWaveProvider : IWaveProvider, IDisposable{
    private readonly IWaveProvider? _source;
    private readonly WaveFileWriter? _writer;
    private bool _isDisposed;

    public WaveFormat? WaveFormat => _source?.WaveFormat; 
    
    public SavingWaveProvider (IWaveProvider source, string filePath) {
        _source = source;
        _writer = new WaveFileWriter(filePath, source.WaveFormat);
        _isDisposed = false;
    }
    
    public int Read(byte[] destinationBuffer, int offset, int numBytes) {
        var read = _source.Read(destinationBuffer, offset, numBytes);
        if (numBytes > 0 && !_isDisposed) {
            _writer.Write(destinationBuffer, offset, read);
        }
        else if (numBytes == 0) {
            Dispose();
        }

        return read;
    }

    public void Dispose() {
        if (_isDisposed) return;
        
        _isDisposed = true;
        _writer?.Dispose();
    }
}