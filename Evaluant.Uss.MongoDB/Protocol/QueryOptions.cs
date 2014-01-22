namespace Evaluant.Uss.MongoDB
{
    public enum QueryOptions {
        None = 0,
        TailableCursor = 2,
        SlaveOK = 4,
        NoCursorTimeout = 16
    }
}