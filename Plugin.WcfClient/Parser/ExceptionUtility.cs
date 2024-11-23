using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Plugin.WcfClient.Parser
{
	/// <summary>Утилиты работы с исключительными ситуациями</summary>
	internal class ExceptionUtility
	{
		/// <summary>Выполнить действие с файловой системой и обработать исключительные ситуации</summary>
		/// <remarks>Обрабатываются исключения: IOException, UnauthorizedAccessException</remarks>
		/// <param name="func">Делегат на действие с файловой системой</param>
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

		/// <summary>Проверка исключения на фатальное, после которого дальнейшее выполнение кода невозможно</summary>
		/// <param name="exception">Исключение для проверки</param>
		/// <returns>Исключение фатальное</returns>
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