using GLTF.Extensions;
using GLTF.Math;
using GLTF.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityHcap.Scripts.HcapExtensions
{
    public class HcapDecodeMinMaxExtension : IExtension
    {
        public Vector3 DecodeMin;
        public Vector3 DecodeMax;

        public HcapDecodeMinMaxExtension(Vector3 decodeMin, Vector3 decodeMax)
        {
            DecodeMin = decodeMin;
            DecodeMax = decodeMax;
        }

        public static HcapDecodeMinMaxExtension Deserialize(GLTFRoot root, JsonReader reader)
        {
            Vector3 decodeMin = Vector3.Zero;
            Vector3 decodeMax = Vector3.Zero;
            if (reader.Read() &&
                reader.TokenType == JsonToken.PropertyName &&
                reader.Value.ToString() == "HCAP_holovideo")
            {
                reader.Read();
                while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
                {
                    string str = reader.Value.ToString();
                    switch (str)
                    {
                        case "decodeMin":
                            decodeMin = reader.ReadAsVector3();
                            break;
                        case "decodeMax":
                            decodeMax = reader.ReadAsVector3();
                            break;
                    }
                }
            }

            return new HcapDecodeMinMaxExtension(decodeMin, decodeMax);
        }

        public JProperty Serialize()
        {
            throw new System.NotImplementedException();
        }

        public IExtension Clone(GLTFRoot root) =>
            (IExtension) new HcapDecodeMinMaxExtension(
                new Vector3(DecodeMin),
                new Vector3(DecodeMax));
    }
}