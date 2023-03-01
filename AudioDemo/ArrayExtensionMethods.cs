namespace AudioDemo; 

public static class ArrayExtensionMethods {
    // extension method for absolute value of an array
    public static float[] Abs(this float[] array) {
        var result = new float[array.Length];
        for (var i = 0; i < array.Length; i++) {
            result[i] = Math.Abs(array[i]);
        }
        return result;
    }
    
    // extension method for lowpass filtering an array
    public static float[] Lowpass(this float[] array, float factor) {
        var result = new float[array.Length];
        result[0] = array[0];
        for (var i = 1; i < array.Length; i++) {
            result[i] = factor * array[i] + (1 - factor) * result[i - 1];
        }
        return result;
    }
}