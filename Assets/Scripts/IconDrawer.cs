#define UNABLE_RENDER_TEXT

using GreatClock.Common.IconAtlas;
#if UNABLE_RENDER_TEXT
using GreatClock.Common.UI;
#endif
using System;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

/// <summary>
/// A MonoBehaviour that dealing with icon or block data and its visible contents.
/// </summary>
public class IconDrawer : MonoBehaviour {

	[SerializeField]
	private RawImage m_Image;

	private const int ATLAS_COLUMNS = 14;
	private const int ATLAS_ROWS = 14;
	private const int ATLAS_PADDING = 2;

	private const int ICON_WIDTH = 124;
	private const int ICON_HEIGHT = 124;

	private const int ICON_INNER_PADDING = 4;

	private static RenderTextureIcons s_icon_atlas;

	protected static RenderTextureIcons icon_atlas {
		get {
			if (s_icon_atlas == null) {
				int width = (ICON_WIDTH + ATLAS_PADDING) * ATLAS_COLUMNS + ATLAS_PADDING;
				int height = (ICON_HEIGHT + ATLAS_PADDING) * ATLAS_ROWS + ATLAS_PADDING;
				s_icon_atlas = new RenderTextureIcons(width, height, RenderTextureFormat.ARGB32, ICON_WIDTH, ICON_HEIGHT, ATLAS_PADDING);
			}
			return s_icon_atlas;
		}
	}

	public void DrawCircleHero(HeroData data) {
		mCurrentData = data;
		mUseCircle = true;
		if (TryInit()) {
			DrawHeroImpl(data);
		}
	}

	public void DrawRectHero(HeroData data) {
		mCurrentData = data;
		mUseCircle = false;
		if (TryInit()) {
			DrawHeroImpl(data);
		}
	}

	public void DrawBagItem(ItemData data) {
		mCurrentData = data;
		mUseCircle = false;
		if (TryInit()) {
			DrawBagItemImpl(data);
		}
	}

	public void DrawEquipment(EquipData data) {
		mCurrentData = data;
		mUseCircle = false;
		if (TryInit()) {
			DrawEquipmentImpl(data);
		}
	}

	private Action mOnRedraw;
	private FakeLoader.OnLoadedDelegate mOnLoaded;

	private object mKey;
	private object mCurrentData;
	private bool mUseCircle;

	void Awake() {
		mOnRedraw = OnRedraw;
		mOnLoaded = (bool success) => {
			if (success) { OnRedraw(); }
		};
	}

	void OnEnable() {
		OnRedraw();
	}

	void OnDisable() {
		if (mKey != null) {
			icon_atlas.ReleaseIcon(mKey);
			mKey = null;
		}
	}

	private bool TryInit() {
		// init when this monobehaviour is enabled
		if (!enabled || m_Image == null) { return false; }
		if (mKey == null) {
			Rect uv;
			mKey = icon_atlas.AllocIcon(mOnRedraw, out uv);
			m_Image.texture = icon_atlas.texture;
			m_Image.uvRect = uv;
		}
		// true when drawing is available.
		return true;
	}

	private void OnRedraw() {
		if (!TryInit()) { return; }
		icon_atlas.ClearIcon(mKey);
		switch (mCurrentData) {
			case HeroData hd:
				DrawHeroImpl(hd);
				break;
			case ItemData id:
				DrawBagItemImpl(id);
				break;
			case EquipData ed:
				DrawEquipmentImpl(ed);
				break;
		}
	}

