using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("VersionOne.ServerConnector")]
[assembly: InternalsVisibleTo("VersionOne.ServiceHost.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("..\\..\\..\\Common\\SigningKey\\VersionOne.snk")]
[assembly: AssemblyKeyName("")]
#endif