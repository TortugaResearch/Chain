﻿namespace Tortuga.Shipwright;

/// <summary>
/// When exposing a method, event, or property, this enumeration indicates what accessibility to use. 
/// </summary>
[Flags]
public enum Accessibility
{
	Public = 0,
	Protected = 1,
	Internal = 2,
	ProtectedOrInternal = 3,
	Private = 4,
}
