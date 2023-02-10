/* this is generated by nino */
namespace TaoTie
{
    public partial class ConfigAttackSphere
    {
        public static ConfigAttackSphere.SerializationHelper NinoSerializationHelper = new ConfigAttackSphere.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ConfigAttackSphere>
        {
            #region NINO_CODEGEN
            public override void Serialize(ConfigAttackSphere value, Nino.Serialization.Writer writer)
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
                writer.WriteCommonVal<TaoTie.BaseValue>(value.Radius);
            }

            public override ConfigAttackSphere Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ConfigAttackSphere value = new ConfigAttackSphere();
                reader.DecompressAndReadEnum<TaoTie.CheckHitLayerType>(ref value.CheckHitLayerType);
                value.HitScene = TaoTie.ConfigHitScene.NinoSerializationHelper.Deserialize(reader);
                value.Born = reader.ReadCommonVal<TaoTie.ConfigBornType>();
                value.Radius = reader.ReadCommonVal<TaoTie.BaseValue>();
                return value;
            }
            #endregion
        }
    }
}