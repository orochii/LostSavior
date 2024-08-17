using Godot;
using System;

public partial class TutorialPopup : Event
{
	[Export] string flagId = "TUTORIAL_1";
	[Export] string message = "Press {0} to jump.";
	[Export] string actionName = "jump";
	[Export] float duration = 2f;
  public override void Execute()
  {
  if (Game.State.GetProgressFlag(flagId) > 0) return;
    var _tlFormat = TranslationServer.Translate(message);
    var _l = UIUtils.GetKeyIcon(actionName);
    var _text = string.Format(_tlFormat, _l);
    Game.PopUp.Create(_text, duration);
    Game.State.SetProgressFlag(flagId, 1);
  }
}
