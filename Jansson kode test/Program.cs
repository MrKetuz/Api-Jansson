using System;
using RestSharp;

class Program
{
    static void Main()
    {
        string username, password, access_token = "", refresh_token = "";
        DateTime expires = DateTime.UtcNow;

        string tokenUrl = "https://api.jansson.dk/v1";

        var client = new RestClient(tokenUrl);

        
        
        while (access_token == "") 
        {
            Console.WriteLine("Please enter a username");
            username = Console.ReadLine();

            Console.WriteLine("Please enter a password");
            password = Console.ReadLine();

            var request = new RestRequest("authentication/logon", Method.Post);

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=password&username={username}&password={password}", ParameterType.RequestBody);

            try
            {
                RestResponse<Response> response = client.Execute<Response>(request);

                if (response.IsSuccessful) 
                {
                    access_token = response.Data.access_token;
                    refresh_token = response.Data.refresh_token;
                    expires = DateTime.UtcNow.AddSeconds(response.Data.expires_in);
                    Console.WriteLine("Sucessfully got tokens!");
                }
                else
                {
                    Console.WriteLine(response.ResponseStatus+response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error attempting to get access token: {ex.Message}");
            }
        }

        var getProfileRequest = new RestRequest("profile/detailed", Method.Get);
        if (expires <= DateTime.UtcNow)
        {
            Console.WriteLine("Token expired, attempting to get a new one via refresh token...");
            string[] tokens = refreshAccessToken(client, refresh_token);
            access_token = tokens[0];
            refresh_token = tokens[1];
        }

        try
        {
            getProfileRequest.AddHeader("Authorization", $"Bearer {access_token}");

            RestResponse profile = client.Execute(getProfileRequest);

            Console.WriteLine(profile.Content);
        }
        catch(Exception ex) 
        {
            Console.WriteLine($"Error attempting to get detailed profile: {ex.Message}");
        }

    }
/// <summary>
/// Takes a RestClient and a refresh token to generate a new access token.
/// </summary>
/// <param name="client"></param>
/// <param name="refresh_token"></param>
/// <returns>
/// String array with new access token and refresh token
/// </returns>
       static string[] refreshAccessToken (RestClient client, string refresh_token)
       {

        var requestRefresh = new RestRequest("authentication/logon", Method.Post);

        requestRefresh.AddHeader("content-type", "application/x-www-form-urlencoded");
        requestRefresh.AddParameter("application/x-www-form-urlencoded", $"grant_type=refresh_token&refresh_token={refresh_token}", ParameterType.RequestBody);

        try
        {
            RestResponse<Response> response = client.Execute<Response>(requestRefresh);

            if (response.IsSuccessful)
            {
                Console.WriteLine("Sucessfully got new tokens");
                string[] tokens = new string[] { response.Data.access_token, response.Data.refresh_token };
                return tokens;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error attempting to refresh access token: {ex.Message}");
            return null;
        }
        return null;
    }
}

public class Response
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
}
