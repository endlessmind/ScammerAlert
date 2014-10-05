This is a small application, with the simple purpose: Helping steam users to avoid scammers.

The application connect directly to steams server, no data is storaged what so ever*.

How the application works:
We have a server setup, that has a database with just one table in it.
This table will only contain the SteamID of a scammer.
When you're logged in to the application, it will cross-reference all your friends (even the ones you haven't accepted as your friend yet) SteamID with that database.
(If you only want to report a scammer you don't have to log in to the application. The login function is only for scanning your friend list for scammers).
If there is a match, it will warn you. Not by a sound or popup, but the message in the application itself will change.
As it only takes about 5 seconds to cross-reference, you don't need to have the app running all the time.
Just start it up when you get weird friend requests.


Adding scammers:
If you get scammed and you want to add the scammer to the database, to help other avoid him/her.
Then you can use the "Report a scammer" button at the bottom of the Application window.
It takes a SteamID in the following format**: STEAM_0:X:XXXXXXXX
When the SteamID has been correctly entered, the application will display the nickname that is currently
applied to that account.
If that checks out, then go ahead and hit "Report" and then you're done.
Now all the users will get warned if that account adds you as friend!

As of today (17/7 2014), there is not many scammers reported.
But we're hoping, that the database will grow as more users report scammers.

The goal with this application, is to prevent scamming in general.
The more users that has this application, and the more scammers that get reported,
the harder and less rewarding it will be for the scammers.

This application was made as a teamwork exercise and problem solving exercise at work.
The jobdescription we got were: Create an application that help in the war against crime :)




PS: If you only want to report a scammer you don't have to log in to the application. The login function is only for scanning your friend list for scammers


*When logging in with SteamGuard, some device identification data (random data, has no meaning) is sent by Steams server.
This data is storage in the same way the Steam application storage it. This is so, then next time you log in, you won't need to
enter a SteamGuard code.


**If you're having trubbles finding the SteamID of the scammer, then just open his/hers profile page.
You can use that URL to get the current SteamID from this site: http://steamidfinder.com/