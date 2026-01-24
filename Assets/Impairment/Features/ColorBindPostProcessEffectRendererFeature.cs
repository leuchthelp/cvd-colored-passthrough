using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// Create a Scriptable Renderer Feature that implements a post-processing effect when the camera is inside a custom volume.
// For more information about creating scriptable renderer features, refer to https://docs.unity3d.com/Manual/urp/customizing-urp.html
public sealed class ColorBlindPostProcessEffectRendererFeature : ScriptableRendererFeature
{
    #region FEATURE_FIELDS

    // Declare the material used to render the post-processing effect.
    // Add a [SerializeField] attribute so Unity serializes the property and includes it in builds.
    [SerializeField]
    //[HideInInspector]
    private Material m_Material;

    // Declare the render pass that renders the effect.
    private ColorBlindPostRenderPass m_FullScreenPass;

    #endregion

    #region FEATURE_METHODS

    // Override the Create method.
    // Unity calls this method when the Scriptable Renderer Feature loads for the first time, and when you change a property.
    public override void Create()
    {
#if UNITY_EDITOR
        // Assign a material asset to m_Material in the Unity Editor.
        if (m_Material == null)
            m_Material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Testing/ColorBlind.mat");
#endif

        if (m_Material)
        {
            m_FullScreenPass = new ColorBlindPostRenderPass(name, m_Material);
        }
    }

    // Override the AddRenderPasses method to inject passes into the renderer. Unity calls AddRenderPasses once per camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Skip rendering if m_Material or the pass instance are null.
        if (m_Material == null || m_FullScreenPass == null)
            return;

        // Skip rendering if the target is a Reflection Probe or a preview camera.
        if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        // Skip rendering if the camera is outside the custom volume.
        ColorBlindEffectComponent myVolume = VolumeManager.instance.stack?.GetComponent<ColorBlindEffectComponent>();
        if (myVolume == null || !myVolume.IsActive())
            return;

        // Specify when the effect will execute during the frame.
        // For a post-processing effect, the injection point is usually BeforeRenderingTransparents, BeforeRenderingPostProcessing, or AfterRenderingPostProcessing.
        // For more information, refer to https://docs.unity3d.com/Manual/urp/customize/custom-pass-injection-points.html 
        m_FullScreenPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        // Specify that the effect doesn't need scene depth, normals, motion vectors, or the color texture as input.
        m_FullScreenPass.ConfigureInput(ScriptableRenderPassInput.None);

