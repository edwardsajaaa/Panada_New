using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/Old TV Effect")]
public class OldTVVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter curvature = new ClampedFloatParameter(4f, 1f, 10f);
    public FloatParameter scanlineCount = new FloatParameter(250f);
    public FloatParameter scanlineSpeed = new FloatParameter(5f);
    public FloatParameter noiseSpeed = new FloatParameter(50f);
    public ClampedFloatParameter vignetteIntensity = new ClampedFloatParameter(1.2f, 0f, 3f);
    public ColorParameter scanlineColor = new ColorParameter(new Color(0f, 0f, 0f, 0.6f));
    public ColorParameter noiseColor = new ColorParameter(new Color(1f, 1f, 1f, 0.03f));

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}
