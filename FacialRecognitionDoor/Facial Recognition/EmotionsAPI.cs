using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace FacialRecognitionDoor.Facial_Recognition
{
    class EmotionsAPI
    {
        public async Task<string> analyze(StorageFile photo, string username)
        {
            // Setup http content using stream of captured photo
            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            HttpStreamContent streamContent = new HttpStreamContent(stream);
            streamContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/octet-stream");

            // Setup http request using content
            Uri apiEndPoint = new Uri("https://api.projectoxford.ai/emotion/v1.0/recognize");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiEndPoint);
            request.Content = streamContent;

            // Do an asynchronous POST.
            string apiKey = "1dd1f4e23a5743139399788aa30a7153"; //Replace this with your own Microsoft Cognitive Services Emotion API key from https://www.microsoft.com/cognitive-services/en-us/emotion-api. Please do not use my key. I include it here so you can get up and running quickly
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            HttpResponseMessage response = await httpClient.SendRequestAsync(request).AsTask();

            // Read response
            string responseContent = await response.Content.ReadAsStringAsync();

            //En total 6 estados de ánimo aunque contempla también indiferencia y disgustado
            string tobesearched;
            //Enfado
            tobesearched = "anger\":";
            string enfado = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            enfado = enfado.Substring(0, enfado.IndexOf(","));
            decimal enfa = decimal.Parse(enfado, NumberStyles.Float) * 100;
            string enfadosub = enfa.ToString().Substring(0, enfa.ToString().IndexOf(".")) + " %";

            //Asustado
            tobesearched = "fear\":";
            string asustado = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            asustado = asustado.Substring(0, asustado.IndexOf(","));
            decimal asus = decimal.Parse(asustado, NumberStyles.Float) * 100;
            string fearsub = asus.ToString().Substring(0, asus.ToString().IndexOf(".")) + " %";

            //Felicidad
            tobesearched = "happiness\":";
            string felicidad = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            felicidad = felicidad.Substring(0, felicidad.IndexOf(","));
            decimal feli = decimal.Parse(felicidad, NumberStyles.Float) * 100;
            string felisub = feli.ToString().Substring(0, feli.ToString().IndexOf(".")) + " %";

            //Neutral
            tobesearched = "neutral\":";
            string neutral = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            neutral = neutral.Substring(0, neutral.IndexOf(","));
            decimal neutro = decimal.Parse(neutral, NumberStyles.Float) * 100;
            string neutrosub = neutro.ToString().Substring(0, neutro.ToString().IndexOf(".")) + " %";

            //Tristeza
            tobesearched = "sadness\":";
            string tristeza = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            tristeza = tristeza.Substring(0, tristeza.IndexOf(","));
            decimal triste = decimal.Parse(tristeza, NumberStyles.Float) * 100;
            string tristesub = triste.ToString().Substring(0, triste.ToString().IndexOf(".")) + " %";

            //Sorpresa
            tobesearched = "surprise\":";
            string sorpresa = responseContent.Substring(responseContent.IndexOf(tobesearched) + tobesearched.Length);
            sorpresa = sorpresa.Substring(0, sorpresa.IndexOf("}"));
            decimal sorpre = decimal.Parse(sorpresa, NumberStyles.Float) * 100;
            string sorpresasub = sorpre.ToString().Substring(0, sorpre.ToString().IndexOf(".")) + " %";

           /* Debug.WriteLine("Enfado: " + enfadosub + "\n");
            Debug.WriteLine("Asustado: " + fearsub + "\n");
            Debug.WriteLine("Felicidad: " + felisub + "\n");
            Debug.WriteLine("Neutral: " + neutrosub + "\n");
            Debug.WriteLine("Triste: " + tristesub + "\n");
            Debug.WriteLine("Sorpresa: " + sorpresasub + "\n");*/

            string predominante;

            decimal[] numeros = { enfa, asus, feli, neutro, triste, sorpre };
            decimal max = numeros.Max();
            int maxindex = numeros.ToList().IndexOf(max);

            switch (maxindex)
            {
                case 0:
                    predominante = "enfadado";
                    break;
                case 1:
                    predominante = "asustado";
                    break;
                case 2:
                    predominante = "feliz";
                    break;
                case 3:
                    predominante = "neutro";
                    break;
                case 4:
                    predominante = "triste";
                    break;
                case 5:
                    predominante = "sorprendido";
                    break;
                default:
                    predominante = "";
                    break;
            }
           // await Task.Run(async () => { await AzureIoTHub.SendHumorAsync(predominante); });
            TestPostMessage("Le veo a: "+username+" un " + max.ToString().Substring(0, max.ToString().IndexOf(".")) + " % " + predominante+" en estos momentos");
            return predominante;
        }

        public void TestPostMessage(string message)
        {
            string urlWithAccessToken = GeneralConstants.SlackURI;

            SlackClient client = new SlackClient(urlWithAccessToken);

            client.PostMessage(username: "IronDoor",
                       text: message,
                       channel: "#status");
        }
    }
}
