using System;

namespace Plugin.WcfClient.Extensions
{
	internal static class AppDomainExtension
	{
		public static String GetClientAssemblyPath(this AppDomain domain)
			=> (String)domain.GetData("clientAssemblyPath");

		public static void SetClientAssemblyPath(this AppDomain domain, String assemblyPath)
			=> domain.SetData("clientAssemblyPath", assemblyPath);
	}
}