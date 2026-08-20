using UnityEngine;

namespace Jeomseon.Unity.Projector
{
    public static class ProjectorShaderIds
    {
        public static readonly int EffectVersion = Shader.PropertyToID("_ProjectorEffectVersion");
        public static readonly int WorldToProjection = Shader.PropertyToID("_ProjectorWorldToProjection");
        public static readonly int ProjectionTexture = Shader.PropertyToID("_ProjectionTexture");
        public static readonly int ProjectionColor = Shader.PropertyToID("_ProjectionColor");
    }
}
