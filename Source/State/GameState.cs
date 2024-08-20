using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

public class GameState {
    const int BASE_EXP = 20;
    const int RAISE_EXP = 10;
    public const int MAX_LEVEL = 99;
    const int MAX_ITEMS = 99;
    const int MAX_MONEY = 999999;
    const int MAX_EQUIP_SLOTS = 3;
    public const int MAX_CARD_SLOTS = 2;
    [Serializable]
    public class State {
        // Character Build
        public int Level = 1;
        public int Experience = 0;
        public int Money = 0;
        public string[] EquipmentIds = new string[MAX_EQUIP_SLOTS];
        public string AltWeapon = null;
        public string[] EquippedCards = new string[MAX_CARD_SLOTS];
        public Dictionary<string,int> Inventory = new Dictionary<string,int>();
        public List<string> Cards = new List<string>();
        // Stats
        public int CurrHealth = 1;
        public int MaxHealthPlus = 0;
        public int StrengthPlus = 0;
        public int ConstitutionPlus = 0;
        public int IntelligencePlus = 0;
        // Area
        public string lastWorld = "";
        public string lastStart = "";
        public Dictionary<string,int> progressFlags = new Dictionary<string, int>();
    }
    public string LastCheckpoint => state.lastStart;
    public void SetLastCheckpoint(string name) {
        //state.lastWorld = Game.World. // ?
        state.lastStart = name;
    }
    // Cache
    State state;
    public float[] PowerConsumed = new float[GameState.MAX_CARD_SLOTS];
	public bool[] PowerInRecovery = new bool[GameState.MAX_CARD_SLOTS];
    public bool OnHold;
    public bool MuteLocation;
    int[] ExpList = null;
    ActorData data;
    public ActorData Data => data;
    //
    public GameState() {
        state = new State();
        //
        data = OZResourceLoader.Load<ActorData>("res://Data/actor_data.tres");
        MakeExpList();
        // Set starting level.
        state.Level = data.StartingLevel;
        state.Experience = GetNextLevelExp(state.Level-1);
        // Set starting equipment.
        for (int i = 0; i < MAX_EQUIP_SLOTS; i++) {
            if (data.StartingEquipment.Length > i && data.StartingEquipment[i] != null) {
                state.EquipmentIds[i] = data.StartingEquipment[i].GetId();
            } else {
                state.EquipmentIds[i] = null;
            }
        }
        // Set starting inventory.
        foreach (var i in data.StartingItems) {
            ChangeItem(i.GetId(), 1);
        }
        // Gold.
        state.Money = data.StartingMoney;
        // Recover.
        RecoverAll();
    }
    // Save/Load
    public string SaveFilename { 
        get {
            return "user://savegame.save";
        }
    }

