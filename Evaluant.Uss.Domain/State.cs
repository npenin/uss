namespace Evaluant.Uss
{
	public enum State
	{
		New,

		// Fixed states
		UpToDate,
		Modified,
		Deleted,

		// Serializing states
		Deleting,
		Updating,
		Creating,

		Unknown
	}
}
