/*MIT License

Copyright(c) 2020 Mikhail5412

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityAndroidOpenUrl
{
    public static class AndroidOpenUrl
    {
        public static void OpenFile(string url, string dataType = "application/pdf")
        {
            AndroidJavaObject clazz = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = clazz.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("addFlags", intent.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
            intent.Call<AndroidJavaObject>("setAction", intent.GetStatic<string>("ACTION_VIEW"));

            var apiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");

            AndroidJavaObject uri;
            if (apiLevel > 23)
            {
                AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
                AndroidJavaObject file = new AndroidJavaObject("java.io.File", url);

                AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                string packageName = unityContext.Call<string>("getPackageName");
                string authority = packageName + ".fileprovider";

                uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, file);
            }
            else
            {
                var uriClazz = new AndroidJavaClass("android.net.Uri");
                var file = new AndroidJavaObject("java.io.File", url);
                uri = uriClazz.CallStatic<AndroidJavaObject>("fromFile", file);
            }

            intent.Call<AndroidJavaObject>("setType", dataType);
            intent.Call<AndroidJavaObject>("setData", uri);

            currentActivity.Call("startActivity", intent);
        }
    }
}
