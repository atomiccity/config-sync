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
		using (var input = new StringReader(yamlString))
		{
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();

			return deserializer.Deserialize<Config>(input);
		}
	}
}
