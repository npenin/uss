namespace Evaluant.Uss.Commands
{
	/// <summary>
	/// Visitor pattern to execute concrete commands
	/// </summary>
	public interface ICommandProcessor
	{

		/// <summary>
		/// Gets or sets the new id if an entity gets created.
		/// </summary>
		/// <value>The new id.</value>
		string NewId { get; set; }

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(Command command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(CreateEntityCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(CompoundCreateCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(CompoundUpdateCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(DeleteEntityCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(CreateAttributeCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(UpdateAttributeCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(DeleteAttributeCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(CreateReferenceCommand command);

		/// <summary>
		/// Processes the specified command.
		/// </summary>
		/// <param name="command">The command.</param>
		void Process(DeleteReferenceCommand command);
	}
}
