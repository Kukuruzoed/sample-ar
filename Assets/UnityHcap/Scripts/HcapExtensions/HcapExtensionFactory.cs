using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GLTF.Extensions;
using GLTF.Math;
using GLTF.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityHcap.Scripts.HcapExtensions
{
    public class HcapExtensionFactory : ExtensionFactory
    {
        public const string EXTENSION_NAME = "HCAP_holovideo";

        public HcapExtensionFactory() =>
            this.ExtensionName = EXTENSION_NAME;
        
        public override IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
        {
            List<HcapTimeline> timeline = new List<HcapTimeline>();
            Vector3 boundingMax = HcapExtension.BOUNDING_MAX_DEFAULT;
            Vector3 boundingMin = HcapExtension.BOUNDING_MIN_DEFAULT;
            double framerate = HcapExtension.FRAMERATE_DEFAULT;
            List<MeshId> keyframes = new List<MeshId>();
            long maxIndexCount = 0;
            long maxVertexCount = 0;
            
            if (extensionToken != null)
            {
                Debug.WriteLine(extensionToken.Value.ToString());
                Debug.WriteLine((object) extensionToken.Value.Type);
                JsonReader timelineReader = extensionToken.Value[(object) "timeline"].CreateReader();
                timeline = timelineReader.ReadList<HcapTimeline>((Func<HcapTimeline>) (() => HcapTimeline.Deserialize(root, timelineReader)));
                boundingMax = extensionToken.Value[(object) "boundingMax"].DeserializeAsVector3();
                boundingMin = extensionToken.Value[(object) "boundingMin"].DeserializeAsVector3();
                framerate = extensionToken.Value[(object) "framerate"].DeserializeAsDouble();
                keyframes = extensionToken.Value[(object) "keyframes"].CreateReader().ReadInt32List().Select((int x) =>
                {
                    MeshId mid = new MeshId();
                    mid.Id = x;
                    mid.Root = root;
                    return mid;
                }).ToList();
                maxIndexCount = extensionToken.Value[(object) "maxIndexCount"].DeserializeAsInt();
                maxVertexCount = extensionToken.Value[(object) "maxVertexCount"].DeserializeAsInt();
            }
            
            return (IExtension) new HcapExtension(
                timeline,
                boundingMax,
                boundingMin,
                framerate,
                keyframes,
                maxIndexCount,
                maxVertexCount
            );
        }
    }
}