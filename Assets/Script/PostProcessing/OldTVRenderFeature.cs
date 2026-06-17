using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OldTVRenderFeature : ScriptableRendererFeature
{
    class OldTVPass : ScriptableRenderPass
    {
        private Material material;
        private OldTVVolume volume;
        private RTHandle cameraColorTarget;
        private RTHandle tempTexture;

        public OldTVPass(Material material)
        {
            this.material = material;
            // Kita pindahkan ke AfterRendering agar dipastikan dieksekusi setelah Canvas UI selesai dirender
            renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; 
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempTVTexture");
            
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;
            
            volume = VolumeManager.instance.stack.GetComponent<OldTVVolume>();
            if (volume == null || !volume.IsActive()) return;

            CommandBuffer cmd = CommandBufferPool.Get("Old TV Effect");

            material.SetFloat("_Intensity", volume.intensity.value);
            material.SetFloat("_Curvature", volume.curvature.value);
            material.SetFloat("_ScanlineCount", volume.scanlineCount.value);
            material.SetFloat("_ScanlineSpeed", volume.scanlineSpeed.value);
            material.SetFloat("_NoiseSpeed", volume.noiseSpeed.value);
            material.SetFloat("_VignetteIntensity", volume.vignetteIntensity.value);
            material.SetColor("_ScanlineColor", volume.scanlineColor.value);
            material.SetColor("_NoiseColor", volume.noiseColor.value);

            // Blit ke temporary texture untuk di proses shader
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempTexture, material, 0);
            
            // Kembalikan hasilnya ke layar utama
            Blitter.BlitCameraTexture(cmd, tempTexture, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            if (tempTexture != null) tempTexture.Release();
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Shader shader;
    }

    public Settings settings = new Settings();
    private OldTVPass pass;
    private Material material;

    public override void Create()
    {
        if (settings.shader == null) return;
        material = CoreUtils.CreateEngineMaterial(settings.shader);
        pass = new OldTVPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) return;
        
        // Hanya render di layar utama
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(pass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (material != null) CoreUtils.Destroy(material);
        if (pass != null) pass.Dispose();
    }
}
