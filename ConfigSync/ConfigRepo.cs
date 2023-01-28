namespace ConfigSync;

public class ConfigRepo
{
	private DirectoryInfo _baseDir;
	private readonly Config _config;
	private readonly OsConfig _osConfig;

	public ConfigRepo(string baseDir, string configFile = "software.yaml", string tokenFile = "tokens.yaml")
	{
		_baseDir = new DirectoryInfo(baseDir);
		var configFileInfo = new FileInfo(configFile);
		var tokenFileInfo = new FileInfo(tokenFile);
		var tokenMap = TokenMap.ReadConfig(tokenFileInfo.OpenText().ReadToEnd());
		_config = Config.ReadConfig(configFileInfo.OpenText().ReadToEnd(), tokenMap);
		_osConfig = new OsConfig(tokenMap);
	}

	public void Backup(string appName)
	{
		/*
		val appConfig = getConfig().configs[appName] ?: error("Invalid application name")
		val configRoot = File(appConfig.configLocation)
		val osConfig = OsConfig()

		configRoot.walkTopDown().forEach { file ->
			val relativeFile = file.relativeTo(configRoot)
			val destFile = Path.of(directory, "configs", appName, relativeFile.name).toFile()
			val contents = file.readText()
			val collapsed = osConfig.collapseVariables(contents)
			destFile.writeText(collapsed)
		}
		 */
		var appConfig = _config.Configs[appName];
		var configRoot = new DirectoryInfo(appConfig.ConfigLocation);
		TraverseTree(configRoot, appConfig.Ignore, appConfig.NoProcess,
			(relativePath, file) =>
			{
				var collapsedText = _osConfig.CollapseVariables(file.OpenText().ReadToEnd());

				// TODO: Write collapsed text to file in backup location (create dirs, if necessary)
			},
			(relativePath, file) =>
			{
				file.CopyTo(Path.Combine(_config.ConfigBackupDir, appName, relativePath));
			});
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
		/*
		val appConfig = getConfig().configs[appName] ?: error("Invalid application name")
		val configRoot = Path.of(directory, "configs", appName).toFile()
		val osConfig = OsConfig()

		configRoot.walkTopDown().forEach { file ->
			val relativeFile = file.relativeTo(configRoot)
			val destFile = Path.of(appConfig.configLocation, relativeFile.name).toFile()
			val contents = file.readText()
			val expanded = osConfig.expandVariables(contents)
			destFile.writeText(expanded)
		}
		 */
	}

	public void Restore()
	{
		foreach (var (appName, _) in _config.Configs)
		{
			Restore(appName);
		}
	}

	private static void TraverseTree(DirectoryInfo baseDir, List<string> ignoreList, List<string> noProcessList,
		Action<string, FileInfo> process, Action<string, FileInfo> backupOnly)
	{
		var dirs = new Stack<DirectoryInfo>();
		dirs.Push(baseDir);

		while (dirs.Count > 0)
		{
			var currentDir = dirs.Pop();
			var subDirs = currentDir.GetDirectories();
			var files = currentDir.GetFiles();

			foreach (var file in files)
			{
				var relativeName = file.FullName[baseDir.FullName.Length..].Replace("\\", "/");

				if (!ignoreList.Contains(relativeName))
				{
					if (!noProcessList.Contains(relativeName))
					{
						// Process and backup
						process(relativeName, file);
					}
					else
					{
						// Just backup
						backupOnly(relativeName, file);
					}
				}
			}

			foreach (var dir in subDirs)
			{
				dirs.Push(dir);
			}
		}
	}
}
