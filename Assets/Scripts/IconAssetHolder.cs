using System;
using UnityEngine;
using UnityEngine.UI;

public class IconAssetHolder : MonoBehaviour {

	[Header("For Rectangle Icons")]
	[SerializeField]
	private Image[] m_RectBackgrounds;
	[SerializeField]
	private Image[] m_RectFrames;

	[Header("For Circle Icons")]
	[SerializeField]
	private Image[] m_CircleBackgrounds;
	[SerializeField]
	private Image[] m_CircleFrames;

	[Header("Rock Paper Scissors")]
	[SerializeField]
	private Image m_Rock;
	[SerializeField]
	private Image m_Paper;
	[SerializeField]
	private Image m_Scissors;

	[Header("Equipments Tag")]
	[SerializeField]
	private Image m_EquipClothesTag;
	[SerializeField]
	private Image m_EquipShoesTag;
	[SerializeField]
	private Image m_EquipWeaponTag;
	[SerializeField]
	private Image m_EquipShieldTag;

	[Header("Others")]
	[SerializeField]
	private Image m_Star;
	[SerializeField]
	private Image m_CircleMask;
	[SerializeField]
	private Image m_DefaultHeroIcon;
	[SerializeField]
	private Image m_DefaultItemIcon;
	[SerializeField]
	private Image m_DefaultEquipIcon;
	[SerializeField]
	private Font m_Font;
	[SerializeField]
	private Gradient[] m_ColorsForLevel;

	public bool GetRectSprites(int level, out Sprite background, out Sprite frame) {
		background = null;
		frame = null;
		if (level < 0 || level >= m_RectBackgrounds.Length || level >= m_RectFrames.Length) {
			return false;
		}
		Image bg = m_RectBackgrounds[level];
		Image fr = m_RectFrames[level];
		if (bg != null) { background = bg.sprite; }
		if (fr != null) { frame = fr.sprite; }
		return true;
	}

	public bool GetCircleSprites(int level, out Sprite background, out Sprite frame) {
		background = null;
		frame = null;
		if (level < 0 || level >= m_CircleBackgrounds.Length || level >= m_CircleFrames.Length) {
			return false;
		}
		Image bg = m_CircleBackgrounds[level];
		Image fr = m_CircleFrames[level];
		if (bg != null) { background = bg.sprite; }
		if (fr != null) { frame = fr.sprite; }
		return true;
	}

	public Sprite GetRockPaperScissors(RockPaperScissors v) {
		switch (v) {
			case RockPaperScissors.Rock:
				return m_Rock == null ? null : m_Rock.sprite;
			case RockPaperScissors.Paper:
				return m_Paper == null ? null : m_Paper.sprite;
			case RockPaperScissors.Scissors:
				return m_Scissors == null ? null : m_Scissors.sprite;
		}
		return null;
	}

	public Sprite GetEquipmentTag(EquipmentType type) {
		switch (type) {
			case EquipmentType.Clothes:
				return m_EquipClothesTag == null ? null : m_EquipClothesTag.sprite;
			case EquipmentType.Shoes:
				return m_EquipShoesTag == null ? null : m_EquipShoesTag.sprite;
			case EquipmentType.Weapon:
				return m_EquipWeaponTag == null ? null : m_EquipWeaponTag.sprite;
			case EquipmentType.Shield:
				return m_EquipShieldTag == null ? null : m_EquipShieldTag.sprite;
		}
		return null;
	}

	public Sprite Star { get { return m_Star == null ? null : m_Star.sprite; } }

	public Sprite CircleMask { get { return m_CircleMask == null ? null : m_CircleMask.sprite; } }

	public Gradient GetColorForLevel(int level) {
		if (level < 0 || level >= m_ColorsForLevel.Length) { return null; }
		return m_ColorsForLevel[level];
	}

	public Font TextFont { get { return m_Font; } }

	public Sprite DefaultHeroIcon { get { return m_DefaultHeroIcon == null ? null : m_DefaultHeroIcon.sprite; } }

	public Sprite DefaultItemIcon { get { return m_DefaultItemIcon == null ? null : m_DefaultItemIcon.sprite; } }

	public Sprite DefaultEquipIcon { get { return m_DefaultEquipIcon == null ? null : m_DefaultEquipIcon.sprite; } }

	public static IconAssetHolder instance { get; private set; }

	void Awake() {
		if (instance == null) {
			instance = this;
		} else {
			throw new Exception("'IconAssetHolder' Cannot be instantiated more than once !");
		}
		gameObject.SetActive(false);
	}

}
