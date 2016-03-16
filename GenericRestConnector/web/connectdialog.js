define(['qvangular', 'text!./connectdialog.ng.html', 'css!./connectdialog.css', 'css!./dictionary-factory.css'], function (qvangular, template) {
    return {
        template: template,
        controller: ['$scope', 'input', function ($scope, input) {
            function init() {
                $scope.isEdit = input.editMode;
                $scope.isLoading = true;
                $scope.auth_method;
                $scope.subpageLoading = true;
                $scope.id = input.instanceId;
                $scope.connectionParameters = {};
                $scope.localDictionaryList = [];
                $scope.onlineDictionaryList = [];
                $scope.source = "online";
                $scope.step = "config-selection";
                $scope.connectionTemplates = {
                    None: "/customdata/64/GenericRestConnector/web/noAuth.ng.html",
                    Basic: "/customdata/64/GenericRestConnector/web/basicAuth.ng.html",
                    "OAuth": "/customdata/64/GenericRestConnector/web/oAuth.ng.html",
                    Certificate: "",
                    "API Key": "/customdata/GenericRestConnector/apiKey.ng.html"
                };
                $scope.name;
                $scope.username;
                $scope.password;
                $scope.token;
                $scope.consumer_secret;
                $scope.key;
                $scope.secret;
                $scope.tokenRequested = false;
                $scope.url;
                $scope.dicurl="";
                $scope.config;
                $scope.provider = "GenericRestConnector.exe";
                $scope.subpage = $scope.connectionTemplates["None"];

                //if the connection is being modified
                if (input.editMode) {
                    input.serverside.getConnection(input.instanceId).then(function (result) {
                        console.log(result);
                        var tempArray = result.qConnection.qConnectionString.substring(19, result.qConnection.qConnectionString.length - 1).split(';');
                        for (var i = 0; i < tempArray.length; i++) {
                            var param = tempArray[i].split('=');
                            $scope.connectionParameters[param[0]] = param[1];
                        }
                        $scope.config = $scope.connectionParameters["config"];
                        $scope.name = result.qConnection.qName;
                        $scope.url = $scope.connectionParameters["url"];
                        $scope.dicurl = $scope.connectionParameters["dictionaryurl"];
                        $scope.dictionaryId = $scope.connectionParameters["dictionary"];;
                        $scope.source = $scope.connectionParameters["source"];
                        $scope.username = $scope.connectionParameters["username"];
                        $scope.isLoading = false;
                        $scope.subpageLoading = true;
                        $scope.loadTemplate('connection-settings', $scope.dictionaryId, $scope.dicurl);
                    });
                }
                else {
                    //get online dictionaries
                    input.serverside.sendJsonRequest("getOnlineDictionaries").then(function (response) {
                        $scope.onlineDictionaryList = JSON.parse(response.qMessage).configs;
                        if ($scope.onlineDictionaryList && $scope.localDictionaryList) {
                            $scope.isLoading = false;
                        }
                    });
                    //get local dictionaries
                    input.serverside.sendJsonRequest("getLocalDictionaries").then(function (response) {
                        var catalog = JSON.parse(response.qMessage);
                        $scope.processCatalog(catalog);
                        if ($scope.onlineDictionaryList && $scope.localDictionaryList) {
                            $scope.isLoading = false;
                        }
                    });
                }
            }

            //build the connection string
            function buildConnectionString() {
                var conn = "CUSTOM CONNECT TO \"provider=GenericRestConnector.exe;dictionary=" + $scope.dictionaryId + ";source=" + $scope.source + ";auth-method="+$scope.auth_method+";";
                $('[data-parameter]').each(function (index, item) {
                    if ($(item).attr("data-parameter") == "password") {
                        $scope.password = $(item).val();
                    }
                    else {
                        conn += $(item).attr("data-parameter") + "=" + $(item).val() + ";";
                    }
                });
                
                if ($scope.dicurl) {
                    conn += "dictionaryurl=" + $scope.dicurl + ";";
                }
                conn += "\"";
                console.log(conn);
                return conn;
            }

            //save the connection
            $scope.onOKClicked = function () {
                if ($scope.isEdit) {
                    input.serverside.modifyConnection($scope.instanceId, $scope.name, buildConnectionString(), $scope.provider, true, $scope.username, $scope.password);
                    $scope.destroyComponent();
                }
                else {
                    input.serverside.createNewConnection($scope.name, buildConnectionString(), $scope.username, $scope.password);
                    $scope.destroyComponent();                 
                }                
            };

            //close the dialog
            $scope.onCancelClicked = function () {
                if (!$scope.isLoading) {
                    $scope.destroyComponent();
                }
            };

            //display the appropriate step
            $scope.nextStep = function (step) {
                $scope.step = step;
            };

            $scope.setSource = function (source) {
                $scope.source = source;
            };

            $scope.updateCatalog = function () {
                input.serverside.sendJsonRequest("updateLocalCatalog").then(function (response) {
                    var catalog = JSON.parse(response.qMessage);
                    $scope.processCatalog(catalog);
                    if ($scope.onlineDictionaryList && $scope.localDictionaryList) {
                        $scope.isLoading = false;
                    }
                });
            };

            //load and display the required template based on the config auth setting
            $scope.loadTemplate = function (step, id, dicurl) {
                this.nextStep(step);
                $scope.dictionaryId = id;
                $scope.dicurl = dicurl;
                input.serverside.sendJsonRequest("getDictionaryDef", id, $scope.source).then(function (response) {
                    $scope.dictionaryDef = JSON.parse(response.qMessage);
                    $scope.auth_method = $scope.dictionaryDef.auth_method;
                    $scope.subpage = $scope.connectionTemplates[$scope.dictionaryDef.auth_method];
                    $scope.subpageLoading = false;
                });
            }

            $scope.processCatalog = function (catalog) {
                $scope.localDictionaryList = [];
                if (catalog.configs) {
                    for (var d in catalog.configs) {
                        $scope.localDictionaryList.push(JSON.parse(catalog.configs[d]));
                    }
                }
            }

            init();
        }]
    };
});