using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;

namespace TestTakerScanner
{
    static class Program2
    {
        static void Main()
        {
            MakeRequest();
            //QueryText("https://southeastasia.api.cognitive.microsoft.com/vision/v2.0/textOperations/d3d946e2-7b32-4843-9d35-fa09926f1cad");
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadLine();
        }

        static async void MakeRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "26b4d99217f645818a4d2e049f3865fc");

            // Request parameters
            queryString["mode"] = "Printed";
            var uri = "https://southeastasia.api.cognitive.microsoft.com/vision/v2.0/recognizeText?" + queryString;

            HttpResponseMessage response;

            // Request body
            byte[] byteData = GetImageAsByteArray("Bich_Tuyen_180330_2.jpg");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
            }

            // Asynchronously get the JSON response.
            string contentString = await response.Content.ReadAsStringAsync();
            var operation_location = response.Headers.GetValues("Operation-Location").GetEnumerator();
            operation_location.MoveNext();
            Console.WriteLine(operation_location.Current);
            QueryText(operation_location.Current);

            // Display the JSON response.
            Console.WriteLine(contentString);
        }

        static async void QueryText(string uri)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "26b4d99217f645818a4d2e049f3865fc");
            var x = await client.GetAsync(uri);
            var y = x.Content.ReadAsStringAsync().Result;
            JToken parent = JToken.Parse(y).Last;
            while (parent.First == parent.Last)
                parent = parent.First;
            JToken i = parent.First;
            while(i != parent.Last)
            {
                Console.WriteLine(i.Value<string>("text"));
                i = i.Next;
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
}