	private bool DrawHeroImpl(HeroData data) {
		IconAssetHolder holder = IconAssetHolder.instance;
		bool ret = false;
		Sprite background;
		Sprite frame;
		if (mUseCircle) {
			holder.GetCircleSprites(data.level, out background, out frame);
		} else {
			holder.GetRectSprites(data.level, out background, out frame);
		}
		Rect regionContent = new Rect(
			ICON_INNER_PADDING + 4f, ICON_INNER_PADDING + 4f,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f
		);
		IconDrawProperties pBG = IconDrawProperties.Default;
		pBG.SetDrawRegion(regionContent);
		pBG.SetSaturation(data.saturation_all);
		DrawSprite(background, false, pBG);

		IconDrawProperties pIcon = IconDrawProperties.Default;
		pIcon.SetDrawRegion(regionContent);
		pIcon.SetSaturation(data.saturation_all * data.saturation_icon);
		if (mUseCircle) {
			Sprite mask = holder.CircleMask;
			pIcon.SetMask(mask.texture, GetSpriteOuterRect(mask), eMaskRegionType.DrawRegion);
		}
		Texture2D icon = FakeLoader.GetTextureSync(data.icon);
		if (icon != null) {
			ret = true;
			icon_atlas.Draw(mKey, icon, pIcon);
		} else {
			DrawSprite(holder.DefaultHeroIcon, false, pIcon);
		}

		IconDrawProperties pFR = IconDrawProperties.Default;
		pFR.SetDrawRegion(new Rect(
			ICON_INNER_PADDING, ICON_INNER_PADDING,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING
		));
		pFR.SetSaturation(data.saturation_all);
		DrawSprite(frame, false, pFR);

		Sprite rps = holder.GetRockPaperScissors(data.type);
		IconDrawProperties pRPS = IconDrawProperties.Default;
		Vector2 rpsSize = rps.rect.size;
		pRPS.SetDrawRegion(new Rect(
			ICON_WIDTH - rpsSize.x, ICON_HEIGHT - rpsSize.y,
			rpsSize.x, rpsSize.y
		));
		pRPS.SetSaturation(data.saturation_all);
		DrawSprite(rps, false, pRPS);
#if UNABLE_RENDER_TEXT
		// GreatArtText in Unity Asset Store is required in this code block
		if (!mUseCircle) {
			string heroname = data.name;
			TextToRenderTexture.ArtTextTexture txt = TextToRenderTexture.RenderText(
				holder.TextFont, heroname, 20, FontStyle.Normal,
				TextToRenderTexture.Parameter.Default
					// make a text outline with shadow extend other than outline fx to make text rendering faster
					.SetSpacialShadow(Vector2.zero, 1f, 0f, TextToRenderTexture.FxColor.Color(new Color(0f, 0f, 0f, 0.8f)), 1f)
					// rainbow coloring the text with gradient is also a good choice
					.SetTextGradient(holder.GetColorForLevel(data.level), 20f, false),
				TextToRenderTexture.eRenderTextureAllocType.Temporary
			);
			icon_atlas.Draw(
				mKey, txt.texture,
				IconDrawProperties.Default
					.SetDrawRegion(new Rect(
						ICON_WIDTH - ICON_INNER_PADDING - txt.texture.width - 6,
						ICON_INNER_PADDING + 6,
						txt.texture.width, txt.texture.height
					))
					.SetSaturation(data.saturation_all)
			);
			RenderTexture.ReleaseTemporary(txt.texture);
		}
#endif
		if (!ret) {
			FakeLoader.LoadTexture(data.icon, mOnLoaded);
		}
		return ret;
	}

	private bool DrawBagItemImpl(ItemData data) {
		IconAssetHolder holder = IconAssetHolder.instance;
		bool ret = false;
		Sprite background;
		Sprite frame;
		IconAssetHolder.instance.GetRectSprites(data.level, out background, out frame);
		Rect regionContent = new Rect(
			ICON_INNER_PADDING + 4f, ICON_INNER_PADDING + 4f,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f
		);

		IconDrawProperties pBG = IconDrawProperties.Default;
		pBG.SetDrawRegion(regionContent);
		pBG.SetSaturation(data.saturation_all);
		DrawSprite(background, false, pBG);

		IconDrawProperties pIcon = IconDrawProperties.Default;
		pIcon.SetDrawRegion(regionContent);
		pIcon.SetSaturation(data.saturation_all * data.saturation_icon);
		Texture2D icon = FakeLoader.GetTextureSync(data.icon);
		if (icon != null) {
			ret = true;
			icon_atlas.Draw(mKey, icon, pIcon);
		} else {
			DrawSprite(holder.DefaultItemIcon, false, pIcon);
		}

		IconDrawProperties pFR = IconDrawProperties.Default;
		pFR.SetDrawRegion(new Rect(
			ICON_INNER_PADDING, ICON_INNER_PADDING,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING
		));
		pFR.SetSaturation(data.saturation_all);
		DrawSprite(frame, false, pFR);
#if UNABLE_RENDER_TEXT
		string count = data.count.ToString();
		TextToRenderTexture.ArtTextTexture txt = TextToRenderTexture.RenderText(
			holder.TextFont, count, 24, FontStyle.Normal,
			TextToRenderTexture.Parameter.Default
				.SetSpacialShadow(Vector2.zero, 1f, 0f, TextToRenderTexture.FxColor.Color(new Color(0f, 0f, 0f, 0.8f)), 1f)
				.SetTextColor(Color.white),
			TextToRenderTexture.eRenderTextureAllocType.Temporary
		);
		icon_atlas.Draw(
			mKey, txt.texture,
			IconDrawProperties.Default
				.SetDrawRegion(new Rect(
					ICON_WIDTH - ICON_INNER_PADDING - txt.texture.width - 6,
					ICON_INNER_PADDING + 6,
					txt.texture.width, txt.texture.height
				))
				.SetSaturation(data.saturation_all)
		);
		RenderTexture.ReleaseTemporary(txt.texture);
#endif
		if (!ret) {
			FakeLoader.LoadTexture(data.icon, mOnLoaded);
		}
		return ret;
	}

