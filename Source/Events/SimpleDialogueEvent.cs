using Godot;
public partial class SimpleDialogueEvent : Event {
    [Export] public string rightArt = "";
    [Export] DialogueLine[] dialogueLines;
    public override async void Execute() {
        Game.Player.CancelAction(0,true);
        await ToSignal(GetTree(), "process_frame");
		Game.State.OnHold = true;
		//
		Game.Dialogue.Open();
		await Wait(0.5f);
        Game.Dialogue.SetLeftArt(Game.Dialogue.GetArt("ari"));
		Game.Dialogue.SetRightArt(Game.Dialogue.GetArt(rightArt));
        ///
        foreach (var l in dialogueLines) {
            await Game.Dialogue.SpawnLine(l.Text, l.Speaker);
        }
        ///
        Game.Dialogue.Close();
		//
		await Wait(0.5f);
		Game.State.OnHold = false;
    }
}