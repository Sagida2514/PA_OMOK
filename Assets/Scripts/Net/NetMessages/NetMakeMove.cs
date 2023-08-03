using Unity.Networking.Transport;

public class NetSpawnStone : NetMessage
{
    public int teamId;
    public int posX;
    public int posY;
    
    public NetSpawnStone()
    {
        Code = OpCode.SPAWN_STONE;
    }

    public NetSpawnStone(DataStreamReader reader)
    {
        Code = OpCode.SPAWN_STONE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(posX);
        writer.WriteInt(posY);
        writer.WriteInt(teamId);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        posX = reader.ReadInt();
        posY = reader.ReadInt();
        teamId = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_SPAWN_STONE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_SPAWN_STONE?.Invoke(this, cnn);
    }
}