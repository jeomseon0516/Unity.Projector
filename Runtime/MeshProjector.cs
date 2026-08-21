using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jeomseon.Unity.Projector
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class MeshProjector : MonoBehaviour
    {
        [SerializeField] private ProjectorEffect effect;
        [SerializeField] private Vector3 size = new(10f, 10f, 10f);
        [SerializeField] private LayerMask receiverMask = ~0;
        [SerializeField] private bool automaticallyCollectReceivers = true;
        [SerializeField, Min(0f)] private float automaticRefreshInterval = 0.5f;
        [SerializeField] private bool renderingEnabled = true;
        [SerializeField] private List<MeshRenderer> receivers = new();
        [SerializeField] private List<SkinnedMeshRenderer> skinnedMeshReceivers = new();
        [SerializeField] private List<Terrain> terrainReceivers = new();
        [SerializeField, Range(17, 257)] private int terrainMeshResolution = 129;
        [SerializeField] private bool cullByCameraFrustum = true;
        [SerializeField] private Camera cullingCamera;

        private readonly Dictionary<MeshRenderer, MeshFilter> _meshFilters = new();
        private readonly Dictionary<SkinnedMeshRenderer, Mesh> _skinnedMeshes = new();
        private readonly Dictionary<Terrain, Mesh> _terrainMeshes = new();
        private readonly Dictionary<Terrain, TerrainData> _terrainDataSources = new();
        private readonly Plane[] _frustumPlanes = new Plane[6];
        private bool _frustumPlanesValid;
        private MaterialPropertyBlock _propertyBlock;
        private Material _material;
        private ProjectorEffect _materialEffect;
        private bool _receiversDirty = true;
        private double _nextAutomaticRefreshTime;
        private int _cachedTerrainMeshResolution = -1;

        public ProjectorEffect Effect
        {
            get => effect;
            set
            {
                if (effect == value) return;

                effect = value;
                RecreateMaterial();
            }
        }

        public Vector3 Size
        {
            get => size;
            set
            {
                size = new Vector3(
                    Mathf.Max(Mathf.Epsilon, value.x),
                    Mathf.Max(Mathf.Epsilon, value.y),
                    Mathf.Max(Mathf.Epsilon, value.z));
                _receiversDirty = true;
            }
        }

        public LayerMask ReceiverMask
        {
            get => receiverMask;
            set
            {
                receiverMask = value;
                _receiversDirty = true;
            }
        }

        public bool RenderingEnabled
        {
            get => renderingEnabled;
            set => renderingEnabled = value;
        }

        public bool AutomaticallyCollectReceivers
        {
            get => automaticallyCollectReceivers;
            set
            {
                if (automaticallyCollectReceivers == value) return;

                automaticallyCollectReceivers = value;
                _receiversDirty = true;
            }
        }

        public float AutomaticRefreshInterval
        {
            get => automaticRefreshInterval;
            set
            {
                automaticRefreshInterval = Mathf.Max(0f, value);
                _nextAutomaticRefreshTime = 0d;
            }
        }

        public IReadOnlyList<MeshRenderer> Receivers => receivers;
        public int ReceiverCount => receivers.Count + skinnedMeshReceivers.Count + terrainReceivers.Count;

        public Matrix4x4 WorldToProjectionMatrix
        {
            get
            {
                Vector3 safeSize = new(
                    Mathf.Max(Mathf.Epsilon, size.x),
                    Mathf.Max(Mathf.Epsilon, size.y),
                    Mathf.Max(Mathf.Epsilon, size.z));
                return Matrix4x4.Scale(new Vector3(
                    1f / safeSize.x,
                    1f / safeSize.y,
                    1f / safeSize.z)) * transform.worldToLocalMatrix;
            }
        }

        private void OnEnable()
        {
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightmapChanged;
            TerrainCallbacks.heightmapChanged += OnTerrainHeightmapChanged;
            EnsureResources();
            _receiversDirty = true;
        }

        private void OnDisable()
        {
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightmapChanged;
            ReleaseMaterial();
            ReleaseGeneratedMeshes();
        }

        private void OnDestroy()
        {
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightmapChanged;
            ReleaseMaterial();
            ReleaseGeneratedMeshes();
        }

        private void OnValidate()
        {
            Size = size;
            automaticRefreshInterval = Mathf.Max(0f, automaticRefreshInterval);
            RecreateMaterial();
        }

        private void LateUpdate()
        {
            if (!renderingEnabled || !EnsureResources()) return;

            if (transform.hasChanged)
            {
                _receiversDirty = true;
                transform.hasChanged = false;
            }

            if (automaticallyCollectReceivers &&
                (_receiversDirty || automaticRefreshInterval > 0f &&
                    Time.realtimeSinceStartupAsDouble >= _nextAutomaticRefreshTime))
            {
                RefreshReceivers();
            }
            else if (!automaticallyCollectReceivers && _receiversDirty) RebuildReceiverCache();
            RenderReceivers();
        }

        public void RefreshReceivers()
        {
            receivers.Clear();
            skinnedMeshReceivers.Clear();
            terrainReceivers.Clear();
            _meshFilters.Clear();

            Bounds projectionBounds = CalculateWorldBounds();
            foreach (MeshRenderer candidate in FindObjectsByType<MeshRenderer>(
                         FindObjectsInactive.Exclude,
                         FindObjectsSortMode.None))
            {
                if (candidate == null || !candidate.enabled || candidate.gameObject == gameObject ||
                    (receiverMask.value & 1 << candidate.gameObject.layer) == 0 ||
                    !projectionBounds.Intersects(candidate.bounds) ||
                    !candidate.TryGetComponent(out MeshFilter meshFilter) ||
                    meshFilter.sharedMesh == null)
                {
                    continue;
                }

                receivers.Add(candidate);
                _meshFilters[candidate] = meshFilter;
            }

            foreach (SkinnedMeshRenderer candidate in FindObjectsByType<SkinnedMeshRenderer>(
                         FindObjectsInactive.Exclude,
                         FindObjectsSortMode.None))
            {
                if (candidate == null || !candidate.enabled || candidate.gameObject == gameObject ||
                    (receiverMask.value & 1 << candidate.gameObject.layer) == 0 ||
                    !projectionBounds.Intersects(candidate.bounds) || candidate.sharedMesh == null)
                {
                    continue;
                }

                skinnedMeshReceivers.Add(candidate);
            }

            foreach (Terrain candidate in FindObjectsByType<Terrain>(
                         FindObjectsInactive.Exclude,
                         FindObjectsSortMode.None))
            {
                if (candidate == null || !candidate.enabled || candidate.terrainData == null ||
                    (receiverMask.value & 1 << candidate.gameObject.layer) == 0 ||
                    !projectionBounds.Intersects(CalculateTerrainBounds(candidate)))
                {
                    continue;
                }

                terrainReceivers.Add(candidate);
            }

            SynchronizeGeneratedMeshes();

            _receiversDirty = false;
            _nextAutomaticRefreshTime = automaticRefreshInterval > 0f
                ? Time.realtimeSinceStartupAsDouble + automaticRefreshInterval
                : double.PositiveInfinity;
        }

        public void SetTexture(int propertyId, Texture texture)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetTexture(propertyId, texture);
        }

        public void SetColor(int propertyId, Color color)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetColor(propertyId, color);
        }

        public void SetFloat(int propertyId, float value)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetFloat(propertyId, value);
        }

        public void SetVector(int propertyId, Vector4 value)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetVector(propertyId, value);
        }

        public void SetMatrix(int propertyId, Matrix4x4 value)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetMatrix(propertyId, value);
        }

        public void SetBuffer(int propertyId, GraphicsBuffer buffer)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetBuffer(propertyId, buffer);
        }

        public void SetBuffer(int propertyId, ComputeBuffer buffer)
        {
            EnsurePropertyBlock();
            _propertyBlock.SetBuffer(propertyId, buffer);
        }

        private bool EnsureResources()
        {
            EnsurePropertyBlock();
            if (effect == null)
            {
                effect = Resources.Load<ProjectorEffect>("DefaultMeshProjectorEffect");
            }

            if (_material != null && _materialEffect == effect) return true;

            RecreateMaterial();
            return _material != null;
        }

        private void RecreateMaterial()
        {
            ReleaseMaterial();
            if (effect == null) return;

            if (!effect.IsCompatible(out string errorMessage))
            {
                Debug.LogError(errorMessage, effect);
                return;
            }

            _material = new Material(effect.Shader)
            {
                name = $"{effect.name} (Projector Instance)",
                hideFlags = HideFlags.HideAndDontSave
            };
            _materialEffect = effect;
            EnsurePropertyBlock();
            if (_material.HasProperty(ProjectorShaderIds.ProjectionTexture))
            {
                _propertyBlock.SetTexture(ProjectorShaderIds.ProjectionTexture, effect.DefaultTexture);
            }

            if (_material.HasProperty(ProjectorShaderIds.ProjectionColor))
            {
                _propertyBlock.SetColor(ProjectorShaderIds.ProjectionColor, effect.DefaultColor);
            }
        }

        private void ReleaseMaterial()
        {
            if (_material != null)
            {
                if (Application.isPlaying) Destroy(_material);
                else DestroyImmediate(_material);
            }

            _material = null;
            _materialEffect = null;
        }

        private void EnsurePropertyBlock() => _propertyBlock ??= new MaterialPropertyBlock();

        private void RenderReceivers()
        {
            _propertyBlock.SetMatrix(ProjectorShaderIds.WorldToProjection, WorldToProjectionMatrix);
            UpdateFrustumPlanes();

            foreach (MeshRenderer receiver in receivers)
            {
                if (receiver == null || !receiver.enabled ||
                    !_meshFilters.TryGetValue(receiver, out MeshFilter meshFilter) ||
                    meshFilter == null || meshFilter.sharedMesh == null ||
                    !IsVisibleToCullingCamera(receiver.bounds))
                {
                    continue;
                }

                Mesh mesh = meshFilter.sharedMesh;
                RenderParams renderParams = new(_material)
                {
                    layer = receiver.gameObject.layer,
                    matProps = _propertyBlock,
                    receiveShadows = false,
                    shadowCastingMode = ShadowCastingMode.Off,
                    worldBounds = receiver.bounds
                };

                for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
                {
                    Graphics.RenderMesh(renderParams, mesh, subMeshIndex, receiver.localToWorldMatrix);
                }
            }

            RenderSkinnedMeshReceivers();
            RenderTerrainReceivers();
        }

        private void RebuildReceiverCache()
        {
            _meshFilters.Clear();
            foreach (MeshRenderer receiver in receivers)
            {
                if (receiver != null && receiver.TryGetComponent(out MeshFilter meshFilter) &&
                    meshFilter.sharedMesh != null)
                {
                    _meshFilters[receiver] = meshFilter;
                }
            }

            SynchronizeGeneratedMeshes();
            _receiversDirty = false;
        }

        private void RenderSkinnedMeshReceivers()
        {
            foreach (SkinnedMeshRenderer receiver in skinnedMeshReceivers)
            {
                if (receiver == null || !receiver.enabled || receiver.sharedMesh == null ||
                    !IsVisibleToCullingCamera(receiver.bounds))
                {
                    continue;
                }

                if (!_skinnedMeshes.TryGetValue(receiver, out Mesh mesh) || mesh == null)
                {
                    mesh = new Mesh { name = $"{receiver.name} (Projector Baked Mesh)", hideFlags = HideFlags.HideAndDontSave };
                    _skinnedMeshes[receiver] = mesh;
                }

                receiver.BakeMesh(mesh);
                RenderMesh(mesh, receiver.localToWorldMatrix, receiver.bounds, receiver.gameObject.layer);
            }
        }

        private void RenderTerrainReceivers()
        {
            foreach (Terrain receiver in terrainReceivers)
            {
                if (receiver == null || !receiver.enabled ||
                    !_terrainMeshes.TryGetValue(receiver, out Mesh mesh) || mesh == null)
                {
                    continue;
                }

                Bounds bounds = CalculateTerrainBounds(receiver);
                if (!IsVisibleToCullingCamera(bounds)) continue;

                RenderMesh(mesh, receiver.transform.localToWorldMatrix, bounds, receiver.gameObject.layer);
            }
        }

        private void UpdateFrustumPlanes()
        {
            Camera camera = Application.isPlaying && cullByCameraFrustum
                ? (cullingCamera != null ? cullingCamera : Camera.main)
                : null;
            _frustumPlanesValid = camera != null;
            if (_frustumPlanesValid) GeometryUtility.CalculateFrustumPlanes(camera, _frustumPlanes);
        }

        private bool IsVisibleToCullingCamera(Bounds bounds) =>
            !_frustumPlanesValid || GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds);

        private void RenderMesh(Mesh mesh, Matrix4x4 localToWorld, Bounds bounds, int layer)
        {
            RenderParams renderParams = new(_material)
            {
                layer = layer,
                matProps = _propertyBlock,
                receiveShadows = false,
                shadowCastingMode = ShadowCastingMode.Off,
                worldBounds = bounds
            };

            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                Graphics.RenderMesh(renderParams, mesh, subMeshIndex, localToWorld);
            }
        }

        private void SynchronizeGeneratedMeshes()
        {
            int resolution = Mathf.Clamp(terrainMeshResolution, 17, 257);
            if (_cachedTerrainMeshResolution != resolution)
            {
                ReleaseTerrainMeshes();
                _cachedTerrainMeshResolution = resolution;
            }

            RemoveStaleGeneratedMeshes(_skinnedMeshes, skinnedMeshReceivers);
            RemoveStaleGeneratedMeshes(_terrainMeshes, terrainReceivers);
            RemoveStaleTerrainDataSources();

            foreach (Terrain terrain in terrainReceivers)
            {
                if (_terrainDataSources.TryGetValue(terrain, out TerrainData source) &&
                    source != terrain.terrainData && _terrainMeshes.Remove(terrain, out Mesh staleMesh))
                {
                    DestroyGeneratedMesh(staleMesh);
                }

                if (_terrainMeshes.ContainsKey(terrain)) continue;

                _terrainMeshes[terrain] = BuildTerrainMesh(terrain);
                _terrainDataSources[terrain] = terrain.terrainData;
            }
        }

        private void RemoveStaleTerrainDataSources()
        {
            if (_terrainDataSources.Count == 0) return;

            List<Terrain> staleReceivers = null;
            foreach (Terrain terrain in _terrainDataSources.Keys)
            {
                if (terrain != null && terrainReceivers.Contains(terrain)) continue;
                (staleReceivers ??= new List<Terrain>()).Add(terrain);
            }

            if (staleReceivers == null) return;
            foreach (Terrain terrain in staleReceivers) _terrainDataSources.Remove(terrain);
        }

        private static void RemoveStaleGeneratedMeshes<TRenderer>(
            Dictionary<TRenderer, Mesh> meshes,
            List<TRenderer> activeReceivers) where TRenderer : UnityEngine.Object
        {
            if (meshes.Count == 0) return;

            List<TRenderer> staleReceivers = null;
            foreach ((TRenderer receiver, Mesh mesh) in meshes)
            {
                if (receiver != null && activeReceivers.Contains(receiver)) continue;

                DestroyGeneratedMesh(mesh);
                (staleReceivers ??= new List<TRenderer>()).Add(receiver);
            }

            if (staleReceivers == null) return;
            foreach (TRenderer receiver in staleReceivers) meshes.Remove(receiver);
        }

        private void OnTerrainHeightmapChanged(Terrain terrain, RectInt _, bool __)
        {
            if (!_terrainMeshes.Remove(terrain, out Mesh mesh)) return;

            DestroyGeneratedMesh(mesh);
            _terrainDataSources.Remove(terrain);
            _receiversDirty = true;
        }

        private Mesh BuildTerrainMesh(Terrain terrain)
        {
            TerrainData data = terrain.terrainData;
            int resolution = Mathf.Clamp(terrainMeshResolution, 17, 257);
            Vector3[] vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            for (int z = 0; z < resolution; z++)
            {
                float normalizedZ = z / (float)(resolution - 1);
                for (int x = 0; x < resolution; x++)
                {
                    float normalizedX = x / (float)(resolution - 1);
                    vertices[z * resolution + x] = new Vector3(
                        normalizedX * data.size.x,
                        data.GetInterpolatedHeight(normalizedX, normalizedZ),
                        normalizedZ * data.size.z);
                }
            }

            int triangleIndex = 0;
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int bottomLeft = z * resolution + x;
                    int topLeft = bottomLeft + resolution;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft + 1;
                    triangles[triangleIndex++] = bottomLeft + 1;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topLeft + 1;
                }
            }

            Mesh mesh = new()
            {
                name = $"{terrain.name} (Projector Terrain Mesh)",
                hideFlags = HideFlags.HideAndDontSave,
                indexFormat = vertices.Length > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private void ReleaseGeneratedMeshes()
        {
            foreach (Mesh mesh in _skinnedMeshes.Values) DestroyGeneratedMesh(mesh);
            foreach (Mesh mesh in _terrainMeshes.Values) DestroyGeneratedMesh(mesh);
            _skinnedMeshes.Clear();
            _terrainMeshes.Clear();
            _terrainDataSources.Clear();
        }

        private void ReleaseTerrainMeshes()
        {
            foreach (Mesh mesh in _terrainMeshes.Values) DestroyGeneratedMesh(mesh);
            _terrainMeshes.Clear();
            _terrainDataSources.Clear();
        }

        private static void DestroyGeneratedMesh(Mesh mesh)
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }

        private static Bounds CalculateTerrainBounds(Terrain terrain)
        {
            Vector3 size = terrain.terrainData.size;
            return CalculateTransformedBounds(
                terrain.transform.localToWorldMatrix,
                Vector3.zero,
                size);
        }

        private Bounds CalculateWorldBounds()
        {
            Vector3 halfSize = size * 0.5f;
            return CalculateTransformedBounds(transform.localToWorldMatrix, -halfSize, halfSize);
        }

        private static Bounds CalculateTransformedBounds(
            Matrix4x4 localToWorld,
            Vector3 localMinimum,
            Vector3 localMaximum)
        {
            Bounds bounds = new(localToWorld.MultiplyPoint3x4(localMinimum), Vector3.zero);
            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        bounds.Encapsulate(localToWorld.MultiplyPoint3x4(new Vector3(
                            x == 0 ? localMinimum.x : localMaximum.x,
                            y == 0 ? localMinimum.y : localMaximum.y,
                            z == 0 ? localMinimum.z : localMaximum.z)));
                    }
                }
            }

            return bounds;
        }
    }
}
