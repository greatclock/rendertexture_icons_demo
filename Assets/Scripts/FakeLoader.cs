using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeLoader : MonoBehaviour {

	private static FakeLoader s_instance;
	private static bool s_inited = false;

	public delegate void OnLoadedDelegate(bool success);

	public static Texture2D GetTextureSync(string path) {
		if (!s_inited) { return null; }
		if (s_instance == null || s_instance.Equals(null)) {
			return null;
		}
		return s_instance.GetTex(path);
	}

	public static void LoadTexture(string path, OnLoadedDelegate onLoaded) {
		if (!s_inited) {
			s_inited = true;
			GameObject go = new GameObject("FakeLoader");
			DontDestroyOnLoad(go);
			s_instance = go.AddComponent<FakeLoader>();
			s_instance.mHead.Next = s_instance.mTail;
			s_instance.mTail.Prev = s_instance.mHead;
		}
		if (s_instance == null || s_instance.Equals(null)) { return; }
		s_instance.LoadTex(path, onLoaded);
	}

	private Dictionary<string, TextureData> mLoaded = new Dictionary<string, TextureData>(256);
	private Dictionary<string, List<OnLoadedDelegate>> mLoading = new Dictionary<string, List<OnLoadedDelegate>>();

	private TextureData mHead = new TextureData(null, null);
	private TextureData mTail = new TextureData(null, null);

	private Texture2D GetTex(string path) {
		TextureData data;
		if (!mLoaded.TryGetValue(path, out data)) { return null; }
		data.Prev.Next = data.Next;
		data.Next.Prev = data.Prev;
		data.Prev = mTail.Prev;
		data.Prev.Next = data;
		data.Next = mTail;
		mTail.Prev = data;
		data.Duetime = Time.realtimeSinceStartup + 30f;
		return data.Tex;
	}

	private void LoadTex(string path, OnLoadedDelegate onLoaded) {
		if (mLoaded.ContainsKey(path)) { return; }
		List<OnLoadedDelegate> list;
		if (!mLoading.TryGetValue(path, out list)) {
			list = new List<OnLoadedDelegate>();
			mLoading.Add(path, list);
			StartCoroutine(LoadTexAsync(path));
		}
		list.Add(onLoaded);
	}

	private IEnumerator LoadTexAsync(string path) {
		ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
		yield return req;
		Texture2D tex = req.asset as Texture2D;
		List<OnLoadedDelegate> list = mLoading[path];
		mLoading.Remove(path);
		bool success = true;
		if (tex == null) {
			success = false;
		} else {
			TextureData data = new TextureData(path, tex);
			data.Duetime = Time.realtimeSinceStartup + 30f;
			mLoaded.Add(path, data);
			data.Prev = mTail.Prev;
			data.Prev.Next = data;
			data.Next = mTail;
			mTail.Prev = data;
		}
		foreach (OnLoadedDelegate onLoaded in list) {
			onLoaded(success);
		}
	}

	void Update() {
		TextureData cur = mHead.Next;
		float now = Time.realtimeSinceStartup;
		while (cur != mTail) {
			TextureData next = cur.Next;
			if (cur.Duetime > now) {
				cur.Prev.Next = cur.Next;
				cur.Next.Prev = cur.Prev;
				mLoaded.Remove(cur.Path);
			}
			cur = next;
		}
	}

	private class TextureData {
		public readonly string Path;
		public readonly Texture2D Tex;
		public TextureData Prev;
		public TextureData Next;
		public float Duetime;
		public TextureData(string path, Texture2D tex) { Path = path; Tex = tex; }
	}

}
