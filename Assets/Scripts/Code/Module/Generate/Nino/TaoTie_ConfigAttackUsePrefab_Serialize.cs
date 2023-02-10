/* this is generated by nino */
namespace TaoTie
{
    public partial class ConfigAttackUsePrefab
    {
        public static ConfigAttackUsePrefab.SerializationHelper NinoSerializationHelper = new ConfigAttackUsePrefab.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ConfigAttackUsePrefab>
        {
            #region NINO_CODEGEN
            public override void Serialize(ConfigAttackUsePrefab value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWriteEnum<TaoTie.CheckHitLayerType>(value.CheckHitLayerType);
                TaoTie.ConfigHitScene.NinoSerializationHelper.Serialize(value.HitScene, writer);
                writer.WriteCommonVal<TaoTie.ConfigBornType>(value.Born);
                writer.Write(value.PrefabPathName);
            }

            public override ConfigAttackUsePrefab Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ConfigAttackUsePrefab value = new ConfigAttackUsePrefab();
                reader.DecompressAndReadEnum<TaoTie.CheckHitLayerType>(ref value.CheckHitLayerType);
                value.HitScene = TaoTie.ConfigHitScene.NinoSerializationHelper.Deserialize(reader);
                value.Born = reader.ReadCommonVal<TaoTie.ConfigBornType>();
                value.PrefabPathName = reader.ReadString();
                return value;
            }
            #endregion
        }
    }
}