## Generic REST Connector for Qlik Sense
**Note:** This connector is currently in an experimental state. Please feel free to use and abuse it but remember it may not work with certain REST APIs depending on their configuration. I also can't guarantee the quality of the published dictionaries.

### Summary
This connector works by interpreting a definition stored within a "dictionary" in order to retrieve data from the desired REST API. The dictionary contains information about where the REST API lives, the *Authentication* method it uses, how it *Pages* data and what the *Schema* of the different endpoints looks like. 

You can build, publish and download dictionaries using the online [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com).
A catalog of online dictionaries is available directly inside the Connector as well as any downloaded dictionaries.

Both the **Connector** and **Dictionary Factory** are Open Source so feel free to contribute and add new Authentication or Paging capabilites.

### Installation
1. Download the zipped Connector **[here](https://github.com/websy85/generic-rest-connector/raw/master/Build/GenericRestConnector.zip)**.
2. Extract the contents of the zip file to **C:\Program Files\Common Files\Qlik\Custom Data**.
3. Restart the Qlik Sense Engine Service (Server) or restart Qlik Sense Desktop.
4. In the **Load Editor** you should now see a new **Generic REST Connector** option under the **Create New Connection** menu.


### Online Dictionaries
The online dictionary list is comprised of dictionaries built and published via the [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com).

##### Dictionary Factory
![alt text][factory]

##### Connector Online Catalog
![alt text][public]

### Local Dictionaries
All published dictionaries and your own private (unpublished) dictionaries can be downloaded and stored locally to the connector. Local dictionaries should be put into **C:\Program Files\Common Files\Qlik\Custom Data\GenericRESTConnector\configs**. To expose them to the connector -

1. Open the **Load Editor**.
2. Create a new connection, selecting **Generic REST Connector**.
3. Click on the **Local** tab.
4. Click **Update Local Catalog**.

##### Connector Local Catalog
![alt text][local]

### WHERE Clauses
Currently any where clause added in the Load Script will be appended to the url for the call to the REST API. So for example, if you're connecting to Twitter and using the **search/tweets.json** endpoint, you can add a where clause of 

`?q=qlikbranch`

### Additional Info
##### Authentication
Currently the Connector and [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com) facilitates the following authentication methods
* Basic (Username/Password)
* API Key (Usually a parameter passed in via the url)
* OAuth 1.0a
* OAuth 2.0

**Note:** Both the Connector and [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com) provide a workflow for getting an Access Token. If you would like to leverage this you can use `https://rest-dictionary-factory.herokuapp.com/auth/oauth` as your OAuth application redirect uri. 

##### Paging
Currently the Connector and [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com) facilitates the following paging methods
* Page Number
* Offset / Limit
* Url

Enjoy!

[factory]: Factory.png "Dictionary Factory Catalog"
[public]: Public.png "Connector Public Catalog"
[local]: Local.png "Connector Local Catalog"
