using System;
using GLTF.Extensions;
using GLTF.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityHcap.Scripts.HcapExtensions
{
    public class HcapMeshExtension : IExtension
    {
        public AccessorId Position;
        public AccessorId Texcoord_0;
        public AccessorId Indices;
        public MaterialId Material;

        public HcapMeshExtension(AccessorId position, AccessorId texcoord_0, AccessorId indices, MaterialId material)
        {
            Position = position;
            Texcoord_0 = texcoord_0;
            Indices = indices;
            Material = material;
        }

        public static HcapMeshExtension Deserialize(GLTFRoot root, JsonReader reader)
        {
            AccessorId position = null;
            AccessorId texcoord_0 = null;
            AccessorId indices = null;
            MaterialId material = null;
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
                        case "attributes":
                            if (reader.Read() && reader.TokenType == JsonToken.StartObject)
                            {
                                while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
                                {
                                    switch (reader.Value.ToString())
                                    {
                                        case "POSITION":
                                            position = AccessorId.Deserialize(root, reader);
                                            break;
                                        case "TEXCOORD_0":
                                            texcoord_0 = AccessorId.Deserialize(root, reader);
                                            break;
                                    }
                                }

                                if (reader.TokenType != JsonToken.EndObject)
                                {
                                    throw new Exception(
                                        "Error by reading the property meshes.primitives.extensions.HCAP_holovideo.attributes.");
                                }
                            }

                            break;
                        case "indices":
                            indices = AccessorId.Deserialize(root, reader);
                            break;
                        case "material":
                            material = MaterialId.Deserialize(root, reader);
                            break;
                    }
                }
            }

            return new HcapMeshExtension(position, texcoord_0, indices, material);
        }

        public JProperty Serialize()
        {
            throw new System.NotImplementedException();
        }

        public IExtension Clone(GLTFRoot root) =>
            (IExtension) new HcapMeshExtension(
                new AccessorId(Position, root), 
                new AccessorId(Texcoord_0, root),
                new AccessorId(Indices, root), 
                new MaterialId(Material, root));
    }
}