using Godot;
using static Godot.Control;

public partial class Event_StartGame : Event {
    public override async void Execute()
    {
        //
        Game.Player.CancelAction(0);
        await ToSignal(GetTree(), "process_frame");
        Game.Player.SetAction("sitting", 1.0f);
        GD.Print("Starting this thing wow.");
        // Put game on hold.
        Game.State.OnHold = true;
        // Show sitting.
        await ToSignal(GetTree(), "process_frame");
        Game.Player.SetAction("sitting", 1.0f);
		// Wait.
        await Wait(1f);
		// Get up.
        Game.Player.SetAction("sitting_get_up", 0);
        // Wait.
        await Wait(3.5f);
        //
        //
        Game.Dialogue.Open();
        await ToSignal(GetTree(), "process_frame");
        await Wait(0.5f);
        Game.Dialogue.SetLeftArt(Game.Dialogue.GetArt("ari"));
		await Game.Dialogue.SpawnLine("My head... it hurts.", ESpeaker.LEFT);
        await Game.Dialogue.SpawnLine("Where am I? What's... my name?", ESpeaker.LEFT);
        await Game.Dialogue.SpawnLine("[i]Ari.[/i]", ESpeaker.MIDDLE);
        await Game.Dialogue.SpawnLine("?", ESpeaker.LEFT);
        await Game.Dialogue.SpawnLine("Hello? Is someone there?", ESpeaker.LEFT);
        Game.Dialogue.FlushLines();
        await Wait(0.5f);
        await Game.Dialogue.SpawnLine("...did I imagine that? I better \ntry and find a way out of here.", ESpeaker.LEFT);
        Game.Dialogue.Close();
        //
        //
        // Free player.
        await Wait(0.5f);
        Game.State.OnHold = false;
        //
        GD.Print("The thing ended.");
        //
        var _tlFormat = TranslationServer.Translate("Press {0} and {1} to move.");
        var _l = UIUtils.GetKeyIcon("move_left");
        var _r = UIUtils.GetKeyIcon("move_right");
        var _text = string.Format(_tlFormat, _l,_r);
        Game.PopUp.Create(_text, 2f);
    }
}
