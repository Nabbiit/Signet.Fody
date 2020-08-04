using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Fody;
using Xunit;

public sealed class ModuleWeaverTests
{
    [Fact]
    public void Attribute()
    {
        // 使用 Signet.Tests 项目的版本信息
        var moduleWeaver = new ModuleWeaver
        {
            Config = XElement.Parse("<Signet From=\"Git\" />"),
            ProjectDirectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))),
        };

        var testResult = moduleWeaver.ExecuteTestRun("AssemblyToProcess.dll", runPeVerify: false, ignoreCodes: new[] { "0x8013129D" });

        if (testResult.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is AssemblyInformationalVersionAttribute attribute)
        {
            var version = attribute.InformationalVersion;

            Assert.False(string.IsNullOrWhiteSpace(version), version);
        }
        else
        {
            Assert.True(default, $"未能在 AssemblyToProcess.dll 程序集上找到 AssemblyInformationalVersionAttribute 特性...");
        }
    }
}
