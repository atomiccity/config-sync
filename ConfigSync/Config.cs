using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigSync;

public class Config
{
	public class Application
	{
		public string ConfigLocation { get; set; }
	}

	public Dictionary<string, Application> Configs { get; set; }

	public static Config ReadConfig(string yamlString)
	{
		using var input = new StringReader(yamlString);
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		// Expand any necessary variables
		var osConfig = new OsConfig();
		var collapsedConfig = deserializer.Deserialize<Config>(input);
		var expandedConfig = new Config();

		foreach (var (appName, appConfig) in collapsedConfig.Configs)
		{
			var app = new Application { ConfigLocation = osConfig.ExpandVariables(appConfig.ConfigLocation) };
			expandedConfig.Configs.Add(appName, app);
		}

		return expandedConfig;
	}
}
