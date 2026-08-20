using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Jeomseon.Unity.Projector.Tests
{
    public sealed class MeshProjectorTests
    {
        [Test]
        public void WorldToProjectionMatrix_MapsBoxBoundaryToHalfExtent()
        {
            GameObject gameObject = new(nameof(WorldToProjectionMatrix_MapsBoxBoundaryToHalfExtent));
            MeshProjector projector = gameObject.AddComponent<MeshProjector>();
            projector.Size = new Vector3(4f, 6f, 8f);

            Vector3 projected = projector.WorldToProjectionMatrix.MultiplyPoint3x4(new Vector3(2f, 3f, 4f));

            Assert.That(projected.x, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(projected.y, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(projected.z, Is.EqualTo(0.5f).Within(0.0001f));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RefreshReceivers_CollectsMeshSkinnedMeshAndTerrainSurfaces()
        {
            GameObject projectorObject = new(nameof(RefreshReceivers_CollectsMeshSkinnedMeshAndTerrainSurfaces));
            MeshProjector projector = projectorObject.AddComponent<MeshProjector>();
            projector.Size = new Vector3(20f, 20f, 20f);

            GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject skinnedObject = new("Skinned Surface");
            SkinnedMeshRenderer skinnedRenderer = skinnedObject.AddComponent<SkinnedMeshRenderer>();
            skinnedRenderer.sharedMesh = Object.Instantiate(meshObject.GetComponent<MeshFilter>().sharedMesh);

            TerrainData terrainData = new() { heightmapResolution = 33, size = new Vector3(10f, 1f, 10f) };
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.position = new Vector3(-5f, -0.5f, -5f);

            projector.RefreshReceivers();

            Assert.That(projector.ReceiverCount, Is.EqualTo(3));

            Object.DestroyImmediate(projectorObject);
            Object.DestroyImmediate(meshObject);
            Object.DestroyImmediate(skinnedRenderer.sharedMesh);
            Object.DestroyImmediate(skinnedObject);
            Object.DestroyImmediate(terrainObject);
            Object.DestroyImmediate(terrainData);
        }

        [Test]
        public void PublicApi_DoesNotExposeMutableMaterial()
        {
            Assert.That(typeof(MeshProjector).GetProperty("Material"), Is.Null);
            Assert.That(typeof(MeshProjector).GetField("material"), Is.Null);
        }

        [Test]
        public void RefreshReceivers_WhenNothingChanged_ReusesTerrainMesh()
        {
            GameObject projectorObject = new(nameof(RefreshReceivers_WhenNothingChanged_ReusesTerrainMesh));
            MeshProjector projector = projectorObject.AddComponent<MeshProjector>();
            projector.Size = new Vector3(20f, 20f, 20f);

            TerrainData terrainData = new() { heightmapResolution = 33, size = new Vector3(10f, 1f, 10f) };
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.position = new Vector3(-5f, -0.5f, -5f);

            projector.RefreshReceivers();
            Mesh firstMesh = GetTerrainMeshes(projector)[terrainObject.GetComponent<Terrain>()];
            projector.RefreshReceivers();
            Mesh secondMesh = GetTerrainMeshes(projector)[terrainObject.GetComponent<Terrain>()];

            Assert.That(secondMesh, Is.SameAs(firstMesh));

            Object.DestroyImmediate(projectorObject);
            Object.DestroyImmediate(terrainObject);
            Object.DestroyImmediate(terrainData);
        }

        [Test]
        public void RefreshReceivers_WithRotatedProjection_UsesTransformedVolumeBounds()
        {
            GameObject projectorObject = new(nameof(RefreshReceivers_WithRotatedProjection_UsesTransformedVolumeBounds));
            MeshProjector projector = projectorObject.AddComponent<MeshProjector>();
            projector.Size = new Vector3(8f, 1f, 1f);
            projectorObject.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            GameObject receiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            receiver.transform.position = new Vector3(2.5f, 0f, -2.5f);
            receiver.transform.localScale = Vector3.one * 0.25f;

            projector.RefreshReceivers();

            Assert.That(projector.ReceiverCount, Is.EqualTo(1));

            Object.DestroyImmediate(projectorObject);
            Object.DestroyImmediate(receiver);
        }

        private static Dictionary<Terrain, Mesh> GetTerrainMeshes(MeshProjector projector)
        {
            FieldInfo field = typeof(MeshProjector).GetField(
                "_terrainMeshes",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (Dictionary<Terrain, Mesh>)field.GetValue(projector);
        }
    }
}
