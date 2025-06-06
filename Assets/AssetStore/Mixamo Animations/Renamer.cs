// Taken from https://github.com/gamedevserj/Useful-Unity-Scripts/blob/master/Assets/Scripts/Editor/Automatic/RenameAnimationsOnImport.cs
// PREVENTS EDITING ANIMATIONS!!! DISABLE ASAP!!!

// using UnityEngine;
// using UnityEditor;

// public class RenameAnimationsOnImport : AssetPostprocessor {
// 	readonly string nameSeparationCharacter = "|"; // Blender animations have | as a separator

// 	// Renames clips in the inspector window (animation tab of the model)
// 	void OnPostprocessModel(GameObject g) {
// 		ModelImporter modelImporter = (ModelImporter)assetImporter;
// 		ModelImporterClipAnimation[] animations = modelImporter.defaultClipAnimations;

// 		if (animations.Length > 1) {
// 			for (int k = 0; k < animations.Length; k++) {
// 				string animationName = animations[k].name;
// 				int lastSeparator = animationName.LastIndexOf(nameSeparationCharacter) + 1;
// 				animationName = animationName.Substring(lastSeparator);
// 				animations[k].name = animationName;
// 			}
// 		} else if (animations.Length == 1) {
// 			animations[0].name = g.name;
// 		}
// 		modelImporter.clipAnimations = animations;
// 	}

// 	// Renames clips on the asset itself
// 	void OnPostprocessAnimation(GameObject g, AnimationClip clip) {
// 		if (!clip.name.Contains("mixamo.com")) {
// 			int lastSeparator = clip.name.LastIndexOf(nameSeparationCharacter) + 1;
// 			string name = clip.name.Substring(lastSeparator);
// 			clip.name = name;
// 		} else {
// 			clip.name = g.name;
// 		}
// 	}
// }
