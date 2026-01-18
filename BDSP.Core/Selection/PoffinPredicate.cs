using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

/// <summary>
/// Lightweight, allocation-free filter for poffins.
/// </summary>
public delegate bool PoffinPredicate(in Poffin poffin);
