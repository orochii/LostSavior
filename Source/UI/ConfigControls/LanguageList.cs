using Godot;
using System;

public partial class LanguageList : OptionButton
{
	public override void _Ready()
	{
		var all = TranslationServer.GetLoadedLocales();
		Clear();
		foreach (var loc in all) {
			AddItem(loc);
		}
	}
}
