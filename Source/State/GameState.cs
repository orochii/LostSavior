using System;
using System.Collections.Generic;
using Godot;

public class GameState {
    const int BASE_EXP = 20;
    const int RAISE_EXP = 10;
    const int MAX_LEVEL = 99;
    const int MAX_EQUIP_SLOTS = 4;

    // Progression
    int Level = 1;
    int Experience = 0;
    int Money = 0;
    StringName[] EquipmentIds = new StringName[MAX_EQUIP_SLOTS];
    List<StringName> Inventory = new List<StringName>();
    // Stats
    int CurrHealth = 1;
    int MaxHealthPlus = 0;
    int StrengthPlus = 0;
    int ConstitutionPlus = 0;
    int IntelligencePlus = 0;
    // Cache
    int[] ExpList = null;
    //
    public GameState() {
        MakeExpList();
        EquipmentIds[0] = "Equip/Pistol";
        Inventory.Add("Potion");
        Inventory.Add("Potion");
        Inventory.Add("Equip/Knife");
        Inventory.Add("Potion");
        Inventory.Add("Potion");
        Inventory.Add("Potion");
        Inventory.Add("Equip/Knife");
        Inventory.Add("Potion");
        Inventory.Add("Potion");
        Inventory.Add("Potion");
        Inventory.Add("Equip/Knife");
        Inventory.Add("Potion");
        CurrHealth = GetMaxHealth();
    }
    // Statistics
    public int GetHealth() {
        return CurrHealth;
    }
    public void ChangeHealth(int v) {
        CurrHealth = Math.Clamp(CurrHealth + v, 0, GetMaxHealth());
    }
    public string GetHealthString() {
        return string.Format("{0:D4}/{1:D4}", CurrHealth, GetMaxHealth());
    }
    private int GetBaseMaxHealth() {
        return 100 + (Level * 20);
    }
    private int GetBaseStr() {
        return 5 + (Level);
    }
    private int GetBaseCon() {
        return 5 + (Level);
    }
    private int GetBaseInt() {
        return 5 + (Level);
    }
    public int GetMaxHealth() {
        int maxHealth = GetBaseMaxHealth() + MaxHealthPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) maxHealth += e.HealthPlus;
        }
        return maxHealth;
    }
    public int GetStr() {
        int v = GetBaseStr() + StrengthPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.StrPlus;
        }
        return v;
    }
    public int GetCon() {
        int v = GetBaseCon() + ConstitutionPlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.ConPlus;
        }
        return v;
    }
    public int GetInt() {
        int v = GetBaseInt() + IntelligencePlus;
        var equips = GetAllEquipment();
        foreach (var e in equips) {
            if (e != null) v += e.IntPlus;
        }
        return v;
    }
    // Equipment
    public EquipItem GetWeapon() {
        var id = EquipmentIds[0];
        if (id==null || id.IsEmpty) return null;
        return OZResourceLoader.Load<EquipItem>(InventoryItem.GetFullPath(id));
    }
    public EquipItem[] GetAllEquipment() {
        EquipItem[] retVal = new EquipItem[MAX_EQUIP_SLOTS];
        for (int i = 0; i < MAX_EQUIP_SLOTS; i++) {
            var id = EquipmentIds[i];
            //
            if (id != null && !id.IsEmpty) {
                var e = OZResourceLoader.Load<EquipItem>(InventoryItem.GetFullPath(id));
                retVal[i] = e;
            }
        }
        return retVal;
    }
    public InventoryItem[] GetAllInventory() {
        InventoryItem[] items = new InventoryItem[Inventory.Count];
        for (int i = 0; i < items.Length; i++) {
            var id = Inventory[i];
            if (id != null && !id.IsEmpty) {
                var e = OZResourceLoader.Load<InventoryItem>(InventoryItem.GetFullPath(id));
                items[i] = e;
            }
        }
        return items;
    }
    // Experience stuff
    void MakeExpList() {
        ExpList = new int[MAX_LEVEL];
        int lastExp = 0;
        for (int level = 0; level < MAX_LEVEL; level++) {
            // Calculate new level's experience.
            int levelExp = BASE_EXP + (level * RAISE_EXP);
            // Set up curve.
            ExpList[level] = lastExp + levelExp;
            lastExp = ExpList[level];
        }
    }
    int GetNextLevelExp(int l) {
        if (ExpList == null) MakeExpList();
        if (l < 0 || l >= ExpList.Length) return 0;
        return ExpList[l-1];
    }
}