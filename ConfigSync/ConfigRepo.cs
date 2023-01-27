namespace ConfigSync;

public class ConfigRepo
{
	private DirectoryInfo _baseDir;
	private FileInfo _configFile;
	private readonly Config _config;

	public ConfigRepo(string baseDir, string configFile = "software.yaml")
	{
		_baseDir = new DirectoryInfo(baseDir);
		_configFile = new FileInfo(configFile);
		_config = Config.ReadConfig(_configFile.OpenText().ReadToEnd());
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
