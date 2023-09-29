using System.Collections.Generic;
using System.Linq;
using GLTF.Math;
using GLTF.Schema;
using Newtonsoft.Json.Linq;

namespace UnityHcap.Scripts.HcapExtensions
{
    public class HcapExtension : IExtension
    {
        public static readonly double FRAMERATE_DEFAULT = 30;
        public static readonly Vector3 BOUNDING_MAX_DEFAULT = new Vector3(1.0f, 1.0f, 1.0f);
        public static readonly Vector3 BOUNDING_MIN_DEFAULT = new Vector3(-1.0f, -1.0f, -1.0f);

        public List<HcapTimeline> Timeline;
        public Vector3 BoundingMax;
        public Vector3 BoundingMin;
        public double Framerate;
        public List<MeshId> Keyframes;
        public long MaxIndexCount;
        public long MaxVertexCount;

        public HcapExtension(List<HcapTimeline> timeline, 
            Vector3 boundingMax,
            Vector3 boundingMin,
            double framerate,
            List<MeshId> keyframes,
            long maxIndexCount,
            long maxVertexCount)
        {
            Timeline = timeline;
            BoundingMax = boundingMax;
            BoundingMin = boundingMin;
            Framerate = framerate;
            Keyframes = keyframes;
            MaxIndexCount = maxIndexCount;
            MaxVertexCount = maxVertexCount;
        }

        public JProperty Serialize()
        {
            JArray timelineArray = new JArray(Timeline.Select(x => (object) x.Serialize()).ToArray());
            JArray keyframesArray = new JArray(Keyframes.Select(x => (object) x.Id).ToArray());
            
            return new JProperty("HCAP_holovideo", (object) new JObject(new object[7]
            {
                (object) new JProperty("timeline", (object) timelineArray),
                (object) new JProperty("boundingMax", (object) new JArray(new object[3]
                {
                    (object) this.BoundingMax.X,
                    (object) this.BoundingMax.Y,
                    (object) this.BoundingMax.Z
                })),
                (object) new JProperty("boundingMin", (object) new JArray(new object[3]
                {
                    (object) this.BoundingMin.X,
                    (object) this.BoundingMin.Y,
                    (object) this.BoundingMin.Z
                })),
                (object) new JProperty("framerate", (object) this.Framerate),
                (object) new JProperty("keyframes", (object) keyframesArray),
                (object) new JProperty("maxIndexCount", (object) this.MaxIndexCount),
                (object) new JProperty("maxVertexCount", (object) this.MaxVertexCount)
            }));
        }

        public IExtension Clone(GLTFRoot root) =>
            (IExtension) new HcapExtension(
                new List<HcapTimeline>(Timeline),
                new Vector3(BoundingMax),
                new Vector3(BoundingMin),
                Framerate,
                new List<MeshId>(Keyframes),
                MaxIndexCount,
                MaxVertexCount
            );
    }
}