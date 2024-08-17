using Godot;

public partial class TitleMenu : Control {
    [Export] Control continueButton;
    [Export] Control startSelection;
    public override void _Ready()
    {
        continueButton.Visible = Game.State.SaveExists();
        if (continueButton.Visible) startSelection = continueButton;
        startSelection.GrabFocus();
    }
    public override void _Process(double delta)
    {
        if (IsVisibleInTree()) {
            var focused = GetViewport().GuiGetFocusOwner();
            if (focused == null) startSelection.GrabFocus();
        }
    }
    public void Continue() {
        if (Game.State.SaveExists()) Game.State.LoadGame();
        Game.Instance.StartGame();
    }
    public void NewGame() {
        Game.Instance.StartGame();
    }
    public void OpenConfig() {
        // TODO.
    }
    public void ExitGame() {
        GetTree().Quit();
    }
}