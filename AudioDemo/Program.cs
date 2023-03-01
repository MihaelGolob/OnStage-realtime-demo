using AudioDemo;
using NAudio.CoreAudioApi;
using NAudio.Wave;

public class Program {
    private static BufferedWaveProvider? _buffer;

    public static void Main() {
        PlaybackWithWasapi();
    }

    private static void PlaybackWithWasapi() {
        Console.WriteLine("Starting setup");
        
        // setup microphone recorder
        var captureDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        var recorder = new WasapiCapture(captureDevice);
        recorder.WaveFormat = new WaveFormat(44100, 32, 2);
        recorder.DataAvailable += RecorderOnDataAvailable;
        
        // create buffer
        _buffer = new BufferedWaveProvider(recorder.WaveFormat);
        var effectProvider = new EffectWaveProvider(_buffer.ToSampleProvider().ToMono());
        Console.WriteLine("WaveFormat: {effectProvider.WaveFormat}");
        
        var provider = new VolumeSampleProvider(effectProvider, 5f);
        
        // setup playback
        using var player = new WasapiOut(AudioClientShareMode.Shared, 0);
        player.Init(provider);

        Console.WriteLine("Recording ...");
        // start playing & recording
        player.Play();
        recorder.StartRecording();
        Console.WriteLine("Press any key to stop recording");
        Console.ReadKey();
        
        recorder.StopRecording();
        player.Stop();
    }

    private static void RecorderOnDataAvailable(object? sender, WaveInEventArgs e) {
        _buffer?.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }
}

