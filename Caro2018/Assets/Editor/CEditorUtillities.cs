using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CEditorUtillities: Editor {

	private static bool m_IsShooted = false;
	private static string m_DirPath = string.Empty;
	private static string m_FullPath = string.Empty;
	private static GameObject m_ScreenCaptureObj;

	[MenuItem("/Utilities/Screen capture")]
	public static void ScreenCaptureGame() {
		EditorUtility.ClearProgressBar();
		m_DirPath = string.Format("{0}/screenshot", Environment.CurrentDirectory);
		var fileName = DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss");
		if (Directory.Exists(m_DirPath) == false) {
			Directory.CreateDirectory(m_DirPath);
		}
		m_FullPath = Path.Combine(m_DirPath, fileName + ".png");
		ScreenCapture.CaptureScreenshot(m_FullPath);
	}

	[MenuItem("/Utilities/Screen capture with object name")]
	public static void ScreenCaptureGameWitObjName() {
		EditorUtility.ClearProgressBar();
		if (Selection.activeGameObject != null) {
			m_DirPath = string.Format("{0}/screenshot", Environment.CurrentDirectory);
			if (Directory.Exists(m_DirPath) == false) {
				Directory.CreateDirectory(m_DirPath);
			}
			var fileName = Selection.activeGameObject.name;
			m_FullPath = Path.Combine(m_DirPath, fileName + ".png");
			ScreenCapture.CaptureScreenshot(m_FullPath);
			m_IsShooted = true;
			m_ScreenCaptureObj = new GameObject("Screen Capture");
			EditorApplication.update -= HandleScreenCaptureGameWitObjName; 
			EditorApplication.update += HandleScreenCaptureGameWitObjName; 
		}
	}

	protected static void HandleScreenCaptureGameWitObjName() {
		if (m_IsShooted) {
			if (File.Exists(m_FullPath) == false) {
				if (EditorUtility.DisplayCancelableProgressBar("Generate screenshot", "file://" + m_FullPath, 0.5f)) {
					EditorUtility.ClearProgressBar();
					m_IsShooted = false;
					m_FullPath = string.Empty;
					GameObject.DestroyImmediate(m_ScreenCaptureObj);
				}
				EditorSceneManager.SaveOpenScenes();
			} else {
				EditorUtility.DisplayProgressBar("Generate screenshot", "file://" + m_FullPath, 1f);
				EditorUtility.ClearProgressBar();
				m_IsShooted = false;
				m_FullPath = string.Empty;
				GameObject.DestroyImmediate(m_ScreenCaptureObj);
			}
		} 
	}

}
