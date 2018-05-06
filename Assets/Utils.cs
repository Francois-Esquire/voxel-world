using UnityEngine;

public class Utils {

    static int maxHeight = 150;
    static float smooth = 0.01f;
    static int octaves = 4;
    static float persistence = 0.5f;

    public static int GenerateHeight(float x, float y, int offset = 0, int octset = 0) {
        float variant = FBM(x * smooth, y * smooth, octaves + octset, persistence);

        float height = Map(0, maxHeight - offset, 0, 1, variant);

        return (int)height;
    }

    public static float FBM3D(float x, float y, float z, float sm, int oct) {
        float XY = FBM(x * sm, y * sm, oct, 0.5f);
        float YZ = FBM(y * sm, z * sm, oct, 0.5f);
        float XZ = FBM(x * sm, z * sm, oct, 0.5f);

        float YX = FBM(y * sm, x * sm, oct, 0.5f);
        float ZY = FBM(z * sm, y * sm, oct, 0.5f);
        float ZX = FBM(z * sm, x * sm, oct, 0.5f);

        return (XY + YZ + XZ + YX + ZY + ZX)/6.0f;
    }

    //public static float FBM3D(float x, float y, float z, float sm = 0.01f, int oct = 1, float per = 0.5f) {
    //    float XY = FBM(x * sm, y * sm, oct, per);
    //    float YZ = FBM(y * sm, z * sm, oct, per);
    //    float XZ = FBM(x * sm, z * sm, oct, per);

    //    float YX = FBM(y * sm, x * sm, oct, per);
    //    float ZX = FBM(z * sm, x * sm, oct, per);
    //    float ZY = FBM(z * sm, y * sm, oct, per);

    //    return (XY + YZ + XZ + YX + ZX + ZY) / 6.0f;
    //}

    static float Map(float newMin, float newMax, float origMin, float origMax,float value) {
        float inverse = Mathf.InverseLerp(origMin, origMax, value);
        return Mathf.Lerp(newMin, newMax, inverse);
    }

    // Fractal Brownian Motion
    static float FBM(float x, float z, int oct, float pers) {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 1;

        for (int i = 0; i < oct; i++) {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= pers;
            frequency *= 2;
        }

        return total / maxValue;
    }
}