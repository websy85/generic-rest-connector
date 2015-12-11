define(['qvangular', 'text!./connectdialog.ng.html', 'css!./connectdialog.css', 'css!./dictionary-factory.css'], function (qvangular, template) {
    return {
        template: template,
        controller: ['$scope', 'input', function ($scope, input) {
            function init() {
                $scope.isEdit = input.editMode;
                $scope.id = input.instanceId;
                $scope.connectionParameters = {};
                $scope.localDictionaryList = [];
                $scope.onlineDictionaryList = [];
                $scope.selectedConfigSource = "online";
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
                $scope.key;
                $scope.secret;
                $scope.tokenRequested = false;
                $scope.url;
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
                        $scope.loadTemplate('connection-settings', $scope.config);
                    });
                }

                //getLocalConfigs();


                //get config list
                input.serverside.sendJsonRequest("getOnlineDictionaries").then(function (response) {
                    $scope.onlineDictionaryList = JSON.parse(response.qMessage).configs;
                });
            }

            //build the connection string
            function buildConnectionString() {
                var conn = "CUSTOM CONNECT TO \"provider=GenericRestConnector.exe;dictionary=" + $scope.dictionaryId + ";source=" + $scope.source + ";";
                $('[data-parameter]').each(function (index, item) {
                    conn += $(item).attr("data-parameter") + "=" + $(item).val() + ";";
                });
                conn += "\"";
                console.log(conn);
                return conn;
            }

            //save the connection
            $scope.onOKClicked = function () {
                if ($scope.isEdit) {
                    input.serverside.modifyConnection($scope.instanceId, $scope.name, buildConnectionString(), true, $scope.username, $scope.password);
                }
                else {
                    input.serverside.createNewConnection($scope.name, buildConnectionString(), $scope.username, $scope.password);
                }
                $scope.destroyComponent();
            };

            //close the dialog
            $scope.onCancelClicked = function () {
                if (!$scope.isLoading) {
                    $scope.destroyComponent();
                }
            };

            //authorize an oAuth connection
            $scope.authorize = function () {
                input.serverside.sendJsonRequest("getOAuthAuthorizationUrl", $scope.key, $scope.dictionaryId).then(function (response) {
                    $scope.tokenRequested = true;
                    window.open(response.qMessage, "_blank");
                });
            };

            //display the appropriate step
            $scope.nextStep = function (step) {
                $scope.step = step;
            };

            //load and display the required template based on the config auth setting
            $scope.loadTemplate = function (step, id, source) {
                this.nextStep(step);
                $scope.dictionaryId = id;
                $scope.source = source;
                input.serverside.sendJsonRequest("getDictionaryAuth", id).then(function (response) {
                    $scope.subpage = $scope.connectionTemplates[response.qMessage];
                });
            }

            init();
        }]
    };
});