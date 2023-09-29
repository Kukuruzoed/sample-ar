using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GLTF;
using GLTF.Schema;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityHcap.Scripts.HcapExtensions;

namespace UnityHcap.Scripts
{
    public class HcapPlayer : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Material material;
        [SerializeField] private AssetReference videoRef;
        [SerializeField] private AssetReference hcapRef;
        [SerializeField] private AssetReference bin0Ref;
        [SerializeField] private AssetReference bin1Ref;

        private GameObject hcapObject;
        private MeshRenderer hcapRenderer;
        private MeshFilter hcapMeshFilter;

        private VideoClip videoClip;
        private TextAsset hcapFile;
        private RenderTexture renderTexture;
        private Texture2D texture;

        private GLTFRoot gltf;
        private HcapExtension hcapEx;

        private long lastFrame = -1;
        private long curMeshFrame = -1;

        private HcapBuffer[] cashedBuffers;

        private bool loaded = false;

        private IEnumerator Start()
        {
            yield return StartCoroutine(LoadResources());
            loaded = true;
        }

        private IEnumerator LoadResources()
        {
            AsyncOperationHandle handle;
            handle = videoRef.LoadAssetAsync<VideoClip>();
            yield return handle;
            videoClip = (VideoClip)handle.Result;

            handle = hcapRef.LoadAssetAsync<TextAsset>();
            yield return handle;
            hcapFile = (TextAsset)handle.Result;
            LoadHcapFile(hcapFile, out gltf);
            ReadHcapExtension(gltf, out hcapEx);
            InitBuffers(gltf, out cashedBuffers);
            SetupVideo(videoClip);
            PrepareRenderObject();

            handle = bin0Ref.LoadAssetAsync<TextAsset>();
            yield return handle;
            cashedBuffers[0].Bytes = ((TextAsset)handle.Result).bytes;

            handle = bin1Ref.LoadAssetAsync<TextAsset>();
            yield return handle;
            cashedBuffers[1].Bytes = ((TextAsset)handle.Result).bytes;

            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.frameReady += VideoPlayer_frameReady;
            videoPlayer.frameDropped += VideoPlayer_frameDropped;

            hcapRenderer.material.mainTexture = videoPlayer.texture;
        }

        private void VideoPlayer_frameDropped(VideoPlayer source)
        {
            LoadMesh(((int)videoPlayer.frame), hcapEx);

            Debug.Log("mesh frame - " + videoPlayer.frame + " dropped");
        }

        private void VideoPlayer_frameReady(VideoPlayer source, long frameIdx)
        {
            LoadMesh(((int)videoPlayer.frame), hcapEx);

            Debug.Log("mesh frame - " + videoPlayer.frame);
        }

        private void LoadHcapFile(TextAsset hcap, out GLTFRoot gltfRoot)
        {
            // ILoader fileLoader = new FileLoader(Path.GetDirectoryName(uri));
            byte[] myByteArray = hcap.bytes;
            MemoryStream stream = new MemoryStream();
            stream.Write(myByteArray, 0, myByteArray.Length);

            using (stream)
            {
                GLTFParser.ParseJson(stream, out gltfRoot);
            }
        }

        private void LoadMesh(int frame, HcapExtension hcap)
        {
            if (frame != -1)
            {
                HcapMeshExtension hcapMeshEx = HcapMeshExtension.Deserialize(
                        gltf,
                        ((DefaultExtension)hcap.Keyframes[frame].Value.Primitives[0].Extensions["HCAP_holovideo"])
                        .ExtensionData.CreateReader());

                hcapMeshFilter.mesh.Clear(false);
                hcapMeshFilter.mesh.SetVertices(GetVertices(gltf, hcapMeshEx.Position));
                hcapMeshFilter.mesh.SetUVs(0, GetUVs(gltf, hcapMeshEx.Texcoord_0));
                hcapMeshFilter.mesh.SetTriangles(GetTriangles(gltf, hcapMeshEx.Indices), 0);

                hcapRenderer.material.mainTexture = videoPlayer.texture;
            }
        }

        private void ReadHcapExtension(GLTFRoot gltfRoot, out HcapExtension hcapExtension)
        {
            ExtensionFactory extensionFactory = new HcapExtensionFactory();
            hcapExtension = (HcapExtension)extensionFactory.Deserialize(gltfRoot,
                ((DefaultExtension)gltfRoot.Extensions["HCAP_holovideo"]).ExtensionData);
        }

        private VideoClip LoadVideoClipFromResources(string uri) =>
            Resources.Load<VideoClip>(uri);

        private void SetupVideo(VideoClip clip)
        {
            renderTexture = new RenderTexture((int)clip.width, (int)clip.height, 3);
            texture = new Texture2D(renderTexture.width, 1, TextureFormat.RGB24, false);

            //videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            //videoPlayer.targetTexture = renderTexture;
            videoPlayer.isLooping = true;
            videoPlayer.clip = clip;
        }

        private void PrepareRenderObject()
        {
            hcapObject = new GameObject("hcapMesh");
            hcapObject.transform.SetParent(transform);
            hcapObject.transform.localScale = Vector3.one;
            hcapObject.transform.localPosition = Vector3.zero;
            hcapObject.transform.localRotation = Quaternion.identity;


            hcapRenderer = hcapObject.AddComponent<MeshRenderer>();
            hcapRenderer.material = material;
            hcapRenderer.material.mainTexture = renderTexture;

            hcapMeshFilter = hcapObject.AddComponent<MeshFilter>();
            hcapMeshFilter.mesh = new Mesh();
        }

