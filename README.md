# The retarded Patrick bot

#### TODO

Create your config.json file under `/Patrick` directory and set the file property `Copy to output directory` value to `"Copy always/if newer"`

```js
{
        "Discord": {
                "Token": "<insert discord token here>",
                "TriggerText": "<insert your preferred trigger token here. Mine is '!' (without quotes)>",
                "BotId": <insert your bot id here>,
                "TypingDuration": 0.7,
                "KnownChannels": [
                        <insert server's channel id here or leave empty>
                ],
                "KnownUsers": [
                        <insert discord user id here or leave empty>
                ],
                "Icons": {
                        "CommandIcon": "https://findicons.com/files/icons/127/sleek_xp_software/300/command_prompt.png"
                }
        },
        // this one is optional (remove this comment as well)
        "GitHub": {
                "AppId": "<value>",
                "ClientId": "<value>",
                "ClientSecret": "<value>",
                "RedirectUrl": "<value>",
                "TargetRedirectKey": "code",
                "Scopes": [ "repo", "user", "gist" ]
        }
}
```