    public bool SaveExists() {
        return FileAccess.FileExists(SaveFilename);
    }
    public void SaveGame() {
        var saveFile = FileAccess.Open(SaveFilename, FileAccess.ModeFlags.Write);
        string json = JsonConvert.SerializeObject(state);
        saveFile.StoreString(json);
        saveFile.Close();
        GD.Print("SAVED: ",SaveFilename);
    }
    public void LoadGame() {
        var saveFile = FileAccess.Open(SaveFilename, FileAccess.ModeFlags.Read);
        string json = saveFile.GetAsText();
        state = JsonConvert.DeserializeObject<State>(json);
        saveFile.Close();
    }
    // Progress
    public int GetProgressFlag(string id) {
        if (state.progressFlags.ContainsKey(id)) return state.progressFlags[id];
        return 0;
    }
    public void SetProgressFlag(string id, int v) {
        state.progressFlags[id] = v;
        Game.Instance.EmitSignal(Game.SignalName.OnFlagUpdate, id, v);
    }
    public int GetLevel() {
        return state.Level;
    }
    public int GetMoney() {
        return state.Money;
    }
    public void ChangeExperience(int v) {
        state.Experience += v;
        while (state.Experience >= GetNextLevelExp(state.Level)) {
            if (state.Level >= MAX_LEVEL) break;
            state.Level += 1;
            RecoverAll();
            Game.Player.ShowLevelUp();
        }
    }
    public void ChangeMoney(int v) {
        state.Money = Math.Clamp(state.Money+v, 0, MAX_MONEY);
    }
    // Statistics
    public void RecoverAll() {
        state.CurrHealth = GetMaxHealth();
    }
    public int GetHealth() {
        return state.CurrHealth;
    }
    public void ChangeHealth(int v) {
        state.CurrHealth = Math.Clamp(state.CurrHealth + v, 0, GetMaxHealth());
    }
    public string GetHealthString() {
        return string.Format("{0:D4}/{1:D4}", state.CurrHealth, GetMaxHealth());
    }
    private int GetBaseStat(Vector2 growth) {
        return (int)(growth.X + (state.Level * growth.Y));
    }
    private int GetBaseMaxHealth() {
        return GetBaseStat(data.HealthGrowth);
    }
    private int GetBaseStr() {
        return GetBaseStat(data.StrengthGrowth);
    }
    private int GetBaseCon() {
        return GetBaseStat(data.ConstitutionGrowth);
    }
    private int GetBaseInt() {
        return GetBaseStat(data.IntelligenceGrowth);
    }
    public int GetMaxHealth() {
        int maxHealth = GetBaseMaxHealth() + state.MaxHealthPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) maxHealth += e.HealthPlus;
        }
        return maxHealth;
    }
    public int GetStr() {
        int v = GetBaseStr() + state.StrengthPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.StrPlus;
        }
        return v;
    }
    public int GetCon() {
        int v = GetBaseCon() + state.ConstitutionPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.ConPlus;
        }
        return v;
    }
    public int GetInt() {
        int v = GetBaseInt() + state.IntelligencePlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.IntPlus;
        }
        return v;
    }
    // Equipment
    public string GetAltWeapon() {
        return state.AltWeapon;
    }
    public void SetAltWeapon(string v) {
        state.AltWeapon = v;
    }
    public void SwitchWeapon() {
        if (state.AltWeapon == null) return;
        if (state.AltWeapon.Length == 0) return;
        // Check if you have the weapon to switch to.
        int count = GetItemCount(state.AltWeapon);
        if (count > 0) {
            // Get item references
            var curr = GetWeapon();
            var alt = OZResourceLoader.Load<EquipItem>(InventoryItem.GetFullPath(state.AltWeapon));
            // Alternate
            state.AltWeapon = curr.GetId();
            // Equip
            Equip(alt);
        }
    }
    public EquipItem GetWeapon() {
        var id = state.EquipmentIds[0];
        if (id==null || id.Length==0) return null;
        return OZResourceLoader.Load<EquipItem>(InventoryItem.GetFullPath(id));
    }
    public EquipItem[] GetAllEquipment() {
        EquipItem[] retVal = new EquipItem[MAX_EQUIP_SLOTS];
        for (int i = 0; i < MAX_EQUIP_SLOTS; i++) {
            var id = state.EquipmentIds[i];
            //
            if (id != null && id.Length!=0) {
                var e = OZResourceLoader.Load<EquipItem>(InventoryItem.GetFullPath(id));
                retVal[i] = e;
            }
        }
        return retVal;
    }
    public BaseCard GetEquippedCard(int i) {
        var id = state.EquippedCards[i];
        if (id==null || id.Length==0) return null;
        return OZResourceLoader.Load<BaseCard>(BaseCard.GetFullPath(id));
    }
    public BaseCard[] GetAllEquippedCards() {
        BaseCard[] retVal = new BaseCard[MAX_CARD_SLOTS];
        for (int i = 0; i < MAX_CARD_SLOTS; i++) {
            var id = state.EquippedCards[i];
            if (id != null && id.Length != 0) {
                var c = OZResourceLoader.Load<BaseCard>(BaseCard.GetFullPath(id));
                retVal[i] = c;
            }
        }
        return retVal;
    }
    public InventoryItem[] GetAllInventory() {
        var keys = state.Inventory.Keys.ToArray();
        InventoryItem[] items = new InventoryItem[keys.Length];
        for (int i = 0; i < items.Length; i++) {
            var id = keys[i];
            if (id != null && id.Length!=0) {
                var e = OZResourceLoader.Load<InventoryItem>(InventoryItem.GetFullPath(id));
                items[i] = e;
            }
        }
        Array.Sort(items);
        return items;
    }
    public int GetItemCount(string id) {
        if (state.Inventory.ContainsKey(id)) return state.Inventory[id];
        return 0;
    }
    public void ChangeItem(string id, int ammount) {
        if (state.Inventory.ContainsKey(id)) {
            state.Inventory[id] = Math.Clamp(state.Inventory[id] + ammount, 0, MAX_ITEMS);
            if (state.Inventory[id] < 1) state.Inventory.Remove(id);
        } else {
            if (ammount > 0) {
                state.Inventory.Add(id,ammount);
            }
        }
    }
    public BaseCard[] GetAllCards() {
        var ary = new BaseCard[state.Cards.Count];
        for (int i = 0; i < ary.Length; i++) {
            var id = state.Cards[i];
            if (id != null && id.Length!=0) {
                var c = OZResourceLoader.Load<BaseCard>(BaseCard.GetFullPath(id));
                ary[i] = c;
            }
        }
        return ary;
    }
    public void AddCard(string id) {
        if (!state.Cards.Contains(id)) {
            state.Cards.Add(id);
            state.Cards.Sort();
        }
    }
    public bool HasCard(string id) {
        return state.Cards.Contains(id);
    }
    // Experience stuff
    void MakeExpList() {
        ExpList = new int[MAX_LEVEL];
        int lastExp = 0;
        for (int level = 0; level < MAX_LEVEL; level++) {
            // Calculate new level's experience.
            int levelExp = BASE_EXP + ((level * level * 3 / 2) * RAISE_EXP);
            // Set up curve.
            ExpList[level] = lastExp + levelExp;
            lastExp = ExpList[level];
        }
    }
    int GetNextLevelExp(int l) {
        if (ExpList == null) MakeExpList();
        if (l <= 0) return 0;
        if (l >= ExpList.Length) return ExpList[ExpList.Length-1];
        return ExpList[l-1];
    }
    public int GetTotalLevelExp() {
        return GetNextLevelExp(state.Level) - GetNextLevelExp(state.Level-1);
    }
    public int GetCurrentLevelExp() {
        return state.Experience - GetNextLevelExp(state.Level-1);
    }
    public float GetExpPercent()
    {
        var total = GetTotalLevelExp();
        var curr = GetCurrentLevelExp();
        return curr * 1f / total;
    }
    public void EquipCard(int slot, string id) {
        state.EquippedCards[slot] = id;
    }
    internal void Equip(EquipItem equip)
    {
        var slot = (int)equip.EquipKind - 1;
        var oldId = state.EquipmentIds[slot];
        if (oldId != null) {
            // receive current
            ChangeItem(oldId, 1);
        }
        if (equip != null) {
            // take off item
            ChangeItem(equip.GetId(), -1);
            // change equip slot to this
            state.EquipmentIds[slot] = equip.GetId();
        }
    }
}
