// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.FeasibilityTests;

if (args.Length == 0)
{
    Console.WriteLine("Usage: MultiProjectFeasibilityTest <config-file>");
    Console.WriteLine("Example: MultiProjectFeasibilityTest multi-project-config.json");
    Environment.Exit(1);
}

var configFile = args[0];
if (!File.Exists(configFile))
{
    Console.WriteLine($"Error: Config file not found: {configFile}");
    Environment.Exit(1);
}

var test = new MultiProjectFeasibilityTest(configFile);
var result = await test.RunAsync();

Environment.Exit(result.Success ? 0 : 1);
