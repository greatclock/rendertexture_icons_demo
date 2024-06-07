using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RockPaperScissors {
	Rock, Paper, Scissors
}

public enum EquipmentType {
	Clothes, Shoes, Weapon, Shield
}

public abstract class IconBaseData {
	public string icon;
	public int level;
	public float saturation_all = 1f;
	public float saturation_icon = 1f;
}

public class HeroData : IconBaseData {
	public string name;
	public RockPaperScissors type;
}

public class ItemData : IconBaseData {
	public int count;
}

public class EquipData : IconBaseData {
	public EquipmentType type;
	public int stars;
}