        // Add the render pass to the renderer.
        renderer.EnqueuePass(m_FullScreenPass);
    }

    #endregion

    // Create the custom render pass.
    private class ColorBlindPostRenderPass : ScriptableRenderPass
    {
        #region PASS_FIELDS

        // Declare the material used to render the post-processing effect.
        private readonly Material m_Material;

        // Declare a property block to set additional properties for the material.
        private static readonly MaterialPropertyBlock s_SharedPropertyBlock = new();

        // Declare a property that enables or disables the render pass that samples the color texture.
        private static readonly bool kSampleActiveColor = true;

        // Create shader properties in advance, which is more efficient than referencing them by string.
        private static readonly int kBlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
        private static readonly int kBlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");

        private static readonly Color[,] coblisV1RGB =
        {
            { new Color(1f,0f,0f),   new Color(0f,1f,0f), new Color(0f,0f,1f) },    // Normal
            { new Color(.56667f, .43333f, 0f), new Color(.55833f, .44167f, 0f), new Color(0f, .24167f, .75833f) },    // Protanopia
            { new Color(.81667f, .18333f, 0f), new Color(.33333f, .66667f, 0f), new Color(0f, .125f, .875f)    }, // Protanomaly
            { new Color(.625f, .375f, 0f), new Color(.70f, .30f, 0f), new Color(0f, .30f, .70f)    },   // Deuteranopia
            { new Color(.80f, .20f, 0f), new Color(.25833f, .74167f, 0), new Color(0f, .14167f, .85833f)    },    // Deuteranomaly
            { new Color(.95f, .05f, 0), new Color(0f, .43333f, .56667f), new Color(0f, .475f, .525f) }, // Tritanopia
            { new Color(.96667f, .03333f, 0), new Color(0f, .73333f, .26667f), new Color(0f, .18333f, .81667f) }, // Tritanomaly
            { new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f)  },   // Achromatopsia
            { new Color(.618f, .32f, .062f), new Color(.163f, .775f, .062f), new Color(.163f, .320f, .516f)  }    // Achromatomaly
        };

        private static readonly Color[,] machadoRGB =
        {
            { new Color(1f,0f,-0f),   new Color(0f,1f,0f), new Color(-0f,0f,1f) }, // Normal
            //{ new Color(.56667f, .43333f, 0f), new Color(.55833f, .44167f, 0f), new Color(0f, .24167f, .75833f) }, // Protanopia
            { new Color(0.152286f, 1.052583f, -0.204868f), new Color(0.114503f, 0.786281f, 0.099216f), new Color(-0.003882f, -0.048116f, 1.051998f) }, // Protanomaly
            //{ new Color(.625f, .375f, 0f), new Color(0.029342f, 0.955115f, 0.015544f), new Color(0f, .30f, .70f) }, // Deuteranopia
            { new Color(0.367322f, 0.860646f, -0.227968f), new Color(0.280085f, 0.672501f, 0.047413f), new Color(-0.011820f, 0.042940f, 0.968881f) }, // Deuteranomaly
            //{ new Color(.95f, .05f, 0), new Color(0f, .43333f, .56667f), new Color(0f, .475f, .525f) }, // Tritanopia
            { new Color(1.255528f, -0.076749f, -0.178779f), new Color(-0.078411f, 0.930809f, 0.147602f), new Color(0.004733f, 0.691367f, 0.303900f) }, // Tritanomaly
            //{ new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f) }, // Achromatopsia
            //{ new Color(.618f, .32f, .062f), new Color(.163f, .775f, .062f), new Color(.163f, .320f, .516f) } // Achromatomaly
        };

        private static readonly List<Color[,]> matrixSwitch = new()
        {
            coblisV1RGB,
            machadoRGB,
        };

        #endregion

        public ColorBlindPostRenderPass(string passName, Material material)
        {
            // Add a profiling sampler.
            profilingSampler = new ProfilingSampler(passName);

            // Assign the material to the render pass.
            m_Material = material;

            // To make sure the render pass can sample the active color buffer, set URP to render to intermediate textures instead of directly to the backbuffer.
            requiresIntermediateTexture = kSampleActiveColor;
        }

        #region PASS_SHARED_RENDERING_CODE

        // Add commands to render the effect.
        // This method is used in both the render graph system path and the Compatibility Mode path.
        private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material)
        {
            // Clear the material properties.
            s_SharedPropertyBlock.Clear();
            if (sourceTexture != null)
                s_SharedPropertyBlock.SetTexture(kBlitTexturePropertyId, sourceTexture);

            // Set the scale and bias so shaders that use Blit.hlsl work correctly.
            s_SharedPropertyBlock.SetVector(kBlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));

            // Set the material properties based on the blended values of the custom volume.
            // For more information, refer to https://docs.unity3d.com/Manual/urp/post-processing/custom-post-processing-with-volume.html
            ColorBlindEffectComponent myVolume = VolumeManager.instance.stack?.GetComponent<ColorBlindEffectComponent>();
            if (myVolume != null)
            {
                s_SharedPropertyBlock.SetFloat("_Intensity", myVolume.intensity.value);
                int matrixType = (int) myVolume.type.value;
                int mode = (int) myVolume.mode.value;

                var severity = myVolume.severity.value;
                if (matrixType == 0)
                    s_SharedPropertyBlock.SetFloat("_Severity", severity);
                else
                {
                    myVolume.severity.overrideState = false;
                }

                var matrix = matrixSwitch[matrixType];

                s_SharedPropertyBlock.SetColor("_R", matrix[mode, 0]);
                s_SharedPropertyBlock.SetColor("_G", matrix[mode, 1]);
                s_SharedPropertyBlock.SetColor("_B", matrix[mode, 2]);
            }

            // Draw to the current render target.
            cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
        }

        #endregion

        #region PASS_RENDER_GRAPH_PATH

        // Declare the resources the main render pass uses.
        // This method is used only in the render graph system path.
        private class MainPassData
        {
            public Material material;

            public ColorBlindMode mode;
            public TextureHandle inputTexture;
        }

        private static void ExecuteMainPass(MainPassData data, RasterGraphContext context)
        {
            ExecuteMainPass(context.cmd, data.inputTexture.IsValid() ? data.inputTexture : null, data.material);
        }

        // Override the RecordRenderGraph method to implement the rendering logic.
        // This method is used only in the render graph system path.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {

            // Get the resources the pass uses.
            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Sample from the current color texture.
            using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(passName, out var passData, profilingSampler))
            {
                passData.material = m_Material;

                TextureHandle destination;

                // Copy cameraColor to a temporary texture, if the kSampleActiveColor property is set to true. 
                if (kSampleActiveColor)
                {
                    var cameraColorDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
                    cameraColorDesc.name = "_CameraColorColorBlindPostProcessing";
                    cameraColorDesc.clearBuffer = false;

                    destination = renderGraph.CreateTexture(cameraColorDesc);
                    passData.inputTexture = resourcesData.cameraColor;

                    // If you use framebuffer fetch in your material, use builder.SetInputAttachment to reduce GPU bandwidth usage and power consumption. 
                    builder.UseTexture(passData.inputTexture, AccessFlags.Read);
                }
                else
                {
                    destination = resourcesData.cameraColor;
                    passData.inputTexture = TextureHandle.nullHandle;
                }


                // Set the render graph to render to the temporary texture.
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                // Set the render method.
                builder.SetRenderFunc((MainPassData data, RasterGraphContext context) => ExecuteMainPass(data, context));

                // Set cameraColor to the new temporary texture so the next render pass can use it. You don't need to blit to and from cameraColor if you use the render graph system.
                if (kSampleActiveColor)
                {
                    resourcesData.cameraColor = destination;
                }
            }
        }

        #endregion
    }
}
