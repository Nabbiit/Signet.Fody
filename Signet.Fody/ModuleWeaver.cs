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
        switch (GetConfig("From", "SVN"))
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
        var sb = new StringBuilder(48);
        var arguments = $"info \"{GetDirectory()}\" -r {GetConfig("Revision", "Head")} --show-item";

        if (int.TryParse(GetOutput("svn.exe", $"{arguments} last-changed-revision"), out var revision))
        {
            sb.Append($"{revision} ");
        }
        else
        {
            WriteError($"执行“svn.exe {arguments} last-changed-revision”命令后未能获取到有效信息。");
        }

        if (DateTime.TryParse(GetOutput("svn.exe", $"{arguments} last-changed-date"), out var dateTime))
        {
            sb.Append((GetConfig("Kind", "Local")) switch
            {
                "Local" => dateTime.ToLocalTime().ToString("O"),
                "UTC" => dateTime.ToUniversalTime().ToString("O"),
                _ => Throw<string>("配置项“Kind”不允许指定为“Local”或“UTC”以外的值。"),
            });
        }
        else
        {
            WriteError($"执行“svn.exe {arguments} last-changed-date”命令后未能获取到有效信息。");
        }

        return sb.ToString();
    }

    private T Throw<T>(string message)
    {
        WriteError(message);

        return default;
    }

    private string GetDirectory() => (GetConfig("Level", "Project")) switch
    {
        "Project" => ProjectDirectoryPath,
        "Solution" => SolutionDirectoryPath,
        _ => Throw<string>("配置项“Level”不允许指定为“Project”或“Solution”以外的值。"),
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

    private string GetConfig(string name, string defaultValue) => (Config.Attribute(name) is XAttribute attribute) ? attribute.Value : defaultValue;
}
