using System.Reflection;

[assembly: AssemblyTitle("VersionOne.ServerConnector")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("..\\..\\..\\Common\\SigningKey\\VersionOne.snk")]
[assembly: AssemblyKeyName("")]
#endif