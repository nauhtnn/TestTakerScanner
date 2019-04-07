using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;

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
                while (i != null)
                {
                    var t = i.Value<JArray>("boundingBox");
                    if (maxImageX < (int)t[2])
                        maxImageX = (int)t[2];
                    i = i.Next;
                }
                TextLine.ImageX4 = maxImageX / 4;
                int[] vStart = { 0, TextLine.ImageX4 };
                int[] vEnd = { TextLine.ImageX4, TextLine.ImageX4 * 3};
                for(int j = 0; j < vStart.Length; ++j)
                {
                    i = parent.First;
                    while (i != null)
                    {
                        var t = i.Value<JArray>("boundingBox");
                        if (vStart[j] <= (int)t[0] && (int)t[0] <= vEnd[j])
                            ocrText.Add(new TextRun(t, i.Value<string>("text")));
                        i = i.Next;
                    }
                }
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
        public int TopLeftX, TopLeftY, BottomLeftY, TopRightY, BottomRightY, TopRightX;
        public string Value;

        public TextRun(JArray box, string text)
        {
            TopLeftX = (int)box[0];
            TopLeftY = (int)box[1];
            TopRightY = (int)box[3];
            BottomRightY = (int)box[5];
            BottomLeftY = (int)box[7];
            TopRightX = (int)box[2];
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
        public int TopRightY, BottomRightY, TopLeftY, BottomLeftY;
        public int TopRightX;
        public ArrayList vText;
        public TextLine()
        {
            TopRightY = BottomRightY = TopLeftY = BottomLeftY = TopRightX = 0;
            vText = new ArrayList();
        }
        public int TestInlined(TextRun run)
        {
            int ry, topY, bottomY;
            if(TopRightX < run.TopLeftX)
            {
                ry = (run.TopLeftY + run.BottomLeftY) / 2;
                topY = TopRightY;
                bottomY = BottomRightY;
            }
            else
            {
                ry = (run.TopRightY + run.BottomRightY) / 2;
                topY = TopLeftY;
                bottomY = BottomLeftY;
            }
            if (ry < topY)
                return -1;
            else if (bottomY < ry)
                return 1;
            else
                return 0;
        }
        public void Add(TextRun run)
        {
            vText.Add(run);
            vText.Sort();
            TextRun r = vText[vText.Count - 1] as TextRun;
            TopRightY = r.TopRightY;
            BottomRightY = r.BottomRightY;
            TopRightX = r.TopRightX;
            r = vText[0] as TextRun;
            TopLeftY = r.TopLeftY;
            BottomLeftY = r.BottomLeftY;
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
            {
                Regex rx = new Regex("[AB][0-9]+");
                if(!rx.IsMatch(run.Value))
                    return;
            }
            int start = 0, end = vLine.Count - 1;
            while(start <= end)
            {
                int middle = (start + end) / 2;
                int testInlined = (vLine[middle] as TextLine).TestInlined(run);
                if (testInlined == 0)
                {
                    (vLine[middle] as TextLine).Add(run);
                    vLine.Sort();
                    return;
                }
                if (testInlined < 0)
                    end = middle - 1;
                else
                    start = middle + 1;
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
