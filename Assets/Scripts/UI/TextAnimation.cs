using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextAnimation : MonoBehaviour
{
    [SerializeField] public bool Enabled = true;
    [SerializeField] private float Speed;
    
    private TMP_Text Text;

    private void Awake()
    {
        Text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (!Enabled)
        {
            return;
        }
        
        Text.ForceMeshUpdate();

        TMP_TextInfo info = Text.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = info.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            Vector3[] verts = info.meshInfo[charInfo.materialReferenceIndex].vertices;
            
            // Find the verts for this character
            for (int j = 0; j < 4; j++)
            {
                var original = verts[charInfo.vertexIndex + j];
                
                // Add a sine wave offset
                verts[charInfo.vertexIndex + j] = original + new Vector3(0, Mathf.Sin(Time.time * 2f + original.x * 0.01f) * Speed, 0);
            }
        }

        for (int i = 0; i < info.meshInfo.Length; i++)
        {
            var meshInfo = info.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            Text.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    public void Reset()
    {
        Text.ForceMeshUpdate();

        TMP_TextInfo info = Text.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = info.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            Vector3[] verts = info.meshInfo[charInfo.materialReferenceIndex].vertices;
            
            // Find the verts for this character
            for (int j = 0; j < 4; j++)
            {
                var original = verts[charInfo.vertexIndex + j];
                
                // Add a sine wave offset
                verts[charInfo.vertexIndex + j] = original;
            }
        }

        for (int i = 0; i < info.meshInfo.Length; i++)
        {
            var meshInfo = info.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            Text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
