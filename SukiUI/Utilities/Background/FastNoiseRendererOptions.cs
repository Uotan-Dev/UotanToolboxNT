namespace SukiUI.Utilities.Background;

public readonly struct FastNoiseRendererOptions
{
    public FastNoiseLite.NoiseType Type { get; }
    public float NoiseScale { get; }
    public float XAnimSpeed { get; }
    public float YAnimSpeed { get; }
    public float PrimaryAlpha { get; }
    public float AccentAlpha { get; }

    public FastNoiseRendererOptions(
        FastNoiseLite.NoiseType type,
        float noiseScale = 1.5f,
        float xAnimSpeed = 2f,
        float yAnimSpeed = 1f,
        float primaryAlpha = 0.7f,
        float accentAlpha = 0.04f,
        float animSeedScale = 0.1f
           /* float noiseScale = 1f,
            float xAnimSpeed = 0.05f,
            float yAnimSpeed = 0.025f,
            float primaryAlpha = 0.75f,
            float accentAlpha = 0.2f,
            float animSeedScale = 0.1f*/)
    {
        Type = type;
        NoiseScale = noiseScale;
        XAnimSpeed = xAnimSpeed * animSeedScale;
        YAnimSpeed = yAnimSpeed * animSeedScale;
        PrimaryAlpha = primaryAlpha;
        AccentAlpha = accentAlpha;
    }
}