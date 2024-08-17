using System;
using Godot;
using Newtonsoft.Json;

[Serializable]
public class ConfigData {
    public int ScreenWidth = 480;
    public int ScreenHeight = 270;
    public int ScreenScale = 2;
    public bool ScreenMode = false;
    public float VolumeMaster = 0;
    public float VolumeBGM = 0;
    public float VolumeSFX = 0;
    public float VolumeAMB = 0;
    public string Language = "en";
}

public class Config {
    private static Config instance;
    ConfigData data;
    public static void Init() {
        instance = new Config();
    }
    private Config() {
        if (FileExists()) Load();
        else data = new ConfigData();
        //
        RefreshVolume();
        RefreshScale();
    }
    private void RefreshVolume() {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"),data.VolumeMaster);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BGM"),data.VolumeBGM);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"),data.VolumeSFX);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("AMB"),data.VolumeAMB);
    }
    private void RefreshScale() {
        Game.Instance.Resize(data.ScreenWidth,data.ScreenHeight,data.ScreenScale);
    }
    public string Filename { 
        get {
            return "user://config.dat";
        }
    }
    public bool FileExists() {
        return FileAccess.FileExists(Filename);
    }
    public void Save() {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Write);
        string json = JsonConvert.SerializeObject(data);
        saveFile.StoreString(json);
        saveFile.Close();
    }
    public void Load() {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Read);
        string json = saveFile.GetAsText();
        data = JsonConvert.DeserializeObject<ConfigData>(json);
        saveFile.Close();
    }
}