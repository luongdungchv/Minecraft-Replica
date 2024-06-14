using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{
    [SerializeField] private Texture2D tex, resultTex;
    [Sirenix.OdinInspector.Button]
    private void Start() {
        // var data = tex.EncodeToPNG();
        // StartCoroutine(Upload(data));
        var trs = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Debug.Log(trs);
    }
    IEnumerator Upload(byte[] data)
    {
        // Create a UnityWebRequest instance
        var formData = new WWWForm();
        formData.AddBinaryData("file", data, "image.png", "image/png");
        UnityWebRequest request = UnityWebRequest.Post("http://luongdungchv.pythonanywhere.com/convert", formData);

        DownloadHandlerTexture downloadHandler = new DownloadHandlerTexture();
        request.downloadHandler = downloadHandler;

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Image upload successful!");

            Texture2D downloadedTexture = downloadHandler.texture;
            this.resultTex = downloadedTexture;
        }
        else
        {
            Debug.LogError("Image upload failed: " + request.error);
        }
    }
}
