﻿using UnityEngine;
using System.Collections;

namespace Framework	
{
public class Boot : MonoBehaviour {

	// Use this for initialization
	void Start () {
		VersionMgr versionMgr = new VersionMgr();
		versionMgr.StartCheckVersion();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
}