        private void InitBuffers(GLTFRoot root, out HcapBuffer[] buffers)
        {
            buffers = new HcapBuffer[root.Buffers.Count];
            for (int i = 0; i < buffers.Length; i++)
            {
                buffers[i] = new HcapBuffer() { Bytes = new byte[0] };
            }
        }

        private HcapBuffer GetCashedBuffer(int index) =>
            cashedBuffers[index];

        private int ReadFrameMarker(Texture2D tex, int markerSize)
        {
            int frameMarker = 0;
            int step = tex.width / markerSize;
            for (int i = 0; i < markerSize; i++)
            {
                int x = (int)((i + 0.5f) * step);
                Debug.Log("pixel - " + tex.GetPixel(x, 0));
                if (tex.GetPixel(x, 0).r > 0.5f)
                {
                    frameMarker += 1 << i;
                }
            }
            return frameMarker;
        }

        private Vector3[] GetVertices(GLTFRoot root, AccessorId accessor)
        {
            Assert.IsTrue(accessor.Value.ComponentType == GLTFComponentType.UnsignedShort);
            Assert.IsTrue(accessor.Value.Type == GLTFAccessorAttributeType.VEC3);

            HcapDecodeMinMaxExtension decodeMinMax = HcapDecodeMinMaxExtension.Deserialize(
                root,
                ((DefaultExtension)accessor.Value.Extensions["HCAP_holovideo"]).ExtensionData.CreateReader());

            Vector3[] vertices = new Vector3[accessor.Value.Count];

            HcapBuffer buffer = GetCashedBuffer(accessor.Value.BufferView.Value.Buffer.Id);

            int binStart = (int)(accessor.Value.BufferView.Value.ByteOffset + accessor.Value.ByteOffset) / 2;
            int binLength = (int)accessor.Value.BufferView.Value.ByteLength / 2;

            const int dataSize = 3;
            int dataLength = binLength / dataSize;
            for (int i = 0; i < dataLength; ++i)
            {
                ArraySegment<ushort> segment =
                    new ArraySegment<ushort>(buffer.UShorts, binStart + i * dataSize, dataSize);
                float[] uncompressed = UncomressVec3(segment, decodeMinMax.DecodeMin, decodeMinMax.DecodeMax);
                vertices[i] = new Vector3(uncompressed[0], uncompressed[1], uncompressed[2]);
            }

            return vertices;
        }

        private float[] UncomressVec3(IReadOnlyList<ushort> segment, GLTF.Math.Vector3 min, GLTF.Math.Vector3 max)
        {
            Assert.IsTrue(segment.Count == 3);
            float[] uncompressed = {
                Uncompress(segment[0], min.X, max.X),
                Uncompress(segment[1], min.Y, max.Y),
                Uncompress(segment[2], min.Z, max.Z)
            };
            return uncompressed;
        }

        private Vector2[] GetUVs(GLTFRoot root, AccessorId accessor)
        {
            Assert.IsTrue(accessor.Value.ComponentType == GLTFComponentType.UnsignedShort);
            Assert.IsTrue(accessor.Value.Type == GLTFAccessorAttributeType.VEC2);

            Vector2[] uvs = new Vector2[accessor.Value.Count];

            HcapBuffer buffer = GetCashedBuffer(accessor.Value.BufferView.Value.Buffer.Id);

            int binStart = (int)(accessor.Value.BufferView.Value.ByteOffset + accessor.Value.ByteOffset) / 2;
            int binLength = (int)accessor.Value.BufferView.Value.ByteLength / 2;

            const int dataSize = 2;
            int dataLength = binLength / dataSize;
            for (int i = 0; i < dataLength; ++i)
            {
                ArraySegment<ushort> segment =
                    new ArraySegment<ushort>(buffer.UShorts, binStart + i * dataSize, dataSize);
                float[] uncompressed = UncomressVec2(segment, 0.0f, 1.0f);
                uvs[i] = new Vector2(uncompressed[0], 1.0f - uncompressed[1]);
            }

            return uvs;
        }

        private float[] UncomressVec2(IReadOnlyList<ushort> segment, float min, float max)
        {
            Assert.IsTrue(segment.Count == 2);
            float[] uncompressed = {
                Uncompress(segment[0], min, max),
                Uncompress(segment[1], min, max),
            };
            return uncompressed;
        }

        private int[] GetTriangles(GLTFRoot root, AccessorId accessor)
        {
            Assert.IsTrue(accessor.Value.ComponentType == GLTFComponentType.UnsignedShort);
            Assert.IsTrue(accessor.Value.Type == GLTFAccessorAttributeType.SCALAR);

            int[] tris = new int[accessor.Value.Count];

            HcapBuffer buffer = GetCashedBuffer(accessor.Value.BufferView.Value.Buffer.Id);

            int binStart = (int)(accessor.Value.BufferView.Value.ByteOffset + accessor.Value.ByteOffset) / 2;
            int binLength = (int)accessor.Value.BufferView.Value.ByteLength / 2;

            const int dataSize = 1;
            int dataLength = binLength / dataSize;
            for (int i = 0; i < dataLength; ++i)
            {
                tris[i] = buffer.UShorts[binStart + i * dataSize];
            }

            return tris;
        }

        private float Uncompress(ushort value, float min, float max)
        {
            float range = max - min;
            float valueNorm = (float)value / ushort.MaxValue;
            return valueNorm * range + min;
        }

        private long GetLastKeyframe(HcapExtension hcapEx, int frame)
        {
            return -1;
        }
    }
}
