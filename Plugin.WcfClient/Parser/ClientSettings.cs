using System;
using System.Reflection;
using System.Security.Permissions;
using Plugin.WcfClient.Extensions;

namespace Plugin.WcfClient.Parser
{
	internal static class ClientSettings
	{
		private static Assembly _clientAssembly;

		public static Assembly ClientAssembly
		{
			get
			{
				if(ClientSettings._clientAssembly == null)
				{
					String codeBase = AppDomain.CurrentDomain.GetClientAssemblyPath();
					if(codeBase != null)
					{
						FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, codeBase);
						permission.Assert();

						ClientSettings._clientAssembly = Assembly.Load(new AssemblyName() { CodeBase = codeBase, });
					}
				}
				return ClientSettings._clientAssembly;
			}
		}

		public static Type GetType(String typeName)
		{
			Assembly asm = ClientSettings.ClientAssembly;
			if(asm == null)
				return null;

			Type type = asm.GetType(typeName);
			return type == null
				? Type.GetType(typeName)
				: type;
		}
	}
}