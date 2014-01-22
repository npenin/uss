using System;

namespace Evaluant.Uss.Sync
{

    public class ProgressEventArgs : EventArgs
    {
        internal ProgressEventArgs(int current, string message, int upperBound)
        {
            this.current = current;
            this.message = message;
            this.upperBound = upperBound;
        }

        protected int current;

        /// <summary>
        /// Gets the current amount of work done by the operation. This is always less than or equal to <paramref name="UpperBound"/>.
        /// </summary>
        /// <value>The current.</value>
        public int Current
        {
            get
            {
                return current;
            }
        }

        protected string message;

        /// <summary>
        /// Gets or sets optional additional information regarding the operation's progress. 
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return message;
            }
        }

        protected int upperBound;

        /// <summary>
        /// Gets the total amount of work required to be done by the operation. 
        /// </summary>
        /// <value>The upper bound.</value>
        public int UpperBound
        {
            get
            {
                return upperBound;
            }
        }

        protected bool cancel;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ProgressEventArgs"/> is canceled.
        /// </summary>
        /// <value><c>true</c> if canceled; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }

    }

}
