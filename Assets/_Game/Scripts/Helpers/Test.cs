using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{
    [SerializeField] private Texture2D tex, resultTex;


    [SerializeField] private Box test1, test2;
    private void Start()
    {
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
    [Sirenix.OdinInspector.Button]
    private void Test3(){
        Debug.Log(IsCubesOverlap(test1, test2, out var normal));
        Debug.Log(normal);
    }
    bool IsCubesOverlap(Box b1, Box b2, out Vector3 normal)
    {
        Vector2 b1X = GetProjectionRange(b1, new Vector2(1, 0));
        Vector2 b2X = GetProjectionRange(b2, new Vector2(1, 0));

        Vector2 b1Y = GetProjectionRange(b1, new Vector2(0, 1));
        Vector2 b2Y = GetProjectionRange(b2, new Vector2(0, 1));

        float b1XCenter = (b1X.x + b1X.y) / 2;
        float b2XCenter = (b2X.x + b2X.y) / 2;
        float collideDistX = (b1X.y - b1X.x) / 2 + (b2X.y - b2X.x) / 2;
        float trueDistX = Mathf.Abs(b1XCenter - b2XCenter);

        float b1YCenter = (b1Y.x + b1Y.y) / 2;
        float b2YCenter = (b2Y.x + b2Y.y) / 2;
        float collideDistY = (b1Y.y - b1Y.x) / 2 + (b2Y.y - b2Y.x) / 2;
        float trueDistY = Mathf.Abs(b1YCenter - b2YCenter);

        float collideDistZ = (b1.extents.y + b2.extents.y);
        float trueDistZ = Mathf.Abs(b1.center.y - b2.center.y);

        float xDiff = trueDistX - collideDistX;
        float yDiff = trueDistY - collideDistY;
        float zDiff = trueDistZ - collideDistZ;

        float minDiff = Mathf.Min(Mathf.Min(xDiff, yDiff), zDiff);
        if (xDiff > 0 || yDiff > 0 || zDiff > 0)
        {
            normal = Vector3.zero;
            return false;
        }
        float maxDiff = Mathf.Max(Mathf.Max(xDiff, yDiff), zDiff);
        if (xDiff == maxDiff) normal = new Vector3(1, 0, 0);
        else if (yDiff == maxDiff) normal = new Vector3(0, 1, 0);
        else if (zDiff == maxDiff) normal = new Vector3(0, 0, 1);
        else normal = new Vector3(0, 0, 0);
        Debug.Log((xDiff, yDiff, zDiff));
        Debug.Log((b1Y, b2Y));
        return true;
    }

    Vector2 GetProjectionRange(Box b, Vector2 projMask)
    {
        Vector2[] dirs = new Vector2[]{
        new Vector2(1, 1),
        new Vector2(-1, 1),
        new Vector2(-1, -1),
        new Vector2(1, -1)
    };

        float minVal = 2147483647;
        float maxVal = -minVal;

        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = dirs[i] * b.extents.XZ();
            Vector2 corner = dir;

            float sine = Mathf.Sin(b.orientation * Mathf.Deg2Rad);
            float cosine = Mathf.Cos(b.orientation * Mathf.Deg2Rad);
            var temp = corner;
            corner.x = temp.x * cosine - temp.y * sine;
            corner.y = temp.x * sine + temp.y * cosine;
            if(b.extents.y == 1 && projMask == Vector2.up) Debug.Log(corner);
            corner += b.center.XZ();

            corner *= projMask;
            float length = corner.x + corner.y;
            if (length < minVal)
            {
                minVal = length;
            }
            if (length > maxVal) maxVal = length;

        }

        

        return new Vector2(minVal, maxVal);
    }

    [System.Serializable]
    public struct Box
    {
        public Vector3 center;
        public Vector3 extents;
        public float orientation;
    };
}
