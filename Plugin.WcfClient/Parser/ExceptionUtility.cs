using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Plugin.WcfClient.Parser
{
	/// <summary>Exception handling utilities</summary>
	internal class ExceptionUtility
	{
		/// <summary>Perform a file system action and handle exceptions</summary>
		/// <remarks>Handled exceptions: IOException, UnauthorizedAccessException</remarks>
		/// <param name="func">Delegate for file system action</param>
		public static Boolean InvokeFSAction(Action func)
		{
			DialogResult result;
			do
			{
				try
				{
					func();
					return true;
				} catch(IOException exc)
				{
					result = MessageBox.Show(exc.Message, "IOException", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
				} catch(UnauthorizedAccessException exc)
				{
					result = MessageBox.Show(exc.Message, "UnauthorizedAccessException", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
				}
			} while(result != DialogResult.Cancel);
			return false;
		}

		/// <summary>Checking for a fatal exception, after which further code execution is impossible</summary>
		/// <param name="exception">Exception to check</param>
		/// <returns>Fatal exception</returns>
		public static Boolean IsFatal(Exception exception)
		{
			while(exception != null)
			{
				if((exception is OutOfMemoryException && !(exception is InsufficientMemoryException)) || exception is ThreadAbortException || exception is AccessViolationException || exception is SEHException)
					return true;
				if(!(exception is TypeInitializationException) && !(exception is TargetInvocationException))
					break;
				exception = exception.InnerException;
			}
			return false;
		}
	}
}