using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;

namespace MyLib
{
    public class Class1
    {
        public static string textURL;
        public static StringBuilder imgText;
        public static void Init()
        {
            textURL = string.Empty;
            imgText = new StringBuilder();
        }
        public static async void PostImageToGetURL(string imgPath)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "26b4d99217f645818a4d2e049f3865fc");

            // Request parameters
            queryString["mode"] = "Printed";
            var uri = "https://southeastasia.api.cognitive.microsoft.com/vision/v2.0/recognizeText?" + queryString;
            //var uri = "https://southeastasia.api.cognitive.microsoft.com/vision/v2.0/ocr?language=en&detectOrientation=false";

            HttpResponseMessage response;

            // Request body
            byte[] byteData = GetImageAsByteArray(imgPath);

            textURL = string.Empty;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = client.PostAsync(uri, content).Result;
            }

            // Asynchronously get the JSON response.
            string contentString = response.Content.ReadAsStringAsync().Result;
            var operation_location = response.Headers.GetValues("Operation-Location").GetEnumerator();
            operation_location.MoveNext();
            textURL = operation_location.Current;
        }

        public static async void GetImageText()
        {
            if (textURL.Length == 0)
                return;
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "26b4d99217f645818a4d2e049f3865fc");
            var x = client.GetAsync(textURL).Result;
            var y = x.Content.ReadAsStringAsync().Result;
            //System.IO.File.WriteAllText("json.txt", y);
            JToken parent = JToken.Parse(y).Last;
            while (parent.HasValues && parent.First == parent.Last)
                parent = parent.First;
            if (parent.HasValues)
            {
                JToken i = parent.First;
                while (i != parent.Last)
                {
                    var t = i.Value<JArray>("boundingBox");
                    imgText.Append(i.Value<string>("text") + "\n");
                    i = i.Next;
                }
            }
            else
                imgText.Append("Running");
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
