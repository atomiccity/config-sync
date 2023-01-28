namespace ConfigSync;

public class ConfigRepo
{
	private DirectoryInfo _baseDir;
	private FileInfo _configFile;
	private FileInfo _tokenFile;
	private readonly Config _config;

	public ConfigRepo(string baseDir, string configFile = "software.yaml", string tokenFile = "tokens.yaml")
	{
		_baseDir = new DirectoryInfo(baseDir);
		_configFile = new FileInfo(configFile);
		_tokenFile = new FileInfo(tokenFile);
		var tokenMap = TokenMap.ReadConfig(_tokenFile.OpenText().ReadToEnd());
		_config = Config.ReadConfig(_configFile.OpenText().ReadToEnd(), tokenMap);
	}

	public void Backup(string appName)
	{

	}

	public void Backup()
	{
		foreach (var (appName, _) in _config.Configs)
		{
			Backup(appName);
		}
	}

	public void Restore(string appName)
	{

	}

	public void Restore()
	{
		foreach (var (appName, _) in _config.Configs)
		{
			Restore(appName);
		}
	}
}
