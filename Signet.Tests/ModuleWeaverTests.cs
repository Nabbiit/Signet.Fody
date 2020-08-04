using System.Reflection;
using Fody;
using Xunit;

public sealed class ModuleWeaverTests
{
    [Fact]
    public void WithFields()
    {
        var moduleWeaver = new ModuleWeaver();
        var testResult = moduleWeaver.ExecuteTestRun("AssemblyToProcess.dll", ignoreCodes: new[] { "0x8013129D" });
    }
}
