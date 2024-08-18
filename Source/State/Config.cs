using System;
using Godot;
using Newtonsoft.Json;

[Serializable]
public class ConfigData {
    public int ScreenSize { get; set; } = 0;
    public int ScreenScale { get; set; } = 1;
    public bool ScreenMode { get; set; } = false;
    public float VolumeMaster { get; set; } = 0;
    public float VolumeBGM { get; set; } = 0;
    public float VolumeSFX { get; set; } = 0;
    public float VolumeAMB { get; set; } = 0;
    public string Language { get; set; } = "en";
}

public class Config {
    public static Vector2I[] SCREEN_SIZES = new Vector2I[] {
        new Vector2I(480,270),
        new Vector2I(320,180),
    };
    private static Config instance;
    ConfigData data;
    public static ConfigData Data {
        get {
            if (instance==null) return null;
            return instance.data;
        }
    }
    public static void Init() {
        instance = new Config();
    }
    public static void Refresh() {
        if (instance == null) return;
        instance.RefreshVolume();
        instance.RefreshScale();
        instance.RefreshLanguage();
    }
    public static void Save() {
        if (instance == null) return;
        instance.SaveInternal();
    }
    private Config() {
        if (FileExists()) LoadInternal();
        else data = new ConfigData();
        //
        RefreshVolume();
        RefreshScale();
        RefreshLanguage();
    }
    private void RefreshVolume() {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"),data.VolumeMaster);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BGM"),data.VolumeBGM);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"),data.VolumeSFX);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("AMB"),data.VolumeAMB);
    }
    private void RefreshScale() {
        var size = SCREEN_SIZES[data.ScreenSize];
        Game.Instance.Resize(size.X,size.Y,data.ScreenScale+1);
        if (data.ScreenMode) DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }
    private void RefreshLanguage() {
        TranslationServer.SetLocale(data.Language);
    }
    public string Filename { 
        get {
            return "user://config.dat";
        }
    }
    public bool FileExists() {
        return FileAccess.FileExists(Filename);
    }
    private void SaveInternal() {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Write);
        string json = JsonConvert.SerializeObject(data);
        saveFile.StoreString(json);
        saveFile.Close();
    }
    private void LoadInternal() {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Read);
        string json = saveFile.GetAsText();
        data = JsonConvert.DeserializeObject<ConfigData>(json);
        saveFile.Close();
    }
}