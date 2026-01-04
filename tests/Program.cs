// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.FeasibilityTests;

// Check if multi-project mode
if (args.Length > 0 && File.Exists(args[0]))
{
    Console.WriteLine("Running Multi-Project Feasibility Test...");
    Console.WriteLine();
    
    var multiTest = new MultiProjectFeasibilityTest(args[0]);
    var multiResult = await multiTest.RunAsync();
    Environment.Exit(multiResult.Success ? 0 : 1);
}
else
{
    Console.WriteLine("Running Single-Project Feasibility Test...");
    Console.WriteLine();
    
    var test = new FeasibilityTest();
    var result = await test.RunAsync();
    Environment.Exit(result.Success ? 0 : 1);
}
