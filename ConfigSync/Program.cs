using ConfigSync;

const string Document = @"
configs:
  alacritty:
    configLocation: configDir/alacritty
  helix-editor:
    configLocation: configDir/helix
";

var config = Config.ReadConfig(Document);

System.Console.WriteLine(config.Configs.First().Value.ConfigLocation);
