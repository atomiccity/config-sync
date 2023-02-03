namespace ConfigSync;

public class ConfigRepo
{
	private DirectoryInfo _baseDir;
	private readonly Config _config;
	private readonly OsConfig _osConfig;

	public ConfigRepo(string baseDir = ".", string configFile = "software.yaml", string tokenFile = "tokens.yaml")
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
		var appConfig = _config.Configs[appName];
		var configRoot = new DirectoryInfo(appConfig.ConfigLocation);
		var backupDir = Path.Combine(Directory.GetCurrentDirectory(), _config.ConfigBackupDir, appName);
		TraverseTree(configRoot, appConfig.Ignore, appConfig.NoProcess,
			(relativePath, file) =>
			{
				var collapsedText = _osConfig.CollapseVariables(file.OpenText().ReadToEnd());
				File.WriteAllText(Path.Combine(backupDir, relativePath), collapsedText);
			},
			(relativePath, file) =>
			{
				var destFile = new FileInfo(Path.Combine(backupDir, relativePath));
				Directory.CreateDirectory(Path.GetDirectoryName(destFile.FullName));
				file.CopyTo(destFile.FullName);
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
		var appConfig = _config.Configs[appName];
		var configRoot = new DirectoryInfo(appConfig.ConfigLocation);
		var srcDir = Path.Combine(Directory.GetCurrentDirectory(), _config.ConfigBackupDir, appName);
		try
		{
			TraverseTree(new DirectoryInfo(srcDir), new List<string>(), new List<string>(),
				(relativePath, file) =>
				{
					var expandedText = _osConfig.ExpandTokens(file.OpenText().ReadToEnd());
					File.WriteAllText(Path.Combine(configRoot.FullName, relativePath), expandedText);
				},
				(relativePath, file) =>
				{
					file.CopyTo(Path.Combine(configRoot.FullName, relativePath));
				});
		}
		catch (ArgumentException e)
		{
			Console.WriteLine(e);
		}
	}

	public void Restore()
	{
		foreach (var (appName, _) in _config.Configs)
		{
			Restore(appName);
		}
	}

	private static void TraverseTree(DirectoryInfo baseDir, ICollection<string> ignoreList,
		ICollection<string> noProcessList, Action<string, FileInfo> process, Action<string, FileInfo> copy)
	{
		var dirs = new Stack<DirectoryInfo>();

		if (!baseDir.Exists)
		{
			throw new ArgumentException($"Directory {baseDir.FullName} doesn't exist");
		}
		dirs.Push(baseDir);

		while (dirs.Count > 0)
		{
			var currentDir = dirs.Pop();
			var subDirs = currentDir.GetDirectories();
			var files = currentDir.GetFiles();

			foreach (var file in files)
			{
				var relativeName = file.FullName[baseDir.FullName.Length..].Replace("\\", "/");

				if (relativeName.StartsWith("/"))
				{
					relativeName = relativeName[1..];
				}

				if (!ignoreList.Contains(relativeName))
				{
					if (!noProcessList.Contains(relativeName))
					{
						// Process and copy
						process(relativeName, file);
					}
					else
					{
						// Just copy
						copy(relativeName, file);
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
