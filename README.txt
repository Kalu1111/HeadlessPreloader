## License
This program is licensed under the MIT License. You are free to use, modify, and distribute this software for both commercial and non-commercial purposes.

## Usage
Feel free to use this program in your commercial projects without any restrictions. Feedback, and suggestions are always welcome!



## Config file parameter tutorial
{
  "DisplayWebBrowser": false,                                   //true will open a visibel chrome browser window, false will run in headless mode, meaning it will work the same but without displaying a visible browser
  "GlobalSettings": {                                           
    "AwaitMode": 0,                                             //explanation here: https://www.puppeteersharp.com/api/PuppeteerSharp.WaitUntilNavigation.html
    "AdditionalDelaySeconds": 2,                                //once the page finishes loading we'll only move on to the next page after 'AdditionalDelaySeconds' seconds
    "TimeoutSeconds": 0,                                        //defines the maximum time a page can take to load. Anything over it and the load atempt fails. setting it to 0 disables any timeouts
    "MaxAttemptsBeforeSkip": 10                                 //max number of failed retries for a single page until the page gets skipped
  },                                                             
  "WebsiteDomain": "https://www.youtube.com",                   //the website domain (e.g. "https://www.youtube.com"). It can also just be empty as long as you always use the full url in the Url fields below
  "LoginPage": {                                                //you can give me the following login details so that I'm able to login if required during the whole process (e.g. if a page takes so long to load that the authentication cookie expires)
    "Username": "test",                                         //the username of a user with access to all the pages you want me to load
    "Password": "test",                                         //their password
    "UsernameFieldId": "#wf-log-in-email",                      //the id of the text field where I should type the username. (#id)
    "PasswordFieldId": "#wf-log-in-password",                   //the id of the text field where I should type the password. (#id)
    "Url": "https://one.outsystems.com/log-in"                  //Url fields can contain either a site's full url or their relative url in relation to a given domain (e.g. "https://www.youtube.com/watch?v=dQw4w9WgXcQ" or "/watch?v=dQw4w9WgXcQ")
  },                                                             
  "Websites": [                                                 //this list should contain all the webpages you want me to load
    {                                                            
      "Url": "https://one.outsystems.com/log-in"                //Url fields can contain either a site's full url or their relative url in relation to a given domain (e.g. "https://www.youtube.com/watch?v=dQw4w9WgXcQ" or "/watch?v=dQw4w9WgXcQ")
    },                                                           
    {                                                            
      "Url": "/watch?v=dQw4w9WgXcQ",                            //same as the previous one except we're using a path relative to the previously defined WebsiteDomain
      "OptionalSettings": {                                     //OptionalSettings give you a way to override the previously defined GlobalSettings for a specific page
        "AwaitMode": 0,                                         //same
        "AdditionalDelaySeconds": 5,                            //     as
        "TimeoutSeconds": 0,                                    //        the
        "MaxAttemptsBeforeSkip": 15                             //            GlobalSettings
      }                                                          
    },                                                           
    {                                                            
      "Url": "/watch?v=dQw4w9WgXcQ"                             //etc...
    },                                                           
    {                                                            
      "Url": "/watch?v=dQw4w9WgXcQ"                             //etc...
    },                                                           
    {                                                            
      "Url": "https://www.youtube.com/watch?v=dQw4w9WgXcQ"      //etc...
    }                                                            
  ]                                                              
}



## Commands
You have these 7 commands available to you:
--configFile     //allows you to use config files other than the default one (e.g. --configFile C://MyAmazingFolder/FullOfNiceConfigFiles/NiceConfFile.json). If your path has spaces you'll want to use '"'s
--run            //starts the process of loading the webpages you've defined in the config file
--silent         //disables printing messages to the console
--editConfig     //it will open the default config file in your default text editor, so you can access it without having to look for it
--validateConfig //it runs a configuration and login test without loading the rest of the pages defined in the config file.
--readme         //it will open this README file in your default text editor, so you can access it without having to look for it
--help           //will display you this same list of commands

You can string multiple commands together, like HeadlessPreloader --silent --run --configFile C://MyAmazingFolder/FullOfNiceConfigFiles/NiceConfFile.json. The other commands probably don't make much sense strung together but don't let me stop you



## Notes
This was only tested in Windows but it should be able to be run in Linux and macOS as well.
You might need to install .NET 8.0 x64 (https://dotnet.microsoft.com/en-us/download/dotnet/8.0) in your machine in order to run this.
You should also, either run me as an admin or install me in a folder where I'm able to read and write to, since at least during my first run I will have to build a default config file and download a web browser.
I'll also require around 600Mb of your hard drive space once I'm done downloading my web browser.
