using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("6943c22a-2efe-4321-a793-7d4ab0f82a7d")]
[assembly: System.CLSCompliant(true)]

#if NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=96")]
#else

[assembly: AssemblyDescription("WS\\WCF Client")]
[assembly: AssemblyCopyright("Copyright © WcfClient authors Danila Korablin 2012-2025")]
#endif

/*if $(ConfigurationName) == Release (
..\..\..\..\ILMerge.exe "/out:$(ProjectDir)..\bin\$(TargetFileName)" "$(TargetDir)$(TargetFileName)" "$(ProjectDir)Microsoft.VisualStudio.VirtualTreeGrid.dll" "/lib:..\..\..\SAL\bin"
)*/