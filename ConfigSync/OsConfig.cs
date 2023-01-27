using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConfigSync;

public class OsConfig
{
	private enum OperatingSystem
	{
		Windows,
		Linux,
		MacOS,
		Unknown
	}

	private readonly Dictionary<string, string> _tokenMap;
	private readonly Dictionary<string, string> _envOverrides;
	private readonly OperatingSystem _hostOs;

	public OsConfig(Dictionary<string, string>? envOverrides = null)
	{
		_hostOs = GetOs();
		_envOverrides = envOverrides ?? new Dictionary<string, string>();
		_tokenMap = GetTokenMap();
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

	private Dictionary<string, string> GetTokenMap()
	{
		var tokens = new Dictionary<string, string>();

		// $configDir
		var token = "$configDir";
		if (_hostOs == OperatingSystem.Windows)
		{
			tokens.Add(token, GetEnvVar("AppData"));
		}
		else
		{
			var value = string.IsNullOrEmpty(GetEnvVar("XDG_CONFIG_HOME"))
				? GetEnvVar("XDG_CONFIG_HOME")
				: Path.Combine(GetEnvVar("HOME"), ".config");
			tokens.Add(token, value);
		}

		// $homeDir
		token = "$homeDir";
		if (_hostOs == OperatingSystem.Windows)
		{
			tokens.Add(token, GetEnvVar("UserProfile"));
		}
		else
		{
			tokens.Add(token, GetEnvVar("HOME"));
		}

		return tokens;
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

	public string ExpandVariables(string text)
	{
		var expanded = text;

		foreach (var (token, value) in _tokenMap)
		{
			expanded = expanded.Replace(token, value);
		}

		return expanded;
	}

	public string CollapseVariables(string text)
	{
		// NOTE:  When collapsing variables, make sure to do $homeDir last because most other dirs
		//        will be children of $homeDir.
		// NOTE:  On Windows, search should be case-insensitive and directory separators can be
		//        either '/' or '\'.
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
