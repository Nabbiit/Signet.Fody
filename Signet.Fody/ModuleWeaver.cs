using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Fody;
using Mono.Cecil;

public sealed class ModuleWeaver : BaseModuleWeaver
{
    public override bool ShouldCleanReference => true;

    public override void Execute()
    {
        switch (GetAttribute("From", "SVN"))
        {
            case "Git": Git(); break;
            case "SVN": SVN(); break;
            default: WriteError("配置项“From”不允许指定为“Git”或“SVN”以外的值。"); break;
        }
    }

    public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();

    private void Git() => SetAttribute<AssemblyInformationalVersionAttribute>(GetLog());

    private void SVN() => SetAttribute<AssemblyInformationalVersionAttribute>(GetInfo());

    private string GetLog()
    {
        var arguments = $"log -1 --pretty=format:\"%h %ai\" -- \"{GetDirectory()}\"";
        var output = GetOutput("git.exe", arguments);

        if (string.IsNullOrWhiteSpace(output))
            WriteError($"执行“git.exe {arguments}”命令后未能获取到有效信息。");

        return output;
    }

    private string GetInfo()
    {
        var sb = new StringBuilder();
        var directory = GetDirectory();
        var revision = GetAttribute("Revision", "Head");

        foreach (var showItem in new string[] { "last-changed-revision", "last-changed-date", })
        {
            var arguments = $"info \"{directory}\" -r {revision} --show-item {showItem}";
            var output = GetOutput("svn.exe", arguments);

            if (string.IsNullOrWhiteSpace(output))
                WriteError($"执行“svn.exe {arguments}”命令后未能获取到有效信息。");

            sb.Append($"{output.Trim()} ");
        }

        return sb.ToString().Trim();
    }

    private T Throw<T>(string message)
    {
        WriteError(message);

        return default;
    }

    private string GetDirectory() => (GetAttribute("Level", "Project")) switch
    {
        "Project" => ProjectDirectoryPath,
        "Solution" => SolutionDirectoryPath,
        _ => Throw<string>("配置项“From”不允许指定为“Git”或“SVN”以外的值。"),
    };

    private string GetOutput(string fileName, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,

                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                },
            };

            process.Start();
            process.WaitForExit();

            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(error))
                WriteError($"执行“{fileName} {arguments}”命令时发生错误：{Environment.NewLine}{error}");

            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception exc)
        {
            WriteError($"执行“{fileName} {arguments}”命令时产生异常：{Environment.NewLine}{exc}");

            return default;
        }
    }

    private void SetAttribute<T>(string value)
    {
        var type = typeof(T);
        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;
        var constructorArgument = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, value);

        foreach (var attribute in customAttributes)
        {
            if (attribute.AttributeType.Name == type.Name)
            {
                attribute.ConstructorArguments[0] = constructorArgument;

                return;
            }
        }

        customAttributes.Add(new CustomAttribute(ModuleDefinition.ImportReference(type.GetConstructors()[0])) { ConstructorArguments = { constructorArgument, }, });
    }

    private string GetAttribute(string name, string defaultValue) => (Config.Attribute(name) is XAttribute attribute) ? attribute.Value : defaultValue;
}
