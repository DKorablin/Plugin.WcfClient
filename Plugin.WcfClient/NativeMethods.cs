using System;
using System.Runtime.InteropServices;

namespace Plugin.WcfClient
{
	internal static class NativeMethods
	{
		private const String AdvApi32 = "Advapi32.dll";

		/// <summary>
		/// Registry entries subordinate to this key define the physical state of the computer, including data about the bus type, system memory, and installed hardware and software.
		/// It contains subkeys that hold current configuration data, including Plug and Play information (the Enum branch, which includes a complete list of all hardware that has ever been on the system), network logon preferences, network security information, software-related information (such as server names and the location of the server), and other system information.
		/// </summary>
		private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

		/// <summary>
		/// Registry entries subordinate to this key define the preferences of the current user. These preferences include the settings of environment variables, data about program groups, colors, printers, network connections, and application preferences.
		/// This key makes it easier to establish the current user's settings; the key maps to the current user's branch in HKEY_USERS.
		/// In HKEY_CURRENT_USER, software vendors store the current user-specific preferences to be used within their applications. Microsoft, for example, creates the HKEY_CURRENT_USER\Software\Microsoft key for its applications to use, with each application creating its own subkey under the Microsoft key.
		/// </summary>
		private static readonly UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

		/// <summary>Closes a handle to the specified registry key</summary>
		/// <param name="hKey">A handle to the open key to be closed</param>
		/// <returns>If the function succeeds, the return value is ERROR_SUCCESS</returns>
		[DllImport(NativeMethods.AdvApi32)]
		internal static extern UInt32 RegCloseKey(UIntPtr hKey);

		/// <summary>Opens the specified registry key. Note that key names are not case sensitive</summary>
		/// <param name="hKey">A handle to an open registry key</param>
		/// <param name="lpSubKey">The name of the registry subkey to be opened</param>
		/// <param name="ulOptions">Specifies the option to apply when opening the key</param>
		/// <param name="samDesired">
		/// A mask that specifies the desired access rights to the key to be opened.
		/// The function fails if the security descriptor of the key does not permit the requested access for the calling process
		/// </param>
		/// <param name="phkResult">
		/// A pointer to a variable that receives a handle to the opened key.
		/// If the key is not one of the predefined registry keys, call the RegCloseKey function after you have finished using the handle
		/// </param>
		/// <returns>If the function succeeds, the return value is ERROR_SUCCESS</returns>
		[DllImport(NativeMethods.AdvApi32)]
		internal static extern UInt32 RegOpenKeyEx(UIntPtr hKey, String lpSubKey, UInt32 ulOptions, Int32 samDesired, out UIntPtr phkResult);

		/// <summary>Retrieves the type and data for the specified value name associated with an open registry key</summary>
		/// <param name="hKey">A handle to an open registry key. The key must have been opened with the KEY_QUERY_VALUE access right</param>
		/// <param name="lpValueName">The name of the registry value</param>
		/// <param name="lpReserved">This parameter is reserved and must be NULL</param>
		/// <param name="lpType">A pointer to a variable that receives a code indicating the type of data stored in the specified value</param>
		/// <param name="lpData">A pointer to a buffer that receives the value's data. This parameter can be NULL if the data is not required</param>
		/// <param name="lpchData">A pointer to a variable that specifies the size of the buffer pointed to by the lpData parameter, in bytes. When the function returns, this variable contains the size of the data copied to lpData</param>
		/// <returns></returns>
		[DllImport(NativeMethods.AdvApi32)]
		internal static extern UInt32 RegQueryValueEx(UIntPtr hKey, String lpValueName, UInt32 lpReserved, ref UInt32 lpType, IntPtr lpData, ref Int32 lpchData);

		public static String GetRegistryValue(String path,String name)
		{
			String result = null;
			Int32 num = 1;
			Int32 num2 = 256;
			UIntPtr hKey = UIntPtr.Zero;
			IntPtr lpData = IntPtr.Zero;
			try
			{
				if(NativeMethods.RegOpenKeyEx(NativeMethods.HKEY_LOCAL_MACHINE, path, 0u, num | num2, out hKey) == 0u)
				{
					lpData = Marshal.AllocCoTaskMem(256);

					UInt32 type = 1u;
					Int32 dataSize = 256;
					if(NativeMethods.RegQueryValueEx(hKey, name, 0u, ref type, lpData, ref dataSize) == 0u && dataSize > 0 && dataSize <= 256)
						result = Marshal.PtrToStringAnsi(lpData, dataSize - 1);
				}
			}
			finally
			{
				if(hKey != UIntPtr.Zero)
					NativeMethods.RegCloseKey(hKey);
				if(lpData != IntPtr.Zero)
					Marshal.FreeCoTaskMem(lpData);
			}
			return result;
		}
	}
}