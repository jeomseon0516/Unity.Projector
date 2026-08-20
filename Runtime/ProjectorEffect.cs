using UnityEngine;

namespace Jeomseon.Unity.Projector
{
    public sealed class ProjectorEffect : ScriptableObject
    {
        [SerializeField] private Shader shader;
        [SerializeField] private Texture defaultTexture;
        [SerializeField] private Color defaultColor = Color.white;

        public Texture DefaultTexture => defaultTexture;
        public Color DefaultColor => defaultColor;
        internal Shader Shader => shader;

        public bool IsCompatible(out string errorMessage)
        {
            if (shader == null)
            {
                errorMessage = "The ProjectorEffect has no Shader.";
                return false;
            }

            if (!HasProperty(ProjectorShaderIds.EffectVersion))
            {
                errorMessage = $"Shader '{shader.name}' does not declare _ProjectorEffectVersion.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool HasProperty(int propertyId)
        {
            for (int index = 0; index < shader.GetPropertyCount(); index++)
            {
                if (shader.GetPropertyNameId(index) == propertyId) return true;
            }

            return false;
        }
    }
}
