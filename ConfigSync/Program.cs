using System.CommandLine;
using ConfigSync;

class Program
{
	private void InitRepo(string baseDir)
	{
		const string defaultSoftwareConfig = "software.yaml";
		const string defaultTokenConfig = "tokens.yaml";

		// Create baseDir
		Directory.CreateDirectory(baseDir);

		// Write tokenConfig
		File.WriteAllText(Path.Combine(baseDir, defaultTokenConfig), TokenMap.Default().ToYaml());

		// Write softwareConfig
		var defaultConfig = Config.Example();
		Directory.CreateDirectory(Path.Combine(baseDir, defaultConfig.ConfigBackupDir));
		File.WriteAllText(Path.Combine(baseDir, defaultSoftwareConfig), defaultConfig.ToYaml());
	}

	private void Backup(string[] apps)
	{
		var configRepo = new ConfigRepo();

		if (apps.Length > 0)
		{
			foreach (var app in apps)
			{
				configRepo.Backup(app);
			}
		}
		else
		{
			configRepo.Backup();
		}
	}

	private void Restore(string[] apps)
	{
		var configRepo = new ConfigRepo();

		if (apps.Length > 0)
		{
			foreach (var app in apps)
			{
				configRepo.Restore(app);
			}
		}
		else
		{
			configRepo.Restore();
		}
	}

	private void Run(string[] args)
	{
		var rootCommand = new RootCommand("Utility to create cross-platform config file backups");

		var initCommand = new Command("init", "Initialize a configuration repository");
		initCommand.SetHandler(() =>
		{
			InitRepo(".");
		});

		var appArguments = new Argument<string[]>(
			name: "app",
			description: "Name of application(s) to operate on"
		)
		{
			Arity = ArgumentArity.ZeroOrMore
		};

		var backupCommand = new Command("backup", "Backup one or more application configs");
		backupCommand.AddArgument(appArguments);
		backupCommand.SetHandler(Backup, appArguments);

		var restoreCommand = new Command("restore", "Restore one or more application configs");
		restoreCommand.AddArgument(appArguments);
		restoreCommand.SetHandler(Restore, appArguments);

		rootCommand.AddCommand(initCommand);
		rootCommand.AddCommand(backupCommand);
		rootCommand.AddCommand(restoreCommand);
		rootCommand.Invoke(args);
	}

	public static void Main(string[] args)
	{
		var program = new Program();
		program.Run(args);
	}
}
