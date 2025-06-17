namespace VBQOL
{
    public static class Vb_QualitySetting
    {
        public static void Init()
        {   
           /// QualitySettings.activeColorSpace = ColorSpace.Linear;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.antiAliasing = (int)0;
            QualitySettings.asyncUploadBufferSize = (int)4;
            QualitySettings.asyncUploadPersistentBuffer = true;
            QualitySettings.asyncUploadTimeSlice = (int)2; 
         //*   QualitySettings.billboardsFaceCameraPosition = true;
            QualitySettings.billboardsFaceCameraPosition = false;
         ///   QualitySettings.desiredColorSpace = Linear;
            QualitySettings.enableLODCrossFade = true;
            QualitySettings.globalTextureMipmapLimit = (int)0;
          //*  QualitySettings.lodBias = (int)5;
            QualitySettings.lodBias = (int)1;
            QualitySettings.maximumLODLevel = (int)0;
            QualitySettings.maxQueuedFrames = (int)2;
         //*   QualitySettings.particleRaycastBudget = (int)4096;
            QualitySettings.particleRaycastBudget = (int)1024;
          //*  QualitySettings.pixelLightCount = (int)8;
            QualitySettings.pixelLightCount = (int)2;
            QualitySettings.realtimeGICPUUsage = (int)25;
        //*    QualitySettings.realtimeReflectionProbes = true;
            QualitySettings.realtimeReflectionProbes = false;
         ///   QualitySettings.renderPipeline);
            QualitySettings.resolutionScalingFixedDPIFactor = (int)1;
            QualitySettings.shadowCascade2Split = (int)0.33;
       ///     QualitySettings.shadowCascade4Split = (int)(0.07, 0.20, 0.47);
           //* QualitySettings.shadowCascades = (int)4;
            QualitySettings.shadowCascades = (int)2;
          //*  QualitySettings.shadowDistance = (int)150;
            QualitySettings.shadowDistance = (int)50;
          //*  QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            QualitySettings.shadowNearPlaneOffset = (int)3;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
      //*      QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadows = ShadowQuality.All;
       //*     QualitySettings.skinWeights = SkinWeights.FourBones;
            QualitySettings.skinWeights = SkinWeights.OneBone;
        //*    QualitySettings.softParticles = true;
         //*   QualitySettings.softVegetation = true;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            QualitySettings.streamingMipmapsActive = false;
            QualitySettings.streamingMipmapsAddAllCameras = true;
            QualitySettings.streamingMipmapsMaxFileIORequests = (int)1024;
            QualitySettings.streamingMipmapsMaxLevelReduction = (int)2;
           //* QualitySettings.streamingMipmapsMemoryBudget = (int)512;
            QualitySettings.streamingMipmapsMemoryBudget = (int)4096;
          //*  QualitySettings.streamingMipmapsRenderersPerFrame = (int)512;
            QualitySettings.streamingMipmapsRenderersPerFrame = (int)256;
       //*     QualitySettings.terrainBasemapDistance = (int)1000;
            QualitySettings.terrainBasemapDistance = (int)100;
            QualitySettings.terrainBillboardStart = (int)50;
            QualitySettings.terrainDetailDensityScale = (int)1;
          //*  QualitySettings.terrainDetailDistance = (int)80;
            QualitySettings.terrainDetailDistance = (int)10;
            QualitySettings.terrainFadeLength = (int)5;
         //*   QualitySettings.terrainMaxTrees = (int)50;
            QualitySettings.terrainMaxTrees = (int)10;
            QualitySettings.terrainPixelError = (int)1;
            QualitySettings.terrainQualityOverrides = TerrainQualityOverrides.None;
       //*     QualitySettings.terrainTreeDistance = (int)5000;
            QualitySettings.terrainTreeDistance = (int)1000;
            QualitySettings.useLegacyDetailDistribution = true;
            QualitySettings.vSyncCount = (int)0;
        }
    }
}
