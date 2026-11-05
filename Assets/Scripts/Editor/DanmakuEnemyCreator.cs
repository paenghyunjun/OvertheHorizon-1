using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 상단 메뉴 Tools > OverTheHorizon 에서 박스 탄막 적을 한 번에 만들어 주는 에디터 도구
public static class DanmakuEnemyCreator
{
    private const string BoxSpritePath = "Assets/Sprites/EnemyBox.png";

    [MenuItem("Tools/OverTheHorizon/탄막 적(박스) 만들기", priority = 0)]
    [MenuItem("GameObject/OverTheHorizon/탄막 적(박스)", priority = 10)]
    private static void CreateBoxEnemy()
    {
        var go = new GameObject("DanmakuEnemy (Box)");
        go.transform.position = new Vector3(0f, 7f, 0f); // 화면 위 밖에서 내려오며 등장
        go.transform.localScale = Vector3.one * 0.8f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateBoxSprite();
        sr.color = new Color(0.8f, 0.35f, 0.9f);
        sr.sortingOrder = 5;

        go.AddComponent<DanmakuEnemy>();

        Undo.RegisterCreatedObjectUndo(go, "탄막 적 만들기");
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        Debug.Log("[DanmakuEnemyCreator] 탄막 적을 만들었습니다. 플레이하면 위에서 내려와 패턴을 시작합니다.");
    }

    // 플레이 중에는 메뉴 비활성화
    [MenuItem("Tools/OverTheHorizon/탄막 적(박스) 만들기", true)]
    [MenuItem("GameObject/OverTheHorizon/탄막 적(박스)", true)]
    private static bool ValidateCreateBoxEnemy()
    {
        return !EditorApplication.isPlaying;
    }

    // 에디터에서도 박스가 보이도록 흰 사각형 이미지를 에셋으로 만들어 둠 (처음 한 번만)
    private static Sprite GetOrCreateBoxSprite()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoxSpritePath);
        if (sprite != null) return sprite;

        Directory.CreateDirectory(Path.GetDirectoryName(BoxSpritePath));

        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(BoxSpritePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(BoxSpritePath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(BoxSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size; // 1 유닛 크기
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(BoxSpritePath);
    }
}
