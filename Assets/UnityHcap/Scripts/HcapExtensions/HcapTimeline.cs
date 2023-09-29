using GLTF.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityHcap.Scripts.HcapExtensions
{
    public class HcapTimeline : IExtension
    {
        public ImageId Image;
        public int StartFrame;
        public NodeId TargetNode;

        public HcapTimeline(ImageId image, int startFrame, NodeId targetNode)
        {
            Image = image;
            StartFrame = startFrame;
            TargetNode = targetNode;
        }

        public static HcapTimeline Deserialize(GLTFRoot root, JsonReader reader)
        {
            ImageId imageId = null;
            int startFrame = -1;
            NodeId targetNode = null;
            while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
            {
                string str = reader.Value.ToString();
                switch (str)
                {
                    case "image":
                        imageId = ImageId.Deserialize(root, reader);
                        break;
                    case "startFrame":
                        startFrame = (int) reader.ReadAsInt32();
                        break;
                    case "targetNode":
                        targetNode = NodeId.Deserialize(root, reader);
                        break;
                }
            }
            return new HcapTimeline(imageId, startFrame, targetNode);
        }

        public JProperty Serialize() =>
            new JProperty("timeline", (object) new JObject(new object[3]
            {
                (object) new JProperty("image", (object) this.Image.Id),
                (object) new JProperty("startFrame", (object) this.StartFrame),
                (object) new JProperty("targetNode", (object) this.TargetNode.Id)
            }));

        public IExtension Clone(GLTFRoot root) => 
            (IExtension) new HcapTimeline(new ImageId(Image, root), StartFrame, new NodeId(TargetNode, root));
    }
}