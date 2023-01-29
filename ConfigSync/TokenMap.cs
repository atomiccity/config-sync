using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigSync;

public class TokenMap
{
	public class Token
	{
		public string Name { get; init; } = string.Empty;
		public Dictionary<OsConfig.OperatingSystem, string> Values { get; init; } = new();
	}

	public List<Token> Tokens { get; init; } = new();

	public static TokenMap Default()
	{
		return new TokenMap
		{
			Tokens = new List<Token>
			{
				new Token
				{
					Name = "homeDir",
					Values = new Dictionary<OsConfig.OperatingSystem, string>
					{
						{ OsConfig.OperatingSystem.Linux, "env(HOME)" },
						{ OsConfig.OperatingSystem.Windows, "env(UserProfile)" },
						{ OsConfig.OperatingSystem.MacOS, "env(HOME)" },
					}
				},
				new Token
				{
					Name = "configDir",
					Values = new Dictionary<OsConfig.OperatingSystem, string>
					{
						{ OsConfig.OperatingSystem.Linux, "env(XDG_CONFIG_HOME)" },
						{ OsConfig.OperatingSystem.Windows, "env(AppData)" },
						{ OsConfig.OperatingSystem.MacOS, "env(HOME)/.config" }
					}
				},
				new Token
				{
					Name = "localConfigDir",
					Values = new Dictionary<OsConfig.OperatingSystem, string>
					{
						{ OsConfig.OperatingSystem.Linux, "env(XDG_CONFIG_HOME)" },
						{ OsConfig.OperatingSystem.Windows, "env(LocalAppData)" },
						{ OsConfig.OperatingSystem.MacOS, "env(HOME)/.config" }
					}
				}
			}
		};
	}

	public static TokenMap ReadConfig(string yamlString)
	{
		using var input = new StringReader(yamlString);
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		return deserializer.Deserialize<TokenMap>(input);
	}

	public string ToYaml()
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();
		return serializer.Serialize(this);
	}

	public Dictionary<string, string> ForOs(OsConfig.OperatingSystem os)
	{
		return Tokens.ToDictionary(token => token.Name, token => token.Values.GetValueOrDefault(os) ?? "");
	}
}
