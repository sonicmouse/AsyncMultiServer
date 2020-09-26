using System;

namespace MultiServerExample.Base
{
	public class AsyncClientBaseErrorEventArgs : EventArgs
	{
		public AsyncClientBaseErrorEventArgs(Exception ex, string message)
		{
			Message = message;
			Exception = ex;
		}

		public Exception Exception { get; private set; }

		public string Message { get; private set; }
	}
}
