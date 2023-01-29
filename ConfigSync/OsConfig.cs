using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConfigSync;

public class OsConfig
{
	public enum OperatingSystem
	{
		Windows,
		Linux,
		MacOS,
		Unknown
	}

	private readonly Dictionary<string, string> _tokenMap;
	private readonly Dictionary<string, string> _envOverrides;
	private readonly OperatingSystem _hostOs;

	public OsConfig(TokenMap tokenMap, Dictionary<string, string>? envOverrides = null)
	{
		_hostOs = GetOs();
		_envOverrides = envOverrides ?? new Dictionary<string, string>();
		_tokenMap = ExpandTokenMap(tokenMap);
	}

	private static OperatingSystem GetOs()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return OperatingSystem.Windows;
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return OperatingSystem.MacOS;
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return OperatingSystem.Linux;
		}
		else
		{
			return OperatingSystem.Unknown;
		}
	}

	private Dictionary<string, string> ExpandTokenMap(TokenMap tokenMap)
	{
		var expandedTokenMap = new Dictionary<string, string>();
		var envVarRegex = new Regex(@"env\(<variable>\)");

		foreach (var (token, value) in tokenMap.ForOs(_hostOs))
		{
			var expandedValue = value;
			var matches = envVarRegex.Matches(value);
			foreach (Match match in matches)
			{
				var envVar = match.Groups["variable"].Value;
				expandedValue = expandedValue.Replace($"env({envVar})", GetEnvVar(envVar));
			}
			expandedTokenMap.Add(token, expandedValue);
		}

		return expandedTokenMap;
	}

	private string GetEnvVar(string variable)
	{
		if (_envOverrides.TryGetValue(variable, out var value))
		{
			return value;
		}
		else
		{
			return Environment.GetEnvironmentVariable(variable) ?? "";
		}
	}

	public string ExpandEnvVariables(string text)
	{
		var envVarRegex = new Regex(@"env\((?<variable>\w+)\)");
		var matches = envVarRegex.Matches(text);
		var expandedValue = text;

		foreach (Match match in matches)
		{
			var envVar = match.Groups["variable"].Value;
			expandedValue = expandedValue.Replace($"env({envVar})", GetEnvVar(envVar));
		}

		return expandedValue;
	}

	public string ExpandVariables(string text)
	{
		var expanded = text;

		foreach (var (token, value) in _tokenMap)
		{
			expanded = expanded.Replace($"${token}", value);
		}

		return expanded;
	}

	public string CollapseVariables(string text)
	{
		// NOTE:  When collapsing variables, make sure to do $homeDir last because most other dirs
		//        will be children of $homeDir.
		// NOTE:  On Windows, search should be case-insensitive and directory separators can be
		//        either '/' or '\'.
		// BUG:  "c:/Users/Micha" is not being replaced by "$homeDir"
		var collapsed = text;

		foreach (var (token, value) in _tokenMap)
		{
			if (_hostOs == OperatingSystem.Windows)
			{
				collapsed = Regex.Replace(collapsed, value, token, RegexOptions.IgnoreCase);
				collapsed = Regex.Replace(collapsed, value.Replace("\\", "/"), token, RegexOptions.IgnoreCase);
			}
			else
			{
				collapsed = Regex.Replace(collapsed, value, token);
			}
		}

		return collapsed;
	}
}
