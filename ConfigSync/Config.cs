using YamlDotNet.Serialization;
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
		var expandedConfig = new Config
		{
			ConfigBackupDir = collapsedConfig.ConfigBackupDir
		};

		foreach (var (appName, appConfig) in collapsedConfig.Configs)
		{
			var app = new Application
			{
				// 1. Replace token with expanded value
				// 2. Replace any env(...) with environment variables
				ConfigLocation = osConfig.ExpandEnvVariables(osConfig.ExpandTokens(appConfig.ConfigLocation)),
				Ignore = appConfig.Ignore,
				NoProcess = appConfig.NoProcess
			};
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
