using System.Collections.Generic;
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
}