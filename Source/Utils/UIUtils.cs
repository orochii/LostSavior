using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

public static class UIUtils {
    public static void SetupHBoxList(List<Control> array) {
        for (int i = 0; i < array.Count; i++) {
            var curr = array[i];
            curr.FocusNeighborLeft = curr.GetPath();
            curr.FocusNeighborRight = curr.GetPath();
            if (i > 0) curr.FocusNeighborTop = array[i-1].GetPath();
            else curr.FocusNeighborTop = curr.GetPath();
            if (i < array.Count-1) curr.FocusNeighborBottom = array[i+1].GetPath();
            else curr.FocusNeighborBottom = curr.GetPath();
        }
    }
    public static string GetKeyIcon(string actionName, int kind = -1) {
        if (kind < 0) kind = Game.Instance.LastInput;
        string result = "";
        var ary = InputMap.ActionGetEvents(actionName);
        foreach (var ev in ary) {
            if (kind == 0) {
                if (ev is InputEventKey) {
                    var key = ev as InputEventKey;
                    //key.Keycode
                    var k = key.PhysicalKeycode;
                    if (k == Key.None) k = key.Keycode;
                    var idx = Game.Instance.GetKeyboardMappingIndex(k);
                    var x = (idx % 16) * 16;
                    var y = (idx / 16) * 16;
                    result += string.Format("[img region={0},{1},16,16]res://Graphics/System/keyboard_icons.png[/img]",x,y);
                }
            }
            if (kind == 1) {
                if (ev is InputEventJoypadButton) {
                    var button = ev as InputEventJoypadButton;
                    //button.ButtonIndex
                    var idx = Game.Instance.GetJoyButtonMappingIndex(button.ButtonIndex);
                    var x = (idx % 16) * 16;
                    var y = (idx / 16) * 16;
                    result += string.Format("[img region={0},{1},16,16]res://Graphics/System/button_icons.png[/img]",x,y);
                }
                if (ev is InputEventJoypadMotion) {
                    var motion = ev as InputEventJoypadMotion;
                    //motion.Axis
                    var idx = Game.Instance.GetJoyAxisMappingIndex(motion.Axis,motion.AxisValue);
                    var x = (idx % 8) * 16;
                    var y = (idx / 8) * 16;
                    result += string.Format("[img region={0},{1},16,16]res://Graphics/System/axis_icons.png[/img]",x,y);
                }
            }
        }
        return result;
    }
}