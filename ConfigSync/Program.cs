using ConfigSync;

// Commands:
//     init - create default tokens.yaml and skeleton software.yaml plus dir structure
//     restore - restore one or all configs
//     backup - backup one or all configs

Console.WriteLine(Config.Example().ToYaml());
Console.WriteLine("=======================");
Console.WriteLine(TokenMap.Default().ToYaml());
