using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using FacialRecognitionDoor;

//A simple C# class to post messages to a Slack channel
//Note: This class uses the Newtonsoft Json.NET serializer available via NuGet
public class SlackClient
{
	private readonly Uri _uri;
    public Uri uri;
	private readonly Encoding _encoding = new UTF8Encoding();
	
	public SlackClient(string urlWithAccessToken)
	{
		_uri = new Uri(urlWithAccessToken);
	}
	
	//Post a message using simple strings
	public void PostMessage(string text, string username = null, string channel = null)
	{
		Payload payload = new Payload()
		{
			Channel = channel,
			Username = username,
			Text = text
		};
		PostMessage(payload);
	}

    //Post a message using a Payload object
    public async void PostMessage(Payload payload)
    {
        string payloadJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(payloadJson);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var client = new HttpClient();
        uri = new Uri(GeneralConstants.SlackURI);
        await client.PostAsync(uri, content);
    }
}

//This class serializes into the Json payload required by Slack Incoming WebHooks
public class Payload
{
	[JsonProperty("channel")]
	public string Channel { get; set; }
	
	[JsonProperty("username")]
	public string Username { get; set; }
	
	[JsonProperty("text")]
	public string Text { get; set; }
}