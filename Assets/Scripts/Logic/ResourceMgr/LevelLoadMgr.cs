﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
public class LevelLoadMgr
{
	public static void LoadLevelSync(string scene)
	{
		SceneManager.LoadScene(scene);
	}

	public static AsyncOperation LoadLevelAsync(string scene)
	{
		return SceneManager.LoadSceneAsync(scene);
	}
}
}