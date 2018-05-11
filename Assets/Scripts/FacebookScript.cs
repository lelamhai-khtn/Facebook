using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using System.Collections.Generic;

public class FacebookScript : MonoBehaviour
{

    public Text FriendsText;
    public Image Avatar;
    private Dictionary<string, string> profile = null;

    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                if (FB.IsInitialized)
                    FB.ActivateApp();
                else
                    Debug.LogError("Couldn't initialize");
            },
            isGameShown =>
            {
                if (!isGameShown)
                    Time.timeScale = 0;
                else
                    Time.timeScale = 1;
            });
        }
        else
            FB.ActivateApp();
    }

    #region Login / Logout
    public void FacebookLogin()
    {
        var permissions = new List<string>() { "public_profile", "email", "user_friends", "publish_actions" , "user_friends" };
        FB.LogInWithReadPermissions(permissions, AuthCallback);
    }
    
    void AuthCallback(IResult result)
    {
        if (FB.IsLoggedIn)
        {
            FB.API("/me?fields=id,name", HttpMethod.GET, DealWithUserName);
            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayPhoto);
            FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, ScoresCallback);
        }
        else
        {
            
        }
    }

    private void DealWithUserName(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.Log("problem with getting profile picture");

            FB.API("/me?fields=id,name", HttpMethod.GET, DealWithUserName);
            return;
        }

        string jsonData = result.RawResult;
        MyClass myObject = new MyClass();
        myObject = JsonUtility.FromJson<MyClass>(jsonData);
        Text UserMsg = FriendsText.GetComponent<Text>();
        UserMsg.text = "Hello, " + myObject.name;
    }



    void DisplayPhoto(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.Log("Error: " + result);

            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayPhoto);
            return;
        }
        Avatar.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
    }



    public void FacebookLogout()
    {
        FB.LogOut();
    }
    #endregion




    private void ScoresCallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.Log("Error: " + result);
            FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, ScoresCallback);
            return;
        }
        Debug.Log(result);
    }
    public void FacebookShare()
    {
        FB.ShareLink(new System.Uri("http://resocoder.com"), "Check it out!",
            "Good programming tutorials lol!",
            new System.Uri("http://resocoder.com/wp-content/uploads/2017/01/logoRound512.png"));
    }

    #region Inviting
    public void FacebookGameRequest()
    {
        FB.AppRequest("Hey! Come and play this awesome game!", title: "Reso Coder Tutorial");
    }

    public void FacebookInvite()
    {
        FB.Mobile.AppInvite(new System.Uri("https://play.google.com/store/apps/details?id=com.tappybyte.byteaway"));
    }
    #endregion

    public void GetFriendsPlayingThisGame()
    {
        string query = "/me/friends";
        FB.API(query, HttpMethod.GET, result =>
        {
            var dictionary = (Dictionary<string, object>)Facebook.MiniJSON.Json.Deserialize(result.RawResult);
            var friendsList = (List<object>)dictionary["data"];
            FriendsText.text = string.Empty;
            foreach (var dict in friendsList)
                FriendsText.text += ((Dictionary<string, object>)dict)["name"];
        });
    }
}