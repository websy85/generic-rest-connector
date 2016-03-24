## Generic REST Connector
for Qlik Sense

### Summary
This connector works by interpreting a definition stored within a "dictionary" in order to retrieve data from the desired REST API. The dictionary contains information about where the REST API lives, the *Authentication* method it uses, how it *Pages* data and what the *Schema* of the different endpoints looks like. 

You can build, publish and download dictionaries using the online [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com).
A catalog of online dictionaries is available directly inside the Connector as well as any downloaded dictionaries.

Both the _Connector_ and _Dictionary Factory_ are Open Source so feel free to contribute and add new Authentication or Paging capabilites.

### Installation
1. Download the zipped Connector [here](https://github.com/websy85/generic-rest-connector/raw/master/Build/GenericRestConnector.zip).
2. Extract the contents of the zip file to _C:\Program Files\Common Files\Qlik\Custom Data
3. Restart the Qlik Sense Engine Service (Server) or restart Qlik Sense Desktop
4. In the _Load Editor_ you should no see a new _Generic REST Connector_ option under the _Create New Connection_ menu


### Online Dictionaries
The online dictionary list is comprised of dictionaries built and published via the [Dictionary Factory](https://rest-dictionary-factory.herokuapp.com).

### Local Dictionaries
All published dictionaries and your own private (unpublished) dictionaries can be downloaded and stored locally to the connector. Local dictionaries should be put into _C:\Program Files\Common Files\Qlik\Custom Data\GenericRESTConnector\configs_. To expose them to the connector -
1. Open the _Load Editor_
2. Create a new connection, selecting _Generic REST Connector_
3. Click on the _Local_ tab
4. Click 
