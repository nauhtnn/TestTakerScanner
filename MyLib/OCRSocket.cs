using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Collections;

namespace MyLib
{
    public class OCRSocket
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

            HttpResponseMessage response = null;

            // Request body
            byte[] byteData = GetImageAsByteArray(imgPath);

            textURL = string.Empty;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    response = client.PostAsync(uri, content).Result;
                }
                catch (System.AggregateException er)
                {
                    response = null;
                }
            }

            if(response == null)
            {
                textURL = string.Empty;
                return;
            }

            // Asynchronously get the JSON response.
            string contentString = response.Content.ReadAsStringAsync().Result;
            try
            {
                var operation_location = response.Headers.GetValues("Operation-Location").GetEnumerator();
                operation_location.MoveNext();
                textURL = operation_location.Current;
            }
            catch(System.InvalidOperationException e)
            {
                textURL = string.Empty;
            }
        }

        public static async void GetImageText()
        {
            if (textURL.Length == 0)
                return;
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "26b4d99217f645818a4d2e049f3865fc");
            HttpResponseMessage x;
            try
            {
                x = client.GetAsync(textURL).Result;
            }
            catch (System.AggregateException er)
            {
                x = null;
            }

            if (x == null)
            {
                imgText.Clear();
                return;
            }
            string y = x.Content.ReadAsStringAsync().Result;
            //System.IO.File.WriteAllText("json.txt", y);
            JToken parent = JToken.Parse(y).Last;
            while (parent.HasValues && parent.First == parent.Last)
                parent = parent.First;
            imgText.Clear();
            TextLineList ocrText = new TextLineList();
            if (parent.HasValues)
            {
                JToken i = parent.First;
                int maxImageX = 0;
                do
                {
                    var t = i.Value<JArray>("boundingBox");
                    if (maxImageX < (int)t[2])
                        maxImageX = (int)t[2];
                    i = i.Next;
                }
                while (i != parent.Last);
                TextLine.ImageX4 = maxImageX / 4;
                i = parent.First;
                do
                {
                    var t = i.Value<JArray>("boundingBox");
                    if (TextLine.ImageX4 < (int)t[0])
                    {
                        i = i.Next;
                        continue;
                    }
                    ocrText.Add(new TextRun(t, i.Value<string>("text")));
                    i = i.Next;
                }
                while (i != parent.Last);
                i = parent.First;
                do
                {
                    var t = i.Value<JArray>("boundingBox");
                    if ((int)t[0] <= TextLine.ImageX4)
                    {
                        i = i.Next;
                        continue;
                    }
                    ocrText.Add(new TextRun(t, i.Value<string>("text")));
                    i = i.Next;
                }
                while (i != parent.Last);
                imgText.Append(ocrText.ToString());
            }
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

    class TextRun : IComparable
    {
        public int TopLeftX, TopLeftY, BottomLeftY, TopRightY, BottomRightY;
        public string Value;

        public TextRun(JArray box, string text)
        {
            TopLeftX = (int)box[0];
            TopLeftY = (int)box[1];
            TopRightY = (int)box[3];
            BottomRightY = (int)box[5];
            BottomLeftY = (int)box[7];
            Value = text;
        }

        public int CompareTo(object obj)
        {
            TextRun run = obj as TextRun;
            if (TopLeftX == run.TopLeftX)
                return 0;
            else if (TopLeftX < run.TopLeftX)
                return -1;
            else
                return 1;
        }
    }

    class TextLine : IComparable
    {
        public static int ImageX4 = 0;
        public int TopRightY, BottomRightY;
        public ArrayList vText;
        public TextLine()
        {
            TopRightY = BottomRightY = 0;
            vText = new ArrayList();
        }
        public bool IsInlined(TextRun run)
        {
            int y = (run.TopLeftY + run.BottomLeftY) / 2;
            if (TopRightY <= y && y <= BottomRightY)
                return true;
            return false;
        }
        public void Add(TextRun run)
        {
            vText.Add(run);
            vText.Sort();
            TextRun r = vText[vText.Count - 1] as TextRun;
            TopRightY = r.TopRightY;
            BottomRightY = r.BottomRightY;
        }

        public int CompareTo(object obj)
        {
            TextLine line = obj as TextLine;
            if (TopRightY < line.TopRightY)
                return -1;
            else
                return 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (TextRun r in vText)
                sb.Append(r.Value + " ");
            return sb.ToString();
        }
    }

    class TextLineList
    {
        ArrayList vLine;
        public TextLineList()
        {
            vLine = new ArrayList();
        }
        public void Add(TextRun run)
        {
            if (run.BottomLeftY - run.TopLeftY < 10)//remove noise
                return;
            foreach (TextLine line in vLine)
                if (line.IsInlined(run))
                {
                    line.Add(run);
                    return;
                }
            if (TextLine.ImageX4 < run.TopLeftX)
                return;
            TextLine li = new TextLine();
            li.Add(run);
            vLine.Add(li);
            vLine.Sort();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (TextLine li in vLine)
                sb.Append(li.ToString() + "\n");
            return sb.ToString();
        }
    }
}
