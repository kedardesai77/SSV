using UnityEngine;
using System.Collections;

public class Screenshot : MonoBehaviour {

    private string _data = string.Empty;

    public IEnumerator ScreeAndSave()
    {

        yield return new WaitForEndOfFrame();
        Texture2D newTexture = ScreenShoot(Camera.main, Screen.width, Screen.height);
        //LerpTexture(bg, ref newTexture);
        _data = System.Convert.ToBase64String(newTexture.EncodeToPNG());
        //Application.ExternalEval("document.location.href='data:image/octet-stream;base64," + _data + "'");



        WWWForm form = new WWWForm();
        form.AddField("sa", "screenshot");
        form.AddField("base64", _data);

        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
        yield return www;

        if (www.error != null)
        {
            Application.ExternalEval("alert('Could not save screenshot.')");
        }
        else
        {
            string gd = www.text;
            Application.ExternalEval("ReceiveSafeSimScreenshot('" + gd + "')");
            Debug.Log(_data);
        }

    }


    private static Texture2D ScreenShoot(Camera srcCamera, int width, int height)
    {
        RenderTexture renderTexture = new RenderTexture(width, height, 0);
        Texture2D targetTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        renderTexture.depth = 24;

        srcCamera.targetTexture = renderTexture;
        srcCamera.Render();

        RenderTexture.active = renderTexture;

        targetTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        targetTexture.Apply();

        srcCamera.targetTexture = null;
        RenderTexture.active = null;
        srcCamera.ResetAspect();
        return targetTexture;

    }


}
