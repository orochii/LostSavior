using Godot;
using System;

public partial class World : Node2D
{
	public string LocationName {
		get {
			if (locationId == null) return defaultLocationName;
			return locationId;
		}
	}
	[Export] Node PlayerStart;
	[Export] string defaultLocationName;
	[Export] AudioEntry defaultMusic;
	[Export] Node2D LocationsNode;
	private string locationId = null;
	private WorldLocation currentLocation;
	private WorldLocation lastLocation;
    public override void _Ready()
    {
        base._Ready();
		LocationsNode.Visible = true;
    }
    public bool EnterLocation(WorldLocation location) {
		if (location.id != locationId) {
			bool retVal = false;
			locationId = location.id;
			if (lastLocation != location) {
				if (lastLocation != null) lastLocation.Cleanup();
				retVal = true;
			}
			lastLocation = currentLocation;
			currentLocation = location;
			if (!Game.State.MuteLocation) AudioManager.PlayMusic(location.music);
			Game.LocationName.ShowName(locationId);
			return retVal;
		}
		return false;
	}
	public bool ExitLocation(WorldLocation location) {
		if (location.id == locationId) {
			bool retVal = false;
			locationId = null;
			if (lastLocation != null && lastLocation != location) {
				lastLocation.Cleanup();
				retVal = true;
			}
			lastLocation = currentLocation;
			currentLocation = null;
			if (!Game.State.MuteLocation) AudioManager.PlayMusic(defaultMusic);
			Game.LocationName.ShowName(defaultLocationName);
			return retVal;
		}
		return false;
	}
	public void ReplayLocationMusic() {
		Game.State.MuteLocation = false;
		if (currentLocation == null) AudioManager.PlayMusic(defaultMusic);
		else AudioManager.PlayMusic(currentLocation.music);
	}
	public void InitializePlayer() {
		if(PlayerStart != null) {
			foreach (var c in PlayerStart.GetChildren()) {
				if (c is Node2D) {
					var start = c as Node2D;
					var checkpoint = Game.State.LastCheckpoint;
					if (checkpoint == "" || c.Name == checkpoint) {
						Game.Player.Position = start.Position;
						OnStart(start);
						break;
					}
				}
			}
		}
	}
	public void OnStart(Node2D start) {
		AudioManager.PlayMusic(defaultMusic);
		if (start is Event) {
			var ev = start as Event;
			ev.Execute();
		}
	}
}
