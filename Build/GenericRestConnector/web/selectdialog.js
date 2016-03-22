define(['qvangular'
], function (qvangular) {
    return ['serverside', 'standardSelectDialogService', '$element', 'connectionId', function (serverside, standardSelectDialogService, element, connectionId) {
        var dictionary;
        var contentProvider = {
            getConnectionInfo: function () {
                return {
                    dbusage: false,
                    ownerusage: false,
                    dbfirst: false,
                    specialchars: '',
                    dbseparator: '',
                    defaultdatabase: '',
                    ownerseparator: '',
                    quotesuffix: '',
                    quoteprefix: '',
                    dbmsname: '',
                    keywords: ''
                };
            },
            getDatabases: function () {
                return serverside.sendJsonRequest("getDatabases").then(function (response) {
                    var data = JSON.parse(response.qMessage);
                    dictionary = data.dictionary;
                    return data.qDatabases;
                });
                //return { qDatabases: [{qName:"Not Applicable"}] };
            },
            getOwners: function ( /*databaseName*/) {
                return qvangular.promise([{ name: "" }]);
            },
            getTables: function (databaseName, ownerName) {
                return serverside.sendJsonRequest("getTables", dictionary).then(function (response) {
                    return JSON.parse(response.qMessage);
                });
            },
            getFields: function (databaseName, ownerName, tableName) {
                return serverside.sendJsonRequest("getFields", tableName, dictionary).then(function (response) {
                    return JSON.parse(response.qMessage);
                });
            },
            getPreview: function (databaseName, ownerName, tableName) { //need to add code for preview
                return serverside.sendJsonRequest("getPreview", tableName, dictionary).then(function (response) {
                    return JSON.parse(response.qMessage);
                });


            }
        };

        standardSelectDialogService.showStandardDialog(contentProvider);
    }];
});