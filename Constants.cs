using System.Text;

namespace llp;

public static class Constants
{
    public static readonly string EOM = "<|EOM|>";
    public static readonly string ACK = "<|ACK|>";
    public static readonly byte[] EOMBytes = Encoding.UTF8.GetBytes(EOM);
    public static readonly byte[] ACKBytes = Encoding.UTF8.GetBytes(ACK);
    public static readonly string ERR_INVALID_IP = "INVALID IP ADRESS";
    public static readonly string ERR_BAD_INIT = "BAD SERVER INITIALIZATION";
    public static readonly string ERR_BAD_RESPONSE = "BAD SERVER RESPONSE";
    public static Stream ToStream(this string str)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(str);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}