// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.FeasibilityTests;

Console.WriteLine("Starting Reverse Coverage Feasibility Test...");
Console.WriteLine();

var test = new FeasibilityTest();
var result = await test.RunAsync();

Environment.Exit(result.Success ? 0 : 1);