	private bool DrawEquipmentImpl(EquipData data) {
		IconAssetHolder holder = IconAssetHolder.instance;
		bool ret = false;
		Sprite background;
		Sprite frame;
		IconAssetHolder.instance.GetRectSprites(data.level, out background, out frame);

		Rect regionContent = new Rect(
			ICON_INNER_PADDING + 4f, ICON_INNER_PADDING + 4f,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING - 8f
		);

		IconDrawProperties pBG = IconDrawProperties.Default;
		pBG.SetDrawRegion(regionContent);
		pBG.SetSaturation(data.saturation_all);
		DrawSprite(background, false, pBG);

		IconDrawProperties pIcon = IconDrawProperties.Default;
		pIcon.SetDrawRegion(regionContent);
		pIcon.SetSaturation(data.saturation_all * data.saturation_icon);
		Texture2D icon = FakeLoader.GetTextureSync(data.icon);
		if (icon != null) {
			ret = true;
			icon_atlas.Draw(mKey, icon, pIcon);
		} else {
			DrawSprite(holder.DefaultEquipIcon, false, pIcon);
		}

		IconDrawProperties pFR = IconDrawProperties.Default;
		pFR.SetDrawRegion(new Rect(
			ICON_INNER_PADDING, ICON_INNER_PADDING,
			ICON_WIDTH - ICON_INNER_PADDING - ICON_INNER_PADDING,
			ICON_HEIGHT - ICON_INNER_PADDING - ICON_INNER_PADDING
		));
		pFR.SetSaturation(data.saturation_all);
		DrawSprite(frame, false, pFR);

		Sprite tag = holder.GetEquipmentTag(data.type);
		IconDrawProperties pTag = IconDrawProperties.Default;
		Vector2 rpsSize = tag.rect.size;
		pTag.SetDrawRegion(new Rect(
			ICON_INNER_PADDING + 4f, ICON_HEIGHT - ICON_INNER_PADDING - 4f - rpsSize.y,
			rpsSize.x, rpsSize.y
		));
		pTag.SetSaturation(data.saturation_all);
		DrawSprite(tag, false, pTag);

		Sprite star = holder.Star;
		Vector2 pos = new Vector2(ICON_WIDTH - ICON_INNER_PADDING - 28f, ICON_INNER_PADDING + 10f);
		Vector2 size = new Vector2(20f, 20f);
		for (int i = data.stars - 1; i >= 0; i--) {
			IconDrawProperties pStar = IconDrawProperties.Default;
			pStar.SetDrawRegion(new Rect(pos, size));
			pStar.SetSaturation(data.saturation_all);
			DrawSprite(star, false, pStar);
			pos.x -= 22f;
		}

		if (!ret) {
			FakeLoader.LoadTexture(data.icon, mOnLoaded);
		}
		return ret;
	}

	private void DrawSprite(Sprite sprite, bool sliced, IconDrawProperties props) {
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Rect uv = new Rect(outer.x, outer.y, outer.z - outer.x, outer.w - outer.y);
		if (sliced) {
			Vector4 border = sprite.border;
			props.SetSliced(Mathf.RoundToInt(border.z), Mathf.RoundToInt(border.w), Mathf.RoundToInt(border.x), Mathf.RoundToInt(border.y));
			Vector2 size = new Vector2(1f / uv.width, 1f / uv.height);
			Vector4 inner = DataUtility.GetInnerUV(sprite);
			Vector4 borderUV = new Vector4((inner.x - outer.x) * size.x, (inner.y - outer.y) * size.y,
				(outer.z - inner.z) * size.x, (outer.w - inner.w) * size.y);
			props.SetSpriteBorders(borderUV.x, borderUV.y, borderUV.z, borderUV.w);
		}
		icon_atlas.Draw(mKey, sprite.texture, uv, props);
	}

	private static Rect GetSpriteOuterRect(Sprite sprite) {
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		return new Rect(outer.x, outer.y, outer.z - outer.x, outer.w - outer.y);
	}

}
