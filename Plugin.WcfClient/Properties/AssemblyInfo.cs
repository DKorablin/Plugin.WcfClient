using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("6943c22a-2efe-4321-a793-7d4ab0f82a7d")]
[assembly: System.CLSCompliant(true)]

#if NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=96")]
#else

[assembly: AssemblyTitle("Plugin.WcfClient")]
[assembly: AssemblyDescription("WS\\WCF Client")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyProduct("Plugin.WcfClient")]
[assembly: AssemblyCopyright("Copyright © WcfClient authors Danila Korablin 2012-2024")]
#endif

/*if $(ConfigurationName) == Release (
..\..\..\..\ILMerge.exe "/out:$(ProjectDir)..\bin\$(TargetFileName)" "$(TargetDir)$(TargetFileName)" "$(ProjectDir)Microsoft.VisualStudio.VirtualTreeGrid.dll" "/lib:..\..\..\SAL\bin"
)*/