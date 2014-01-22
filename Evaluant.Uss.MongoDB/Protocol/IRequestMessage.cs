using System.IO;

namespace Evaluant.Uss.MongoDB.Protocol
{
    /// <summary>
    /// A Message that is to be written to the database.
    /// </summary>
    public interface IRequestMessage
    {
        void Write (Stream stream);

        void Write(Stream networkStream, TextWriter log);
    }
}
