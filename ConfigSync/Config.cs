﻿using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigSync;

public class Config
{
	public class Application
	{
		public string ConfigLocation { get; init; } = string.Empty;
		public List<string> Ignore { get; init; } = new();
		public List<string> NoProcess { get; init; } = new();
	}

	public string ConfigBackupDir { get; private init; } = string.Empty;
	public Dictionary<string, Application> Configs { get; private init; } = new();

	public static Config ReadConfig(string yamlString, TokenMap tokenMap, Dictionary<string, string>? envOverrides = null)
	{
		using var input = new StringReader(yamlString);
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		// Expand any necessary variables
		var osConfig = new OsConfig(tokenMap, envOverrides);
		var collapsedConfig = deserializer.Deserialize<Config>(input);
		var expandedConfig = new Config();

		foreach (var (appName, appConfig) in collapsedConfig.Configs)
		{
			var app = new Application { ConfigLocation = osConfig.ExpandVariables(appConfig.ConfigLocation) };
			expandedConfig.Configs.Add(appName, app);
		}

		return expandedConfig;
	}

	public static Config Example()
	{
		return new Config
		{
			ConfigBackupDir = "configs",
			Configs = new Dictionary<string, Application>
			{
				{
					"MyApp",
					new Application
					{
						ConfigLocation = "$configDir/MyApp",
						Ignore = { "secret_data/secret.txt" },
						NoProcess = { "binary_file.bin" },
					}
				}
			}
		};
	}

	public string ToYaml()
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();
		return serializer.Serialize(this);
	}
}
